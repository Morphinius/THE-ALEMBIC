using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class GoblinBoulderThrowerAI : MonoBehaviour
{
    #region PUBLIC VARIABLES - Настройки в инспекторе
    
    [Header("Основные настройки")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float attackDamage = 15f;
    [SerializeField] private float attackCooldown = 1.5f;
    
    [Header("Здоровье противника")]
    [SerializeField] private float maxHealth = 150f;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool isAlive = true;
    
    [Header("Дистанция атаки (ближняя)")]
    [SerializeField] private float preferredAttackDistance = 2f;
    [SerializeField] private float meleeAttackRange = 3f;
    [SerializeField] private float minAttackDistance = 1f;
    
    [Header("UI - Здоровье")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Image healthFillImage;
    [SerializeField] private Gradient healthGradient;
    [SerializeField] private Canvas healthCanvas;
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0, 3f, 0);
    [SerializeField] private bool showHealthBar = true;
    [SerializeField] private float healthBarFadeTime = 2f;
    [SerializeField] private float healthBarShowDuration = 5f;
    
    [Header("Обнаружение игрока")]
    [SerializeField] private float detectionRadius = 12f;
    [SerializeField] private float fieldOfViewAngle = 100f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;
    
    [Header("Патрулирование")]
    [SerializeField] private List<Transform> patrolPoints = new List<Transform>();
    [SerializeField] private float patrolWaitTime = 3f;
    [SerializeField] private float patrolPointThreshold = 0.5f;
    
    [Header("Атаки - вероятности (в сумме должны быть 100)")]
    [SerializeField] [Range(0, 100)] private int normalAttackChance = 60;
    [SerializeField] [Range(0, 100)] private int boulderThrowChance = 40;
    
    [Header("Атака метанием валуна")]
    [SerializeField] private float boulderThrowMinDistance = 5f;
    [SerializeField] private float boulderThrowMaxDistance = 15f;
    [SerializeField] private float boulderThrowPreferredDistance = 8f;
    [SerializeField] private float boulderThrowDamage = 25f;
    [SerializeField] private float boulderThrowRange = 20f;
    [SerializeField] private float boulderExplosionRadius = 3f; // НОВОЕ: Радиус взрыва валуна
    [SerializeField] private GameObject boulderPrefab;
    [SerializeField] private Transform boulderSpawnPoint;
    [SerializeField] private float boulderThrowSpeed = 15f;
    [SerializeField] private float boulderThrowHeight = 5f;
    [SerializeField] private float boulderThrowCurve = 0.5f; // НОВОЕ: Кривизна траектории (0-1)
    [SerializeField] private float boulderThrowCooldown = 3f;
    
    [Header("Визуальные эффекты валуна")]
    [SerializeField] private GameObject boulderTrailPrefab; // Префаб трейла
    [SerializeField] private Material boulderTrailMaterial; // Материал трейла
    [SerializeField] private Color boulderTrailColor = Color.gray;
    [SerializeField] private float boulderTrailWidth = 0.3f;
    [SerializeField] private float boulderTrailTime = 1f;
    
    [Header("Анимация броска валуна")]
    [SerializeField] private string throwAnimationParameter = "IsThrowing";
    [SerializeField] private float throwAnimationDuration = 1.5f;
    [SerializeField] private float throwStartTime = 0.3f;
    [SerializeField] private float throwReleaseTime = 0.8f;
    [SerializeField] private float throwEndTime = 1.2f;
    
    [Header("Регруппировка")]
    [SerializeField] [Range(0, 100)] private int regroupChance = 20;
    [SerializeField] private float regroupDistance = 6f;
    [SerializeField] private float regroupDuration = 2.5f;
    
    [Header("Возвращение на базу")]
    [SerializeField] private float returnToBaseTime = 5f;
    
    [Header("Анимация")]
    [SerializeField] private string moveAnimationParameter = "IsMoving";
    [SerializeField] private string regroupAnimationParameter = "IsRegroup";
    [SerializeField] private string attackAnimationParameter = "IsAttacking";
    [SerializeField] private string deathAnimationParameter = "IsDeath";
    [SerializeField] private string damageAnimationParameter = "IsDamaged";
    [SerializeField] private float damageAnimationDuration = 0.8f;
    
    [Header("Gizmos (для отладки)")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color detectionColor = Color.yellow;
    [SerializeField] private Color attackColor = Color.red;
    [SerializeField] private Color returnColor = Color.blue;
    [SerializeField] private Color preferredDistanceColor = Color.green;
    [SerializeField] private Color boulderThrowDistanceColor = Color.magenta;
    
    #endregion
    
    #region ENUM DEFINITIONS - Определения перечислений
    
    public enum AIState { Idle, Patrol, Chase, Attack, Regroup, Dead, ReturnToBase }
    
    #endregion
    
    #region PRIVATE VARIABLES - Внутренние переменные
    
    private Transform player;
    private Rigidbody rb;
    private Animator Goblin_Animator;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private bool isMoving = false;
    private bool isRegroup = false;
    private bool isAttackingAnim = false;
    private bool isThrowingAnim = false;
    private bool isDeathAnim = false;
    private bool isDamagedAnim = false;
    private bool wasMoving = false;
    private bool wasRegroup = false;
    private bool wasAttacking = false;
    private bool wasThrowing = false;
    private bool wasDeath = false;
    private bool wasDamaged = false;
    
    private float healthBarTimer = 0f;
    private bool healthBarVisible = false;
    private CanvasGroup healthCanvasGroup;
    
    public delegate void HealthChangedDelegate(float currentHealth, float maxHealth);
    public event HealthChangedDelegate OnHealthChanged;
    
    public delegate void EnemyDiedDelegate();
    public event EnemyDiedDelegate OnEnemyDied;
    
    private AIState currentState = AIState.Patrol;
    
    private float attackTimer = 0f;
    private float patrolTimer = 0f;
    private float regroupTimer = 0f;
    private float playerLostTimer = 0f;
    private float damageAnimationTimer = 0f;
    private float boulderThrowTimer = 0f;
    private bool isInDamageAnimation = false;
    
    private int currentPatrolIndex = 0;
    private bool isPatrolForward = true;
    
    private bool isAttacking = false;
    private bool isRegrouping = false;
    private bool isReturningToBase = false;
    
    private int attackCounter = 0;
    
    private Vector3 lastKnownPlayerPosition;
    private bool hasLastKnownPosition = false;
    
    #endregion
    
    #region PROPERTIES - Свойства для доступа из других скриптов
    
    public bool IsAlive => isAlive;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
    public Transform Player => player;
    public bool IsAttacking => isAttacking;
    public int AttackCounter => attackCounter;
    public bool IsMoving => isMoving;
    public AIState CurrentAIState => currentState;
    
    #endregion
    
    #region UNITY CALLBACKS - Методы Unity
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        Goblin_Animator = GetComponent<Animator>();
        startPosition = transform.position;
        startRotation = transform.rotation;
        
        currentHealth = maxHealth;
        isAlive = true;
        
        FindPlayer();
        
        if (patrolPoints.Count == 0)
        {
            GameObject startPoint = new GameObject("StartPatrolPoint");
            startPoint.transform.position = startPosition;
            startPoint.transform.SetParent(transform.parent);
            patrolPoints.Add(startPoint.transform);
        }
        
        InitializeAnimator();
        InitializeHealthBar();
        
        SwitchState(AIState.Patrol);
    }
    
    private void Update()
    {
        if (!isAlive) return;
        
        UpdateTimers();
        StateMachineUpdate();
        UpdateAnimator();
        UpdateHealthBar();
        UpdateDamageAnimation();
    }
    
    private void FixedUpdate()
    {
        if (!isAlive) return;
        
        StateMachineFixedUpdate();
    }
    
    private void LateUpdate()
    {
        UpdateHealthBarPosition();
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        
        Gizmos.color = detectionColor;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        Gizmos.color = attackColor;
        Gizmos.DrawWireSphere(transform.position, meleeAttackRange);
        
        Gizmos.color = preferredDistanceColor;
        Gizmos.DrawWireSphere(transform.position, preferredAttackDistance);
        
        Gizmos.color = boulderThrowDistanceColor;
        Gizmos.DrawWireSphere(transform.position, boulderThrowPreferredDistance);
        Gizmos.DrawWireSphere(transform.position, boulderThrowMinDistance);
        Gizmos.DrawWireSphere(transform.position, boulderThrowMaxDistance);
        
        Gizmos.color = returnColor;
        Gizmos.DrawSphere(startPosition, 0.5f);
        Gizmos.DrawWireSphere(startPosition, 1f);
        
        Vector3 fovLine1 = Quaternion.Euler(0, fieldOfViewAngle / 2, 0) * transform.forward * detectionRadius;
        Vector3 fovLine2 = Quaternion.Euler(0, -fieldOfViewAngle / 2, 0) * transform.forward * detectionRadius;
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, fovLine1);
        Gizmos.DrawRay(transform.position, fovLine2);
        
        if (player != null && CanSeePlayer())
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }
        
        if (hasLastKnownPosition)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(lastKnownPlayerPosition, 0.3f);
            Gizmos.DrawWireSphere(lastKnownPlayerPosition, 0.5f);
        }
        
        Gizmos.color = Color.green;
        for (int i = 0; i < patrolPoints.Count; i++)
        {
            if (patrolPoints[i] != null)
            {
                Gizmos.DrawSphere(patrolPoints[i].position, 0.5f);
                if (i < patrolPoints.Count - 1 && patrolPoints[i + 1] != null)
                {
                    Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                }
            }
        }
    }
    
    #endregion
    
    #region HEALTH BAR SYSTEM - Система HP бара
    
    private void InitializeHealthBar()
    {
        if (healthCanvas == null)
        {
            healthCanvas = GetComponentInChildren<Canvas>();
            
            if (healthCanvas == null)
            {
                CreateHealthBarUI();
            }
        }
        
        if (healthCanvas != null)
        {
            healthCanvasGroup = healthCanvas.GetComponent<CanvasGroup>();
            if (healthCanvasGroup == null)
            {
                healthCanvasGroup = healthCanvas.gameObject.AddComponent<CanvasGroup>();
            }
            
            if (showHealthBar)
            {
                ShowHealthBar(true);
                healthBarVisible = true;
                healthBarTimer = healthBarShowDuration;
            }
            else
            {
                ShowHealthBar(false);
                healthBarVisible = false;
            }
            
            if (healthSlider != null)
            {
                healthSlider.minValue = 0;
                healthSlider.maxValue = maxHealth;
                healthSlider.value = currentHealth;
                
                if (healthFillImage != null)
                {
                    float healthPercentage = currentHealth / maxHealth;
                    healthFillImage.color = healthGradient.Evaluate(healthPercentage);
                }
            }
        }
    }
    
    private void CreateHealthBarUI()
    {
        GameObject canvasObject = new GameObject("HealthBarCanvas");
        canvasObject.transform.SetParent(transform);
        canvasObject.transform.localPosition = healthBarOffset;
        canvasObject.transform.localRotation = Quaternion.identity;
        canvasObject.transform.localScale = Vector3.one;
        
        healthCanvas = canvasObject.AddComponent<Canvas>();
        healthCanvas.renderMode = RenderMode.WorldSpace;
        
        CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
        canvasScaler.dynamicPixelsPerUnit = 10;
        
        GameObject sliderObject = new GameObject("HealthSlider");
        sliderObject.transform.SetParent(canvasObject.transform);
        sliderObject.transform.localPosition = Vector3.zero;
        sliderObject.transform.localRotation = Quaternion.identity;
        sliderObject.transform.localScale = new Vector3(0.2f, 0.03f, 1f);
        
        healthSlider = sliderObject.AddComponent<Slider>();
        
        GameObject backgroundObject = new GameObject("Background");
        backgroundObject.transform.SetParent(sliderObject.transform);
        backgroundObject.transform.localPosition = Vector3.zero;
        backgroundObject.transform.localRotation = Quaternion.identity;
        backgroundObject.transform.localScale = Vector3.one;
        
        Image backgroundImage = backgroundObject.AddComponent<Image>();
        backgroundImage.color = Color.gray;
        
        RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
        backgroundRect.anchorMin = Vector2.zero;
        backgroundRect.anchorMax = Vector2.one;
        backgroundRect.offsetMin = Vector2.zero;
        backgroundRect.offsetMax = Vector2.zero;
        
        GameObject fillAreaObject = new GameObject("Fill Area");
        fillAreaObject.transform.SetParent(sliderObject.transform);
        fillAreaObject.transform.localPosition = Vector3.zero;
        fillAreaObject.transform.localRotation = Quaternion.identity;
        fillAreaObject.transform.localScale = Vector3.one;
        
        RectTransform fillAreaRect = fillAreaObject.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1, 0.75f);
        fillAreaRect.offsetMin = new Vector2(5, 0);
        fillAreaRect.offsetMax = new Vector2(-5, 0);
        
        GameObject fillObject = new GameObject("Fill");
        fillObject.transform.SetParent(fillAreaObject.transform);
        fillObject.transform.localPosition = Vector3.zero;
        fillObject.transform.localRotation = Quaternion.identity;
        fillObject.transform.localScale = Vector3.one;
        
        healthFillImage = fillObject.AddComponent<Image>();
        healthFillImage.color = Color.green;
        
        RectTransform fillRect = fillObject.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        healthSlider.fillRect = fillRect;
        healthSlider.targetGraphic = healthFillImage;
        healthSlider.direction = Slider.Direction.LeftToRight;
        
        healthCanvasGroup = canvasObject.AddComponent<CanvasGroup>();
        
        UpdateHealthBarValue();
    }
    
    private void UpdateHealthBarPosition()
    {
        if (healthCanvas != null && healthCanvasGroup != null && healthCanvasGroup.alpha > 0.01f)
        {
            healthCanvas.transform.position = transform.position + healthBarOffset;
            
            if (Camera.main != null)
            {
                healthCanvas.transform.LookAt(healthCanvas.transform.position + Camera.main.transform.rotation * Vector3.forward,
                    Camera.main.transform.rotation * Vector3.up);
            }
        }
    }
    
    private void UpdateHealthBarValue()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
            
            if (healthFillImage != null)
            {
                float healthPercentage = currentHealth / maxHealth;
                healthFillImage.color = healthGradient.Evaluate(healthPercentage);
            }
        }
    }
    
    private void ShowHealthBar(bool show)
    {
        if (healthCanvasGroup != null)
        {
            healthCanvasGroup.alpha = show ? 1f : 0f;
            healthBarVisible = show;
        }
    }
    
    private void ShowHealthBarTemporarily(float duration)
    {
        if (!showHealthBar) return;
        
        healthBarTimer = duration;
        ShowHealthBar(true);
        healthBarVisible = true;
    }
    
    private void UpdateHealthBar()
    {
        if (!showHealthBar || !healthBarVisible || healthCanvasGroup == null) return;
        
        if (healthBarTimer > 0)
        {
            healthBarTimer -= Time.deltaTime;
            
            if (healthBarTimer <= healthBarFadeTime)
            {
                healthCanvasGroup.alpha = Mathf.Lerp(0f, 1f, healthBarTimer / healthBarFadeTime);
            }
        }
        else if (healthCanvasGroup.alpha > 0)
        {
            ShowHealthBar(false);
        }
    }
    
    #endregion
    
    #region ANIMATION SYSTEM - Система анимации
    
    private void InitializeAnimator()
    {
        if (Goblin_Animator == null) return;
        
        foreach (AnimatorControllerParameter param in Goblin_Animator.parameters)
        {
            if (param.name == moveAnimationParameter && param.type == AnimatorControllerParameterType.Bool) { }
            if (param.name == regroupAnimationParameter && param.type == AnimatorControllerParameterType.Bool) { }
            if (param.name == attackAnimationParameter && param.type == AnimatorControllerParameterType.Bool) { }
            if (param.name == throwAnimationParameter && param.type == AnimatorControllerParameterType.Bool) { }
            if (param.name == deathAnimationParameter && param.type == AnimatorControllerParameterType.Bool) { }
            if (param.name == damageAnimationParameter && param.type == AnimatorControllerParameterType.Bool) { }
        }
    }
    
    private void UpdateAnimator()
    {
        if (Goblin_Animator == null) return;
        
        if (isInDamageAnimation)
        {
            Goblin_Animator.SetBool(moveAnimationParameter, false);
            Goblin_Animator.SetBool(regroupAnimationParameter, false);
            Goblin_Animator.SetBool(attackAnimationParameter, false);
            Goblin_Animator.SetBool(throwAnimationParameter, false);
            Goblin_Animator.SetBool(deathAnimationParameter, false);
            
            if (!wasDamaged || isDamagedAnim != wasDamaged)
            {
                Goblin_Animator.SetBool(damageAnimationParameter, true);
                wasDamaged = true;
            }
            
            wasMoving = false;
            wasRegroup = false;
            wasAttacking = false;
            wasThrowing = false;
            wasDeath = false;
            return;
        }
        
        if (isMoving != wasMoving)
        {
            Goblin_Animator.SetBool(moveAnimationParameter, isMoving);
            wasMoving = isMoving;
        }
        
        if (isRegroup != wasRegroup)
        {
            Goblin_Animator.SetBool(regroupAnimationParameter, isRegroup);
            wasRegroup = isRegroup;
        }
        
        if (isAttackingAnim != wasAttacking)
        {
            Goblin_Animator.SetBool(attackAnimationParameter, isAttackingAnim);
            wasAttacking = isAttackingAnim;
        }
        
        if (isThrowingAnim != wasThrowing)
        {
            Goblin_Animator.SetBool(throwAnimationParameter, isThrowingAnim);
            wasThrowing = isThrowingAnim;
        }
        
        if (isDeathAnim != wasDeath)
        {
            Goblin_Animator.SetBool(deathAnimationParameter, isDeathAnim);
            wasDeath = isDeathAnim;
        }
        
        if (isDamagedAnim != wasDamaged)
        {
            Goblin_Animator.SetBool(damageAnimationParameter, isDamagedAnim);
            wasDamaged = isDamagedAnim;
        }
    }
    
    private void UpdateDamageAnimation()
    {
        if (isDamagedAnim && damageAnimationTimer > 0)
        {
            damageAnimationTimer -= Time.deltaTime;
            isInDamageAnimation = true;
            
            if (damageAnimationTimer <= 0)
            {
                EndDamageAnimation();
            }
        }
        else if (isInDamageAnimation && damageAnimationTimer <= 0)
        {
            EndDamageAnimation();
        }
    }
    
    private void EndDamageAnimation()
    {
        isDamagedAnim = false;
        isInDamageAnimation = false;
        
        if (Goblin_Animator != null)
        {
            Goblin_Animator.SetBool(damageAnimationParameter, false);
            wasDamaged = false;
        }
    }
    
    private void StartDamageAnimation()
    {
        isDamagedAnim = true;
        isInDamageAnimation = true;
        damageAnimationTimer = damageAnimationDuration;
        
        isMoving = false;
        isRegroup = false;
        isAttackingAnim = false;
        isThrowingAnim = false;
        
        if (Goblin_Animator != null)
        {
            Goblin_Animator.SetBool(damageAnimationParameter, true);
            Goblin_Animator.SetBool(moveAnimationParameter, false);
            Goblin_Animator.SetBool(regroupAnimationParameter, false);
            Goblin_Animator.SetBool(attackAnimationParameter, false);
            Goblin_Animator.SetBool(throwAnimationParameter, false);
            
            wasDamaged = true;
            wasMoving = false;
            wasRegroup = false;
            wasAttacking = false;
            wasThrowing = false;
        }
    }
    
    #endregion
    
    #region HEALTH SYSTEM - Система здоровья
    
    public void TakeDamage(float damage)
    {
        if (!isAlive) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        StartDamageAnimation();
        
        UpdateHealthBarValue();
        ShowHealthBarTemporarily(healthBarShowDuration);
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        if (currentState != AIState.Chase && currentState != AIState.Attack && currentState != AIState.Dead)
        {
            SwitchState(AIState.Chase);
        }
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        isAlive = false;
        isMoving = false;
        isRegroup = false;
        isAttackingAnim = false;
        isThrowingAnim = false;
        isDeathAnim = true;
        isDamagedAnim = false;
        isInDamageAnimation = false;
        
        ShowHealthBar(false);
        SwitchState(AIState.Dead);
        
        UpdateAnimator();
        OnEnemyDied?.Invoke();
    }
    
    #endregion
    
    #region PLAYER INTERACTION - Взаимодействие с игроком
    
    public void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }
    
    #endregion
    
    #region ATTACK SYSTEM - Система атаки
    
    private void DamagePlayer()
    {
        if (player == null) return;
        
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }
    }
    
    private bool CanDamagePlayer()
    {
        if (player == null) return false;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        return distanceToPlayer <= meleeAttackRange;
    }
    
    private bool IsAtOptimalBoulderThrowDistance()
    {
        if (player == null) return false;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        return distanceToPlayer >= boulderThrowMinDistance && distanceToPlayer <= boulderThrowMaxDistance;
    }
    
    private bool IsInMeleeAttackRange()
    {
        if (player == null) return false;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        return distanceToPlayer <= meleeAttackRange;
    }
    
    #endregion
    
    #region BOULDER THROW SYSTEM - Система метания валуна
    
    private void ThrowBoulder()
    {
        if (boulderPrefab == null || player == null) return;
        
        Vector3 targetPosition = player.position;
        Vector3 spawnPosition = boulderSpawnPoint != null ? boulderSpawnPoint.position : transform.position + Vector3.up * 2f;
        
        GameObject boulder = Instantiate(boulderPrefab, spawnPosition, Quaternion.identity);
        
        // Добавляем компонент для управления полетом по кривой
        BoulderFlightController flightController = boulder.AddComponent<BoulderFlightController>();
        flightController.Initialize(
            spawnPosition,
            targetPosition,
            boulderThrowSpeed,
            boulderThrowHeight,
            boulderThrowCurve
        );
        
        // Добавляем компонент для урона
        BoulderProjectile boulderProjectile = boulder.GetComponent<BoulderProjectile>();
        if (boulderProjectile == null)
        {
            boulderProjectile = boulder.AddComponent<BoulderProjectile>();
        }
        
        boulderProjectile.SetDamage(boulderThrowDamage);
        boulderProjectile.SetRange(boulderThrowRange);
        boulderProjectile.SetExplosionRadius(boulderExplosionRadius); // Устанавливаем радиус взрыва
        
        boulderProjectile.SetTrailProperties(boulderTrailPrefab, boulderTrailMaterial, boulderTrailColor, boulderTrailWidth, boulderTrailTime);
        
        Debug.Log($"Бросаю валун! Скорость: {boulderThrowSpeed}, Высота: {boulderThrowHeight}, Кривизна: {boulderThrowCurve}, Радиус взрыва: {boulderExplosionRadius}");
    }
    
    #endregion
    
    #region STATE MACHINE - Конечный автомат состояний
    
    private void StateMachineUpdate()
    {
        switch (currentState)
        {
            case AIState.Idle: IdleUpdate(); break;
            case AIState.Patrol: PatrolUpdate(); break;
            case AIState.Chase: ChaseUpdate(); break;
            case AIState.Attack: AttackUpdate(); break;
            case AIState.Regroup: RegroupUpdate(); break;
            case AIState.Dead: DeadUpdate(); break;
            case AIState.ReturnToBase: ReturnToBaseUpdate(); break;
        }
    }
    
    private void StateMachineFixedUpdate()
    {
        switch (currentState)
        {
            case AIState.Chase: ChaseFixedUpdate(); break;
            case AIState.Regroup: RegroupFixedUpdate(); break;
            case AIState.Patrol: PatrolFixedUpdate(); break;
            case AIState.ReturnToBase: ReturnToBaseFixedUpdate(); break;
        }
    }
    
    private void SwitchState(AIState newState)
    {
        switch (currentState)
        {
            case AIState.Patrol: patrolTimer = 0f; break;
            case AIState.Attack: 
                isAttacking = false;
                isAttackingAnim = false;
                isThrowingAnim = false;
                StopAllCoroutines();
                break;
            case AIState.Regroup: 
                isRegrouping = false;
                isRegroup = false;
                regroupTimer = 0f;
                break;
            case AIState.Chase: playerLostTimer = 0f; break;
            case AIState.ReturnToBase: isReturningToBase = false; break;
        }
        
        switch (newState)
        {
            case AIState.Patrol: 
                currentPatrolIndex = GetNearestPatrolPointIndex();
                hasLastKnownPosition = false;
                break;
            case AIState.Regroup: StartRegroup(); break;
            case AIState.Dead: OnDeath(); break;
            case AIState.ReturnToBase: StartReturnToBase(); break;
        }
        
        currentState = newState;
    }
    
    private void IdleUpdate()
    {
        if (CanSeePlayer())
        {
            SwitchState(AIState.Chase);
        }
    }
    
    private void RegroupUpdate()
    {
        if (!isRegrouping) return;
        
        regroupTimer += Time.deltaTime;
        
        if (regroupTimer >= regroupDuration)
        {
            SwitchState(AIState.Chase);
        }
    }
    
    private void DeadUpdate() { }
    
    private void ReturnToBaseUpdate()
    {
        if (CanSeePlayer())
        {
            SwitchState(AIState.Chase);
            return;
        }
        
        float distanceToStart = Vector3.Distance(transform.position, startPosition);
        
        if (distanceToStart <= patrolPointThreshold)
        {
            isMoving = false;
            isRegroup = false;
            isAttackingAnim = false;
            isThrowingAnim = false;
            isDeathAnim = false;
            
            transform.rotation = Quaternion.Lerp(transform.rotation, startRotation, 
                rotationSpeed * Time.deltaTime);
            
            if (Quaternion.Angle(transform.rotation, startRotation) < 5f)
            {
                SwitchState(AIState.Patrol);
            }
        }
    }
    
    private void OnDeath()
    {
        isMoving = false;
        isRegroup = false;
        isAttackingAnim = false;
        isThrowingAnim = false;
        isDeathAnim = true;
        isDamagedAnim = false;
        isInDamageAnimation = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        UpdateAnimator();
        
        Collider enemyCollider = GetComponent<Collider>();
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }
    }
    
    private void StartRegroup()
    {
        isRegrouping = true;
        isRegroup = true;
        isAttackingAnim = false;
        isThrowingAnim = false;
        isDeathAnim = false;
        regroupTimer = 0f;
    }
    
    private void StartReturnToBase()
    {
        isReturningToBase = true;
        playerLostTimer = 0f;
        hasLastKnownPosition = false;
    }
    
    #endregion
    
    #region PLAYER DETECTION - Обнаружение игрока
    
    private bool CanSeePlayer()
    {
        if (player == null) return false;
        
        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        
        if (distanceToPlayer > detectionRadius) return false;
        
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer > fieldOfViewAngle / 2) return false;
        
        if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, 
            distanceToPlayer, obstacleLayer))
        {
            return false;
        }
        
        return true;
    }
    
    private bool IsPlayerInAttackRange()
    {
        if (player == null) return false;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool inBoulderThrowRange = distanceToPlayer >= boulderThrowMinDistance && distanceToPlayer <= boulderThrowMaxDistance;
        bool inMeleeRange = distanceToPlayer <= meleeAttackRange;
        
        return inBoulderThrowRange || inMeleeRange;
    }
    
    private bool IsPlayerTooFar()
    {
        if (player == null) return true;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        return distanceToPlayer > detectionRadius * 2f;
    }
    
    #endregion
    
    #region PATROL - Патрулирование
    
    private void PatrolUpdate()
    {
        if (CanSeePlayer())
        {
            SwitchState(AIState.Chase);
            return;
        }
        
        if (patrolPoints.Count == 0) return;
        
        Transform targetPoint = patrolPoints[currentPatrolIndex];
        if (targetPoint == null) return;
        
        float distanceToPoint = Vector3.Distance(transform.position, targetPoint.position);
        
        if (distanceToPoint <= patrolPointThreshold)
        {
            patrolTimer += Time.deltaTime;
            
            if (patrolTimer >= patrolWaitTime)
            {
                patrolTimer = 0f;
                MoveToNextPatrolPoint();
            }
        }
    }
    
    private void PatrolFixedUpdate()
    {
        if (patrolPoints.Count == 0 || isInDamageAnimation) return;
        
        Transform targetPoint = patrolPoints[currentPatrolIndex];
        if (targetPoint == null) return;
        
        float distanceToPoint = Vector3.Distance(transform.position, targetPoint.position);
        
        if (distanceToPoint > patrolPointThreshold && !isInDamageAnimation)
        {
            isMoving = true;
            isRegroup = false;
            isAttackingAnim = false;
            isThrowingAnim = false;
            isDeathAnim = false;
            MoveTowards(targetPoint.position);
            RotateTowards(targetPoint.position);
        }
        else
        {
            isMoving = false;
            isRegroup = false;
            isAttackingAnim = false;
            isThrowingAnim = false;
            isDeathAnim = false;
        }
    }
    
    private void MoveToNextPatrolPoint()
    {
        if (patrolPoints.Count <= 1) return;
        
        if (isPatrolForward)
        {
            currentPatrolIndex++;
            if (currentPatrolIndex >= patrolPoints.Count)
            {
                currentPatrolIndex = patrolPoints.Count - 2;
                isPatrolForward = false;
            }
        }
        else
        {
            currentPatrolIndex--;
            if (currentPatrolIndex < 0)
            {
                currentPatrolIndex = 1;
                isPatrolForward = true;
            }
        }
    }
    
    private int GetNearestPatrolPointIndex()
    {
        int nearestIndex = 0;
        float nearestDistance = Mathf.Infinity;
        
        for (int i = 0; i < patrolPoints.Count; i++)
        {
            if (patrolPoints[i] == null) continue;
            
            float distance = Vector3.Distance(transform.position, patrolPoints[i].position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestIndex = i;
            }
        }
        
        return nearestIndex;
    }
    
    #endregion
    
    #region CHASE - Преследование
    
    private void ChaseUpdate()
    {
        if (player == null)
        {
            SwitchState(AIState.Patrol);
            return;
        }
        
        if (CanSeePlayer())
        {
            lastKnownPlayerPosition = player.position;
            hasLastKnownPosition = true;
            playerLostTimer = 0f;
        }
        else
        {
            playerLostTimer += Time.deltaTime;
            
            if (playerLostTimer >= returnToBaseTime)
            {
                SwitchState(AIState.Patrol);
                return;
            }
            
            if (IsPlayerTooFar())
            {
                SwitchState(AIState.Patrol);
                return;
            }
        }
        
        if (IsPlayerInAttackRange())
        {
            if (attackTimer <= 0f)
            {
                SwitchState(AIState.Attack);
            }
        }
    }
    
    private void ChaseFixedUpdate()
    {
        if (player == null || isInDamageAnimation)
        {
            if (isInDamageAnimation)
            {
                isMoving = false;
                isRegroup = false;
                isAttackingAnim = false;
                isThrowingAnim = false;
                isDeathAnim = false;
            }
            return;
        }
        
        if (CanSeePlayer())
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            bool shouldUseBoulderThrow = IsAtOptimalBoulderThrowDistance();
            bool shouldUseMeleeAttack = IsInMeleeAttackRange();
            
            if (shouldUseBoulderThrow && !shouldUseMeleeAttack)
            {
                if (distanceToPlayer > boulderThrowPreferredDistance + 0.5f)
                {
                    isMoving = true;
                    isRegroup = false;
                    isAttackingAnim = false;
                    isThrowingAnim = false;
                    isDeathAnim = false;
                    MoveTowards(player.position);
                    RotateTowards(player.position);
                }
                else if (distanceToPlayer < boulderThrowMinDistance - 0.5f)
                {
                    isMoving = true;
                    isRegroup = false;
                    isAttackingAnim = false;
                    isThrowingAnim = false;
                    isDeathAnim = false;
                    MoveAwayFrom(player.position);
                    RotateTowards(player.position);
                }
                else
                {
                    isMoving = false;
                    isRegroup = false;
                    isAttackingAnim = false;
                    isThrowingAnim = false;
                    isDeathAnim = false;
                    RotateTowards(player.position);
                }
            }
            else if (shouldUseMeleeAttack)
            {
                if (distanceToPlayer > preferredAttackDistance + 0.2f)
                {
                    isMoving = true;
                    isRegroup = false;
                    isAttackingAnim = false;
                    isThrowingAnim = false;
                    isDeathAnim = false;
                    MoveTowards(player.position);
                    RotateTowards(player.position);
                }
                else if (distanceToPlayer < preferredAttackDistance - 0.2f)
                {
                    isMoving = true;
                    isRegroup = false;
                    isAttackingAnim = false;
                    isThrowingAnim = false;
                    isDeathAnim = false;
                    MoveAwayFrom(player.position);
                    RotateTowards(player.position);
                }
                else
                {
                    isMoving = false;
                    isRegroup = false;
                    isAttackingAnim = false;
                    isThrowingAnim = false;
                    isDeathAnim = false;
                    RotateTowards(player.position);
                }
            }
            else
            {
                isMoving = true;
                isRegroup = false;
                isAttackingAnim = false;
                isThrowingAnim = false;
                isDeathAnim = false;
                MoveTowards(player.position);
                RotateTowards(player.position);
            }
        }
        else if (hasLastKnownPosition)
        {
            isMoving = true;
            isRegroup = false;
            isAttackingAnim = false;
            isThrowingAnim = false;
            isDeathAnim = false;
            MoveTowards(lastKnownPlayerPosition);
            RotateTowards(lastKnownPlayerPosition);
            
            float distanceToLastPosition = Vector3.Distance(transform.position, lastKnownPlayerPosition);
            if (distanceToLastPosition < 1f)
            {
                isMoving = false;
                isRegroup = false;
                isAttackingAnim = false;
                isThrowingAnim = false;
                isDeathAnim = false;
            }
        }
        else
        {
            isMoving = false;
            isRegroup = false;
            isAttackingAnim = false;
            isThrowingAnim = false;
            isDeathAnim = false;
        }
    }
    
    #endregion
    
    #region ATTACK - Атака
    
    private void AttackUpdate()
    {
        if (player == null || isInDamageAnimation)
        {
            SwitchState(AIState.Chase);
            return;
        }
        
        if (!isAttacking)
        {
            StartAttack();
        }
        
        RotateTowards(player.position);
        
        isMoving = false;
        isRegroup = false;
        isDeathAnim = false;
    }
    
    private void StartAttack()
    {
        isAttacking = true;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        bool inBoulderThrowZone = distanceToPlayer >= boulderThrowMinDistance && 
                                 distanceToPlayer <= boulderThrowMaxDistance;
        
        bool inMeleeZone = distanceToPlayer <= meleeAttackRange;
        
        bool canThrowBoulder = boulderThrowTimer <= 0 && boulderPrefab != null;
        
        if (inBoulderThrowZone && canThrowBoulder && !inMeleeZone)
        {
            int randomValue = Random.Range(0, 100);
            if (randomValue < boulderThrowChance)
            {
                StartCoroutine(BoulderThrowAttack());
            }
            else
            {
                isAttacking = false;
                SwitchState(AIState.Chase);
                return;
            }
        }
        else if (inMeleeZone)
        {
            StartCoroutine(NormalAttack());
        }
        else
        {
            isAttacking = false;
            SwitchState(AIState.Chase);
            return;
        }
        
        attackTimer = attackCooldown;
    }
    
    private IEnumerator NormalAttack()
    {
        isAttackingAnim = true;
        isThrowingAnim = false;
        isDeathAnim = false;
        
        yield return new WaitForSeconds(0.3f);
        
        if (CanDamagePlayer())
        {
            DamagePlayer();
        }
        
        yield return new WaitForSeconds(0.2f);
        
        isAttackingAnim = false;
        
        attackCounter++;
        
        if (attackCounter >= 3)
        {
            attackCounter = 0;
            
            if (Random.Range(0, 100) < regroupChance)
            {
                SwitchState(AIState.Regroup);
            }
            else
            {
                SwitchState(AIState.Chase);
            }
        }
        else
        {
            SwitchState(AIState.Chase);
        }
    }
    
    private IEnumerator BoulderThrowAttack()
    {
        boulderThrowTimer = boulderThrowCooldown;
        
        isThrowingAnim = true;
        isAttackingAnim = false;
        isDeathAnim = false;
        
        yield return new WaitForSeconds(throwStartTime);
        yield return new WaitForSeconds(throwReleaseTime - throwStartTime);
        
        ThrowBoulder();
        
        yield return new WaitForSeconds(throwEndTime - throwReleaseTime);
        
        isThrowingAnim = false;
        
        attackCounter++;
        
        if (attackCounter >= 3)
        {
            attackCounter = 0;
            
            if (Random.Range(0, 100) < regroupChance)
            {
                SwitchState(AIState.Regroup);
            }
            else
            {
                SwitchState(AIState.Chase);
            }
        }
        else
        {
            SwitchState(AIState.Chase);
        }
    }
    
    public void ApplyAttackDamage()
    {
        if (CanDamagePlayer())
        {
            DamagePlayer();
        }
    }
    
    public void ReleaseBoulder()
    {
        ThrowBoulder();
    }
    
    #endregion
    
    #region REGROUP - Регруппировка
    
    private void RegroupFixedUpdate()
    {
        if (player == null || !isRegrouping || isInDamageAnimation)
        {
            isMoving = false;
            isRegroup = false;
            isAttackingAnim = false;
            isThrowingAnim = false;
            isDeathAnim = false;
            return;
        }
        
        Vector3 directionFromPlayer = (transform.position - player.position).normalized;
        Vector3 targetPosition = transform.position + directionFromPlayer * regroupDistance;
        
        isMoving = true;
        isRegroup = true;
        isAttackingAnim = false;
        isThrowingAnim = false;
        isDeathAnim = false;
        
        MoveTowards(targetPosition);
        RotateTowards(targetPosition);
    }
    
    #endregion
    
    #region RETURN TO BASE - Возвращение на базу
    
    private void ReturnToBaseFixedUpdate()
    {
        if (isInDamageAnimation) return;
        
        isMoving = true;
        isRegroup = false;
        isAttackingAnim = false;
        isThrowingAnim = false;
        isDeathAnim = false;
        MoveTowards(startPosition);
        RotateTowards(startPosition);
    }
    
    #endregion
    
    #region IDLE - Бездействие
    
    #endregion
    
    #region MOVEMENT - Система перемещения
    
    private void MoveTowards(Vector3 targetPosition)
    {
        if (!isAlive || isInDamageAnimation) return;
        
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        
        float distance = Vector3.Distance(transform.position, targetPosition);
        
        if (distance > patrolPointThreshold)
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }
    
    private void MoveAwayFrom(Vector3 targetPosition)
    {
        if (!isAlive || isInDamageAnimation) return;
        
        Vector3 direction = (transform.position - targetPosition).normalized;
        direction.y = 0;
        
        float distance = Vector3.Distance(transform.position, targetPosition);
        
        if (distance < preferredAttackDistance - 0.2f)
        {
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }
    
    private void RotateTowards(Vector3 targetPosition)
    {
        if (!isAlive || isInDamageAnimation) return;
        
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        
        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 
                rotationSpeed * Time.deltaTime);
        }
    }
    
    #endregion
    
    #region UTILITY - Вспомогательные методы
    
    private void UpdateTimers()
    {
        if (attackTimer > 0f) attackTimer -= Time.deltaTime;
        if (boulderThrowTimer > 0f) boulderThrowTimer -= Time.deltaTime;
    }
    
    #endregion
}

// КЛАСС ДЛЯ УПРАВЛЕНИЯ ПОЛЕТОМ ПО КРИВОЙ
public class BoulderFlightController : MonoBehaviour
{
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float speed;
    private float maxHeight;
    private float curveFactor;
    private float flightTime;
    private float currentTime = 0f;
    
    private TrailRenderer trailRenderer;
    
    public void Initialize(Vector3 start, Vector3 target, float speed, float height, float curve)
    {
        this.startPosition = start;
        this.targetPosition = target;
        this.speed = speed;
        this.maxHeight = height;
        this.curveFactor = Mathf.Clamp01(curve);
        
        float distance = Vector3.Distance(start, target);
        this.flightTime = distance / speed;
        
        // Убираем физику для контролируемого полета
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }
    
    private void Start()
    {
        CreateTrail();
    }
    
    private void Update()
    {
        if (currentTime < flightTime)
        {
            currentTime += Time.deltaTime;
            float t = currentTime / flightTime;
            
            // Интерполяция позиции с учетом кривой
            Vector3 currentPosition = CalculateCurvedPosition(t);
            transform.position = currentPosition;
            
            // Вращение валуна в полете
            transform.Rotate(Vector3.up, 360f * Time.deltaTime, Space.World);
            transform.Rotate(Vector3.right, 180f * Time.deltaTime, Space.World);
        }
        else
        {
            // Приземление
            GetComponent<BoulderProjectile>().Explode();
            Destroy(this);
        }
    }
    
    private Vector3 CalculateCurvedPosition(float t)
    {
        // Линейная интерполяция
        Vector3 linearPosition = Vector3.Lerp(startPosition, targetPosition, t);
        
        // Кривая Безье для высоты
        float curveT = t;
        float height = CalculateBezierHeight(curveT);
        
        // Комбинируем позицию
        Vector3 curvedPosition = linearPosition + Vector3.up * height;
        
        return curvedPosition;
    }
    
    private float CalculateBezierHeight(float t)
    {
        // Кривая Безье с контрольными точками для настраиваемой кривизны
        float controlPoint = maxHeight * curveFactor;
        
        // Квадратичная кривая Безье
        float oneMinusT = 1f - t;
        float height = oneMinusT * oneMinusT * 0f + // Начальная точка (высота 0)
                       2f * oneMinusT * t * controlPoint + // Контрольная точка
                       t * t * 0f; // Конечная точка (высота 0)
        
        return height;
    }
    
    private void CreateTrail()
    {
        GameObject trailObject = new GameObject("BoulderTrail");
        trailObject.transform.SetParent(transform);
        trailObject.transform.localPosition = Vector3.zero;
        
        trailRenderer = trailObject.AddComponent<TrailRenderer>();
        trailRenderer.time = 1f;
        trailRenderer.startWidth = 0.3f;
        trailRenderer.endWidth = 0.1f;
        trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
        trailRenderer.startColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        trailRenderer.endColor = new Color(0.5f, 0.5f, 0.5f, 0f);
    }
}

// КЛАСС ДЛЯ ВАЛУНА
[RequireComponent(typeof(Rigidbody))]
public class BoulderProjectile : MonoBehaviour
{
    [Header("Настройки валуна")]
    [SerializeField] private float damage = 25f;
    [SerializeField] private float explosionRadius = 3f; // Область взрыва
    [SerializeField] private float lifeTime = 8f;
    [SerializeField] private GameObject explosionEffect;
    
    [Header("Эффекты")]
    [SerializeField] private AudioClip throwSound;
    [SerializeField] private AudioClip explosionSound;
    
    private bool hasExploded = false;
    private AudioSource audioSource;
    private Rigidbody rb;
    private float attackRange = 20f;
    
    // Настройки трейла
    private GameObject trailPrefab;
    private Material trailMaterial;
    private Color trailColor;
    private float trailWidth;
    private float trailTime;
    private TrailRenderer trailRenderer;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (throwSound != null)
        {
            audioSource.PlayOneShot(throwSound);
        }
        
        Destroy(gameObject, lifeTime);
    }
    
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }
    
    public void SetRange(float range)
    {
        attackRange = range;
    }
    
    public void SetExplosionRadius(float radius)
    {
        explosionRadius = radius;
    }
    
    public void SetTrailProperties(GameObject prefab, Material material, Color color, float width, float time)
    {
        trailPrefab = prefab;
        trailMaterial = material;
        trailColor = color;
        trailWidth = width;
        trailTime = time;
        
        CreateTrail();
    }
    
    private void CreateTrail()
    {
        if (trailPrefab != null)
        {
            GameObject trail = Instantiate(trailPrefab, transform.position, Quaternion.identity);
            trail.transform.SetParent(transform);
            trailRenderer = trail.GetComponent<TrailRenderer>();
        }
        else
        {
            // Создаем простой трейл если нет префаба
            GameObject trailObject = new GameObject("BoulderTrail");
            trailObject.transform.SetParent(transform);
            trailObject.transform.localPosition = Vector3.zero;
            
            trailRenderer = trailObject.AddComponent<TrailRenderer>();
            trailRenderer.time = trailTime;
            trailRenderer.startWidth = trailWidth;
            trailRenderer.endWidth = trailWidth * 0.3f;
            
            if (trailMaterial != null)
            {
                trailRenderer.material = trailMaterial;
            }
            else
            {
                trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
            }
            
            trailRenderer.startColor = trailColor;
            trailRenderer.endColor = new Color(trailColor.r, trailColor.g, trailColor.b, 0f);
        }
    }
    
    private void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;
        
        if (collision.gameObject.GetComponent<BoulderProjectile>() != null)
            return;
            
        if (collision.gameObject.CompareTag("Enemy"))
            return;
            
        Explode();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (hasExploded) return;
        
        if (other.CompareTag("Player"))
        {
            Explode();
        }
    }
    
    public void Explode()
    {
        if (hasExploded) return;
        
        hasExploded = true;
        
        // Отключаем визуализацию
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null) meshRenderer.enabled = false;
        
        Collider boulderCollider = GetComponent<Collider>();
        if (boulderCollider != null) boulderCollider.enabled = false;
        
        // Отключаем контроллер полета
        BoulderFlightController flightController = GetComponent<BoulderFlightController>();
        if (flightController != null)
        {
            Destroy(flightController);
        }
        
        // Отключаем трейл
        if (trailRenderer != null)
        {
            trailRenderer.emitting = false;
            trailRenderer.transform.SetParent(null);
            Destroy(trailRenderer.gameObject, trailRenderer.time);
        }
        
        // Эффекты
        if (explosionEffect != null)
        {
            GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(explosion, 3f);
        }
        
        if (explosionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }
        
        // Наносим урон только в области взрыва
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                PlayerHealth playerHealth = collider.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    float distance = Vector3.Distance(transform.position, collider.transform.position);
                    float damageMultiplier = Mathf.Clamp01(1 - (distance / explosionRadius));
                    float finalDamage = damage;
                    
                    playerHealth.TakeDamage(finalDamage);
                    Debug.Log($"Валун нанес {finalDamage:F1} урона игроку (расстояние: {distance:F2}, радиус взрыва: {explosionRadius})");
                }
            }
        }
        
        Destroy(gameObject, 1f);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
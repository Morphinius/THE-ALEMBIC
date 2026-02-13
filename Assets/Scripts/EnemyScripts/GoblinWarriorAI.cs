using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class EnemyAI : MonoBehaviour
{
    #region PUBLIC VARIABLES - Настройки в инспекторе
    
    [Header("Основные настройки")]
<<<<<<< HEAD
    [SerializeField] private float moveSpeed = 5f;
=======
    [SerializeField] private float moveSpeed = 6f;
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1f;
    
    [Header("Здоровье противника")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [SerializeField] private bool isAlive = true;
    
    [Header("Дистанция атаки")]
    [SerializeField] private float preferredAttackDistance = 1.5f; // Дистанция, на которой враг будет атаковать
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float minAttackDistance = 1f; // Минимальная дистанция для атаки
    
    [Header("UI - Здоровье")]
    [SerializeField] private Slider healthSlider; // Ссылка на Slider для HP бара
    [SerializeField] private Image healthFillImage; // Ссылка на Image заполнения HP бара
    [SerializeField] private Gradient healthGradient; // Градиент для изменения цвета HP бара
    [SerializeField] private Canvas healthCanvas; // Canvas для HP бара
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0, 2.5f, 0); // Смещение HP бара над противником
    [SerializeField] private bool showHealthBar = true; // Показывать ли HP бар
    [SerializeField] private float healthBarFadeTime = 2f; // Время через которое HP бар скрывается
    [SerializeField] private float healthBarShowDuration = 5f; // Время показа HP бара при получении урона
    
    [Header("Обнаружение игрока")]
    [SerializeField] private float detectionRadius = 15f;
    [SerializeField] private float fieldOfViewAngle = 90f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleLayer;
    
    [Header("Патрулирование")]
    [SerializeField] private List<Transform> patrolPoints = new List<Transform>();
    [SerializeField] private float patrolWaitTime = 2f;
    [SerializeField] private float patrolPointThreshold = 0.5f;
    
    [Header("Атаки - вероятности (в сумме должны быть 100)")]
    [SerializeField] [Range(0, 100)] private int normalAttackChance = 70;
    [SerializeField] [Range(0, 100)] private int dashAttackChance = 30;
    
    [Header("Атака с рывком")]
    [SerializeField] private float dashAttackMinDistance = 4f; // НОВОЕ: Минимальная дистанция для запуска атаки с рывком
    [SerializeField] private float dashAttackPreferredDistance = 6f; // НОВОЕ: Оптимальная дистанция для запуска атаки с рывком
    [SerializeField] private float dashDistance = 8f;
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float dashAttackDamageMultiplier = 1.5f; // Множитель урона рывка
    
    [Header("Регруппировка")]
    [SerializeField] [Range(0, 100)] private int regroupChance = 20;
    [SerializeField] private float regroupDistance = 5f;
    [SerializeField] private float regroupDuration = 2f;
    
    [Header("Возвращение на базу")]
    [SerializeField] private float returnToBaseTime = 5f; // Время после которого возвращаемся на базу

    [Header("Анимация")]
    [SerializeField] private string moveAnimationParameter = "IsMoving"; // Движение в аниматоре
    [SerializeField] private string regroupAnimationParameter = "IsRegroup"; // Регруппировка в аниматоре
    [SerializeField] private string attackAnimationParameter = "IsAttacking"; // Атака в аниматоре
    [SerializeField] private string dashAttackAnimationParameter = "IsAttackingDash"; // Атака с рывком в аниматоре
<<<<<<< HEAD
    [SerializeField] private string deathAnimationParameter = "IsDeath"; // Смерть в аниматоре
    [SerializeField] private string damageAnimationParameter = "IsDamaged"; // Получение урона в аниматоре
    [SerializeField] private float damageAnimationDuration = 0.8f; // Длительность анимации получения урона (можно менять в инспекторе)
=======
    [SerializeField] private string deathAnimationParameter = "IsDeath"; // Смерть в аниматоре - ДОБАВЛЕНО
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
    
    [Header("Gizmos (для отладки)")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color detectionColor = Color.yellow;
    [SerializeField] private Color attackColor = Color.red;
    [SerializeField] private Color returnColor = Color.blue;
    [SerializeField] private Color preferredDistanceColor = Color.green;
    [SerializeField] private Color dashAttackDistanceColor = Color.magenta; // НОВОЕ: Цвет для дистанции атаки рывком
    
    #endregion
    
    #region ENUM DEFINITIONS - Определения перечислений
    
    // Делаем enum публичным для доступа из других скриптов
    public enum AIState { Idle, Patrol, Chase, Attack, Regroup, Dead, ReturnToBase }
    
    #endregion
    
    #region PRIVATE VARIABLES - Внутренние переменные
    
    private Transform player;
    private Rigidbody rb;
    private Animator Goblin_Animator; // Ссылка на компонент Animator
    private Vector3 startPosition;
    private Quaternion startRotation;

    // ДЛЯ АНИМАЦИЙ - добавляем булеву переменную для отслеживания движения
    private bool isMoving = false;
    private bool isRegroup = false;
    private bool isAttackingAnim = false; // Для аниматора
    private bool isAttackingDashAnim = false; // Для аниматора атаки с рывком
<<<<<<< HEAD
    private bool isDeathAnim = false; // Для аниматора смерти
    private bool isDamagedAnim = false; // Для аниматора получения урона
=======
    private bool isDeathAnim = false; // Для аниматора смерти - ДОБАВЛЕНО
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
    private bool wasMoving = false; // Для отслеживания изменения состояния
    private bool wasRegroup = false; // Для отслеживания изменения состояния регруппировки
    private bool wasAttacking = false; // Для отслеживания изменения состояния атаки
    private bool wasAttackingDash = false; // Для отслеживания изменения состояния атаки с рывком
<<<<<<< HEAD
    private bool wasDeath = false; // Для отслеживания изменения состояния смерти
    private bool wasDamaged = false; // Для отслеживания изменения состояния получения урона
=======
    private bool wasDeath = false; // Для отслеживания изменения состояния смерти - ДОБАВЛЕНО
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
    
    // Для HP бара
    private float healthBarTimer = 0f;
    private bool healthBarVisible = false;
    private CanvasGroup healthCanvasGroup;
    
    // События для UI
    public delegate void HealthChangedDelegate(float currentHealth, float maxHealth);
    public event HealthChangedDelegate OnHealthChanged;
    
    public delegate void EnemyDiedDelegate();
    public event EnemyDiedDelegate OnEnemyDied;
    
    // Состояния ИИ
    private AIState currentState = AIState.Patrol;
    
    // Таймеры
    private float attackTimer = 0f;
    private float patrolTimer = 0f;
    private float regroupTimer = 0f;
    private float playerLostTimer = 0f;
    private float dashAnimationTimer = 0f; // Таймер для анимации рывка
<<<<<<< HEAD
    private float damageAnimationTimer = 0f; // Таймер для анимации получения урона
    private bool isInDamageAnimation = false; // Флаг, что мы находимся в анимации получения урона
=======
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
    
    // Патрулирование
    private int currentPatrolIndex = 0;
    private bool isPatrolForward = true;
    
    // Атака
    private bool isAttacking = false;
    private bool isDashing = false;
    private Vector3 dashDirection;
    
    // Регруппировка
    private bool isRegrouping = false;
    
    // Возвращение на базу
    private bool isReturningToBase = false;
    
    // Счетчик атак для регруппировки
    private int attackCounter = 0;
    
    // Последняя известная позиция игрока
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
    
    // Свойство для доступа к состоянию движения из аниматора
    public bool IsMoving => isMoving;
    
    // Теперь это свойство работает корректно, так как AIState публичный
    public AIState CurrentAIState => currentState;
    
    #endregion
    
    #region UNITY CALLBACKS - Методы Unity
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        Goblin_Animator = GetComponent<Animator>(); // Получаем компонент Animator
        startPosition = transform.position;
        startRotation = transform.rotation;
        
        // Инициализация здоровья
        currentHealth = maxHealth;
        isAlive = true;
        
        // Автоматически находим игрока по тегу
        FindPlayer();
        
        // Если точки патрулирования не заданы, используем стартовую позицию
        if (patrolPoints.Count == 0)
        {
            GameObject startPoint = new GameObject("StartPatrolPoint");
            startPoint.transform.position = startPosition;
            startPoint.transform.SetParent(transform.parent);
            patrolPoints.Add(startPoint.transform);
        }
        
        // Инициализируем аниматор
        InitializeAnimator();
        
        // Инициализируем HP бар
        InitializeHealthBar();
        
        // Начинаем с патрулирования
        SwitchState(AIState.Patrol);
    }
    
    private void Update()
    {
        if (!isAlive) return;
        
        UpdateTimers();
        StateMachineUpdate();
        UpdateAnimator(); // Обновляем аниматор каждый кадр
        
        // Обновляем HP бар
        UpdateHealthBar();
<<<<<<< HEAD
        
        // Обновляем таймер анимации получения урона
        UpdateDamageAnimation();
=======
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
    }
    
    private void FixedUpdate()
    {
        if (!isAlive) return;
        
        StateMachineFixedUpdate();
    }
    
    private void LateUpdate()
    {
        // Обновляем позицию HP бара в LateUpdate чтобы он корректно следовал за противником
        UpdateHealthBarPosition();
    }
    
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        
        // Радиус обнаружения
        Gizmos.color = detectionColor;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
        
        // Радиус атаки
        Gizmos.color = attackColor;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Оптимальная дистанция для обычной атаки
        Gizmos.color = preferredDistanceColor;
        Gizmos.DrawWireSphere(transform.position, preferredAttackDistance);
        
        // Дистанция для атаки с рывком
        Gizmos.color = dashAttackDistanceColor;
        Gizmos.DrawWireSphere(transform.position, dashAttackPreferredDistance);
        Gizmos.DrawWireSphere(transform.position, dashAttackMinDistance);
        
        // Стартовая позиция (база)
        Gizmos.color = returnColor;
        Gizmos.DrawSphere(startPosition, 0.5f);
        Gizmos.DrawWireSphere(startPosition, 1f);
        
        // Угол обзора
        Vector3 fovLine1 = Quaternion.Euler(0, fieldOfViewAngle / 2, 0) * transform.forward * detectionRadius;
        Vector3 fovLine2 = Quaternion.Euler(0, -fieldOfViewAngle / 2, 0) * transform.forward * detectionRadius;
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, fovLine1);
        Gizmos.DrawRay(transform.position, fovLine2);
        
        // Линия к игроку если он виден
        if (player != null && CanSeePlayer())
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);
        }
        
        // Последняя известная позиция игрока
        if (hasLastKnownPosition)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(lastKnownPlayerPosition, 0.3f);
            Gizmos.DrawWireSphere(lastKnownPlayerPosition, 0.5f);
        }
        
        // Точки патрулирования
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
    
    /// <summary>
    /// Инициализация HP бара
    /// </summary>
    private void InitializeHealthBar()
    {
        // Если Canvas не задан, пытаемся найти его среди дочерних объектов
        if (healthCanvas == null)
        {
            healthCanvas = GetComponentInChildren<Canvas>();
            
            // Если не нашли, создаем новый Canvas
            if (healthCanvas == null)
            {
                CreateHealthBarUI();
            }
        }
        
        // Инициализируем CanvasGroup для плавного появления/исчезновения
        if (healthCanvas != null)
        {
            healthCanvasGroup = healthCanvas.GetComponent<CanvasGroup>();
            if (healthCanvasGroup == null)
            {
                healthCanvasGroup = healthCanvas.gameObject.AddComponent<CanvasGroup>();
            }
            
            // Настраиваем начальное состояние HP бара
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
            
            // Настраиваем Slider если он задан
            if (healthSlider != null)
            {
                healthSlider.minValue = 0;
                healthSlider.maxValue = maxHealth;
                healthSlider.value = currentHealth;
                
                // Настраиваем цвет заполнения если задан Image
                if (healthFillImage != null)
                {
                    float healthPercentage = currentHealth / maxHealth;
                    healthFillImage.color = healthGradient.Evaluate(healthPercentage);
                }
            }
        }
        else
        {
            Debug.LogWarning("Health Canvas не найден и не может быть создан.");
        }
    }
    
    /// <summary>
    /// Создание UI элементов для HP бара
    /// </summary>
    private void CreateHealthBarUI()
    {
        // Создаем Canvas
        GameObject canvasObject = new GameObject("HealthBarCanvas");
        canvasObject.transform.SetParent(transform);
        canvasObject.transform.localPosition = healthBarOffset;
        canvasObject.transform.localRotation = Quaternion.identity;
        canvasObject.transform.localScale = Vector3.one;
        
        healthCanvas = canvasObject.AddComponent<Canvas>();
        healthCanvas.renderMode = RenderMode.WorldSpace;
        
        // Добавляем CanvasScaler для правильного масштабирования
        CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
        canvasScaler.dynamicPixelsPerUnit = 10;
        
        // Создаем Slider
        GameObject sliderObject = new GameObject("HealthSlider");
        sliderObject.transform.SetParent(canvasObject.transform);
        sliderObject.transform.localPosition = Vector3.zero;
        sliderObject.transform.localRotation = Quaternion.identity;
        sliderObject.transform.localScale = new Vector3(0.2f, 0.03f, 1f);
        
        healthSlider = sliderObject.AddComponent<Slider>();
        
        // Создаем Background
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
        
        // Создаем Fill Area
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
        
        // Создаем Fill
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
        
        // Настраиваем Slider
        healthSlider.fillRect = fillRect;
        healthSlider.targetGraphic = healthFillImage;
        healthSlider.direction = Slider.Direction.LeftToRight;
        
        // Создаем CanvasGroup для плавного исчезновения
        healthCanvasGroup = canvasObject.AddComponent<CanvasGroup>();
        
        // Обновляем начальные значения
        UpdateHealthBarValue();
    }
    
    /// <summary>
    /// Обновление позиции HP бара
    /// </summary>
    private void UpdateHealthBarPosition()
    {
        if (healthCanvas != null && healthCanvasGroup != null && healthCanvasGroup.alpha > 0.01f)
        {
            // Позиционируем Canvas над противником
            healthCanvas.transform.position = transform.position + healthBarOffset;
            
            // Поворачиваем Canvas к камере
            if (Camera.main != null)
            {
                healthCanvas.transform.LookAt(healthCanvas.transform.position + Camera.main.transform.rotation * Vector3.forward,
                    Camera.main.transform.rotation * Vector3.up);
            }
        }
    }
    
    /// <summary>
    /// Обновление значения HP бара
    /// </summary>
    private void UpdateHealthBarValue()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
            
            // Обновляем цвет заполнения
            if (healthFillImage != null)
            {
                float healthPercentage = currentHealth / maxHealth;
                healthFillImage.color = healthGradient.Evaluate(healthPercentage);
            }
        }
    }
    
    /// <summary>
    /// Показать/скрыть HP бар
    /// </summary>
    /// <param name="show">Показывать ли HP бар</param>
    private void ShowHealthBar(bool show)
    {
        if (healthCanvasGroup != null)
        {
            healthCanvasGroup.alpha = show ? 1f : 0f;
            healthBarVisible = show;
        }
    }
    
    /// <summary>
    /// Показать HP бар на определенное время
    /// </summary>
    /// <param name="duration">Длительность показа</param>
    private void ShowHealthBarTemporarily(float duration)
    {
        if (!showHealthBar) return;
        
        healthBarTimer = duration;
        ShowHealthBar(true);
        healthBarVisible = true;
    }
    
    /// <summary>
    /// Обновление таймера и видимости HP бара
    /// </summary>
    private void UpdateHealthBar()
    {
        if (!showHealthBar || !healthBarVisible || healthCanvasGroup == null) return;
        
        if (healthBarTimer > 0)
        {
            healthBarTimer -= Time.deltaTime;
            
            // Плавное исчезновение в последние секунды
            if (healthBarTimer <= healthBarFadeTime)
            {
                healthCanvasGroup.alpha = Mathf.Lerp(0f, 1f, healthBarTimer / healthBarFadeTime);
            }
        }
        else if (healthCanvasGroup.alpha > 0)
        {
            // Полностью скрываем HP бар
            ShowHealthBar(false);
        }
    }
    
    /// <summary>
    /// Настройка HP бара из других скриптов
    /// </summary>
    /// <param name="slider">Ссылка на Slider</param>
    /// <param name="fillImage">Ссылка на Image заполнения</param>
    /// <param name="canvas">Ссылка на Canvas</param>
    public void SetupHealthBar(Slider slider, Image fillImage = null, Canvas canvas = null)
    {
        healthSlider = slider;
        healthFillImage = fillImage;
        
        if (canvas != null)
        {
            healthCanvas = canvas;
        }
        
        // Инициализируем HP бар с новыми ссылками
        InitializeHealthBar();
    }
    
    /// <summary>
    /// Включить/выключить отображение HP бара
    /// </summary>
    /// <param name="enabled">Включен ли HP бар</param>
    public void SetHealthBarEnabled(bool enabled)
    {
        showHealthBar = enabled;
        
        if (healthCanvasGroup != null)
        {
            if (enabled)
            {
                ShowHealthBarTemporarily(healthBarShowDuration);
            }
            else
            {
                ShowHealthBar(false);
            }
        }
    }
    
    #endregion
    
    #region ANIMATION SYSTEM - Система анимации
    
    /// <summary>
    /// Инициализация аниматора
    /// </summary>
    private void InitializeAnimator()
    {
        if (Goblin_Animator == null)
        {
            Debug.LogError("Animator component not found on " + gameObject.name);
            return;
        }
        
        // Проверяем наличие параметров в аниматоре
        bool hasMoveParam = false;
        bool hasRegroupParam = false;
        bool hasAttackParam = false;
        bool hasDashAttackParam = false;
<<<<<<< HEAD
        bool hasDeathParam = false;
        bool hasDamageParam = false;
=======
        bool hasDeathParam = false; // ДОБАВЛЕНО
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
        
        foreach (AnimatorControllerParameter param in Goblin_Animator.parameters)
        {
            if (param.name == moveAnimationParameter && param.type == AnimatorControllerParameterType.Bool)
            {
                hasMoveParam = true;
            }
            if (param.name == regroupAnimationParameter && param.type == AnimatorControllerParameterType.Bool)
            {
                hasRegroupParam = true;
            }
            if (param.name == attackAnimationParameter && param.type == AnimatorControllerParameterType.Bool)
            {
                hasAttackParam = true;
            }
            if (param.name == dashAttackAnimationParameter && param.type == AnimatorControllerParameterType.Bool)
            {
                hasDashAttackParam = true;
            }
<<<<<<< HEAD
=======
            // ДОБАВЛЕНО: проверка параметра смерти
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
            if (param.name == deathAnimationParameter && param.type == AnimatorControllerParameterType.Bool)
            {
                hasDeathParam = true;
            }
<<<<<<< HEAD
            if (param.name == damageAnimationParameter && param.type == AnimatorControllerParameterType.Bool)
            {
                hasDamageParam = true;
            }
=======
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
        }
        
        if (!hasMoveParam)
        {
<<<<<<< HEAD
            Debug.LogWarning($"Animator parameter '{moveAnimationParameter}' not found.");
=======
            Debug.LogWarning($"Animator parameter '{moveAnimationParameter}' not found. Please add a Bool parameter with this name to the Animator Controller.");
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
        }
        
        if (!hasRegroupParam)
        {
<<<<<<< HEAD
            Debug.LogWarning($"Animator parameter '{regroupAnimationParameter}' not found.");
=======
            Debug.LogWarning($"Animator parameter '{regroupAnimationParameter}' not found. Please add a Bool parameter with this name to the Animator Controller.");
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
        }
        
        if (!hasAttackParam)
        {
<<<<<<< HEAD
            Debug.LogWarning($"Animator parameter '{attackAnimationParameter}' not found.");
=======
            Debug.LogWarning($"Animator parameter '{attackAnimationParameter}' not found. Please add a Bool parameter with this name to the Animator Controller.");
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
        }
        
        if (!hasDashAttackParam)
        {
<<<<<<< HEAD
            Debug.LogWarning($"Animator parameter '{dashAttackAnimationParameter}' not found.");
        }
        
        if (!hasDeathParam)
        {
            Debug.LogWarning($"Animator parameter '{deathAnimationParameter}' not found.");
        }
        
        if (!hasDamageParam)
        {
            Debug.LogWarning($"Animator parameter '{damageAnimationParameter}' not found.");
=======
            Debug.LogWarning($"Animator parameter '{dashAttackAnimationParameter}' not found. Please add a Bool parameter with this name to the Animator Controller.");
        }
        
        // ДОБАВЛЕНО: предупреждение если параметр смерти не найден
        if (!hasDeathParam)
        {
            Debug.LogWarning($"Animator parameter '{deathAnimationParameter}' not found. Please add a Bool parameter with this name to the Animator Controller.");
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
        }
    }
    
    /// <summary>
    /// Обновление параметров аниматора
    /// </summary>
    private void UpdateAnimator()
    {
        if (Goblin_Animator == null) return;
        
<<<<<<< HEAD
        // ВАЖНО: Анимация получения урона имеет приоритет над другими анимациями
        // Если активна анимация получения урона, отключаем другие анимации
        if (isInDamageAnimation)
        {
            // Принудительно отключаем все другие анимации
            Goblin_Animator.SetBool(moveAnimationParameter, false);
            Goblin_Animator.SetBool(regroupAnimationParameter, false);
            Goblin_Animator.SetBool(attackAnimationParameter, false);
            Goblin_Animator.SetBool(dashAttackAnimationParameter, false);
            Goblin_Animator.SetBool(deathAnimationParameter, false);
            
            // Устанавливаем анимацию получения урона
            if (!wasDamaged || isDamagedAnim != wasDamaged)
            {
                Goblin_Animator.SetBool(damageAnimationParameter, true);
                wasDamaged = true;
            }
            
            // Сбрасываем флаги других анимаций для корректного обновления после урона
            wasMoving = false;
            wasRegroup = false;
            wasAttacking = false;
            wasAttackingDash = false;
            wasDeath = false;
            return;
        }
        
        // Обычное обновление анимаций если нет активной анимации урона
=======
        // Обновляем параметр движения только если состояние изменилось
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
        if (isMoving != wasMoving)
        {
            Goblin_Animator.SetBool(moveAnimationParameter, isMoving);
            wasMoving = isMoving;
        }
        
<<<<<<< HEAD
=======
        // Обновляем параметр регруппировки только если состояние изменилось
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
        if (isRegroup != wasRegroup)
        {
            Goblin_Animator.SetBool(regroupAnimationParameter, isRegroup);
            wasRegroup = isRegroup;
        }
        
<<<<<<< HEAD
=======
        // Обновляем параметр обычной атаки только если состояние изменилось
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
        if (isAttackingAnim != wasAttacking)
        {
            Goblin_Animator.SetBool(attackAnimationParameter, isAttackingAnim);
            wasAttacking = isAttackingAnim;
        }
        
<<<<<<< HEAD
=======
        // Обновляем параметр атаки с рывком только если состояние изменилось
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
        if (isAttackingDashAnim != wasAttackingDash)
        {
            Goblin_Animator.SetBool(dashAttackAnimationParameter, isAttackingDashAnim);
            wasAttackingDash = isAttackingDashAnim;
        }
        
<<<<<<< HEAD
=======
        // ДОБАВЛЕНО: обновляем параметр смерти только если состояние изменилось
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
        if (isDeathAnim != wasDeath)
        {
            Goblin_Animator.SetBool(deathAnimationParameter, isDeathAnim);
            wasDeath = isDeathAnim;
        }
<<<<<<< HEAD
        
        // Обновляем параметр получения урона только если состояние изменилось
        if (isDamagedAnim != wasDamaged)
        {
            Goblin_Animator.SetBool(damageAnimationParameter, isDamagedAnim);
            wasDamaged = isDamagedAnim;
        }
=======
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
    }
    
    /// <summary>
    /// Принудительное обновление аниматора
    /// </summary>
    public void ForceAnimatorUpdate()
    {
        if (Goblin_Animator != null)
        {
            Goblin_Animator.SetBool(moveAnimationParameter, isMoving);
            wasMoving = isMoving;
            Goblin_Animator.SetBool(regroupAnimationParameter, isRegroup);
            wasRegroup = isRegroup;
            Goblin_Animator.SetBool(attackAnimationParameter, isAttackingAnim);
            wasAttacking = isAttackingAnim;
            Goblin_Animator.SetBool(dashAttackAnimationParameter, isAttackingDashAnim);
            wasAttackingDash = isAttackingDashAnim;
<<<<<<< HEAD
            Goblin_Animator.SetBool(deathAnimationParameter, isDeathAnim);
            wasDeath = isDeathAnim;
            Goblin_Animator.SetBool(damageAnimationParameter, isDamagedAnim);
            wasDamaged = isDamagedAnim;
        }
    }
    
    /// <summary>
    /// Обновление таймера анимации получения урона
    /// </summary>
    private void UpdateDamageAnimation()
    {
        if (isDamagedAnim && damageAnimationTimer > 0)
        {
            damageAnimationTimer -= Time.deltaTime;
            isInDamageAnimation = true; // Устанавливаем флаг, что мы в анимации получения урона
            
            if (damageAnimationTimer <= 0)
            {
                // Завершаем анимацию получения урона
                EndDamageAnimation();
            }
        }
        else if (isInDamageAnimation && damageAnimationTimer <= 0)
        {
            EndDamageAnimation();
        }
    }
    
    /// <summary>
    /// Завершение анимации получения урона
    /// </summary>
    private void EndDamageAnimation()
    {
        isDamagedAnim = false;
        isInDamageAnimation = false;
        
        // Принудительно обновляем аниматор чтобы убрать параметр IsDamaged
        if (Goblin_Animator != null)
        {
            Goblin_Animator.SetBool(damageAnimationParameter, false);
            wasDamaged = false;
        }
        
        Debug.Log("Анимация получения урона завершена");
    }
    
    /// <summary>
    /// Запуск анимации получения урона
    /// </summary>
    private void StartDamageAnimation()
    {
        isDamagedAnim = true;
        isInDamageAnimation = true;
        damageAnimationTimer = damageAnimationDuration;
        
        // При получении урона останавливаем другие анимации
        isMoving = false;
        isRegroup = false;
        isAttackingAnim = false;
        isAttackingDashAnim = false;
        
        // Принудительно обновляем аниматор
        if (Goblin_Animator != null)
        {
            Goblin_Animator.SetBool(damageAnimationParameter, true);
            Goblin_Animator.SetBool(moveAnimationParameter, false);
            Goblin_Animator.SetBool(regroupAnimationParameter, false);
            Goblin_Animator.SetBool(attackAnimationParameter, false);
            Goblin_Animator.SetBool(dashAttackAnimationParameter, false);
            
            wasDamaged = true;
            wasMoving = false;
            wasRegroup = false;
            wasAttacking = false;
            wasAttackingDash = false;
        }
        
        Debug.Log($"{gameObject.name} получил урон, проигрывается анимация получения урона (длительность: {damageAnimationDuration} сек)");
    }
    
=======
            // ДОБАВЛЕНО: принудительное обновление параметра смерти
            Goblin_Animator.SetBool(deathAnimationParameter, isDeathAnim);
            wasDeath = isDeathAnim;
        }
    }
    
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
    #endregion
    
    #region HEALTH SYSTEM - Система здоровья
    
    /// <summary>
    /// Нанесение урона противнику
    /// </summary>
    /// <param name="damage">Количество урона</param>
    public void TakeDamage(float damage)
    {
        if (!isAlive) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
<<<<<<< HEAD
        // ЗАПУСК АНИМАЦИИ ПОЛУЧЕНИЯ УРОНА
        StartDamageAnimation();
        
=======
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
        // Обновляем HP бар
        UpdateHealthBarValue();
        
        // Показываем HP бар при получении урона
        ShowHealthBarTemporarily(healthBarShowDuration);
        
        // Вызываем событие изменения здоровья
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        // Переходим в режим преследования при получении урона
        if (currentState != AIState.Chase && currentState != AIState.Attack && currentState != AIState.Dead)
        {
            SwitchState(AIState.Chase);
        }
        
        // Проверяем смерть
        if (currentHealth <= 0)
        {
            Die();
        }
        
        Debug.Log($"{gameObject.name} получил {damage} урона. Здоровье: {currentHealth}/{maxHealth}");
    }
    
    /// <summary>
    /// Лечение противника
    /// </summary>
    /// <param name="healAmount">Количество здоровья</param>
    public void Heal(float healAmount)
    {
        if (!isAlive) return;
        
        currentHealth += healAmount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        
        // Обновляем HP бар
        UpdateHealthBarValue();
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    /// <summary>
    /// Полное восстановление здоровья
    /// </summary>
    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;
        
        // Обновляем HP бар
        UpdateHealthBarValue();
        
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    /// <summary>
    /// Смерть противника
    /// </summary>
    private void Die()
    {
        isAlive = false;
        isMoving = false;
        isRegroup = false;
        isAttackingAnim = false;
        isAttackingDashAnim = false;
<<<<<<< HEAD
        isDeathAnim = true;
        isDamagedAnim = false;
        isInDamageAnimation = false;
=======
        isDeathAnim = true; // ДОБАВЛЕНО: включаем анимацию смерти
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
        
        // Скрываем HP бар при смерти
        ShowHealthBar(false);
        
        SwitchState(AIState.Dead);
        
        // Обновляем аниматор
        UpdateAnimator();
        
        // Вызываем событие смерти
        OnEnemyDied?.Invoke();
        
        Debug.Log($"{gameObject.name} умер.");
    }
    
    /// <summary>
    /// Возвращает текущее здоровье (для UI)
    /// </summary>
    public float GetCurrentHealth()
    {
        return currentHealth;
    }
    
    /// <summary>
    /// Возвращает максимальное здоровье (для UI)
    /// </summary>
    public float GetMaxHealth()
    {
        return maxHealth;
    }
    
    /// <summary>
    /// Возвращает статус жизни (для других скриптов)
    /// </summary>
    public bool GetIsAlive()
    {
        return isAlive;
    }
    
    #endregion
    
    #region PLAYER INTERACTION - Взаимодействие с игроком
    
    /// <summary>
    /// Поиск игрока в сцене
    /// </summary>
    public void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogWarning("Игрок не найден! Убедитесь что объект игрока имеет тег 'Player'");
        }
    }
    
    /// <summary>
    /// Возвращает трансформ игрока (для других скриптов)
    /// </summary>
    public Transform GetPlayer()
    {
        if (player == null)
        {
            FindPlayer();
        }
        return player;
    }
    
    #endregion
    
    #region ATTACK SYSTEM - Система атаки
    
    /// <summary>
    /// Нанесение урона игроку
    /// </summary>
    private void DamagePlayer()
<<<<<<< HEAD
    {
        if (player == null) return;
        
        // Получаем компонент Player1 (основной скрипт игрока)
        Player1 playerController = player.GetComponent<Player1>();
        if (playerController != null)
        {
            // Проверяем, можно ли наносить урон игроку
            if (!playerController.CanTakeDamage())
            {
                Debug.Log("Игрок неуязвим (перекат или другая причина), урон не нанесен!");
                return;
            }
        }
        else
        {
            Debug.LogWarning("Не найден компонент Player1 у игрока!");
        }
        
        // Получаем компонент здоровья игрока
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            float damageToDeal = isDashing ? attackDamage * dashAttackDamageMultiplier : attackDamage;
            playerHealth.TakeDamage(damageToDeal);
            Debug.Log($"Нанесено {damageToDeal} урона игроку! ({(isDashing ? "атака с рывком" : "обычная атака")})");
        }
        else
        {
            Debug.LogWarning("Не найден компонент PlayerHealth у игрока!");
        }
    }
=======
{
    if (player == null) return;
    
    // Получаем компонент Player1 (основной скрипт игрока)
    Player1 playerController = player.GetComponent<Player1>();
    if (playerController != null)
    {
        // Проверяем, можно ли наносить урон игроку
        if (!playerController.CanTakeDamage())
        {
            Debug.Log("Игрок неуязвим (перекат или другая причина), урон не нанесен!");
            return;
        }
    }
    else
    {
        Debug.LogWarning("Не найден компонент Player1 у игрока!");
    }
    
    // Получаем компонент здоровья игрока
    PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
    if (playerHealth != null)
    {
        float damageToDeal = isDashing ? attackDamage * dashAttackDamageMultiplier : attackDamage;
        playerHealth.TakeDamage(damageToDeal);
        Debug.Log($"Нанесено {damageToDeal} урона игроку! ({(isDashing ? "атака с рывком" : "обычная атака")})");
    }
    else
    {
        Debug.LogWarning("Не найден компонент PlayerHealth у игрока!");
    }
}
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
    
    /// <summary>
    /// Проверка возможности нанесения урона игроку
    /// </summary>
    private bool CanDamagePlayer()
    {
        if (player == null) return false;
        
        // Проверяем дистанцию для атаки
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        return distanceToPlayer <= attackRange;
    }
    
    /// <summary>
    /// Проверяет, находится ли враг на оптимальной дистанции для атаки с рывком
    /// </summary>
    private bool IsAtOptimalDashAttackDistance()
    {
        if (player == null) return false;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        return distanceToPlayer >= dashAttackMinDistance && distanceToPlayer <= dashAttackPreferredDistance + 1f;
    }
    
    #endregion
    
    #region STATE MACHINE - Конечный автомат состояний
    
    private void StateMachineUpdate()
    {
        switch (currentState)
        {
            case AIState.Idle:
                IdleUpdate();
                break;
            case AIState.Patrol:
                PatrolUpdate();
                break;
            case AIState.Chase:
                ChaseUpdate();
                break;
            case AIState.Attack:
                AttackUpdate();
                break;
            case AIState.Regroup:
                RegroupUpdate();
                break;
            case AIState.Dead:
                DeadUpdate();
                break;
            case AIState.ReturnToBase:
                ReturnToBaseUpdate();
                break;
        }
    }
    
    private void StateMachineFixedUpdate()
    {
        switch (currentState)
        {
            case AIState.Chase:
                ChaseFixedUpdate();
                break;
            case AIState.Regroup:
                RegroupFixedUpdate();
                break;
            case AIState.Patrol:
                PatrolFixedUpdate();
                break;
            case AIState.ReturnToBase:
                ReturnToBaseFixedUpdate();
                break;
        }
    }
    
    private void SwitchState(AIState newState)
    {
        // Выход из предыдущего состояния
        switch (currentState)
        {
            case AIState.Patrol:
                patrolTimer = 0f;
                break;
            case AIState.Attack:
                isAttacking = false;
                isAttackingAnim = false; // Выключаем анимацию обычной атаки
                isAttackingDashAnim = false; // Выключаем анимацию атаки с рывком
                StopAllCoroutines();
                break;
            case AIState.Regroup:
                isRegrouping = false;
                isRegroup = false; // Выключаем анимацию регруппировки
                regroupTimer = 0f;
                break;
            case AIState.Chase:
                playerLostTimer = 0f; // Сбрасываем таймер при смене состояния
                break;
            case AIState.ReturnToBase:
                isReturningToBase = false;
                break;
            case AIState.Dead:
                isDeathAnim = false; // Сбрасываем анимацию смерти при смене состояния (например, при респавне)
<<<<<<< HEAD
                isDamagedAnim = false; // Выключаем анимацию получения урона
                isInDamageAnimation = false; // Сбрасываем флаг анимации урона
=======
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
                break;
        }
        
        // Вход в новое состояние
        switch (newState)
        {
            case AIState.Patrol:
                currentPatrolIndex = GetNearestPatrolPointIndex();
                hasLastKnownPosition = false; // Сбрасываем последнюю позицию
                Debug.Log("Начинаю патрулирование");
                break;
            case AIState.Regroup:
                StartRegroup();
                break;
            case AIState.Dead:
                OnDeath();
                break;
            case AIState.ReturnToBase:
                StartReturnToBase();
                break;
        }
        
        currentState = newState;
        Debug.Log($"{gameObject.name} перешел в состояние: {currentState}");
    }
    
    private void DeadUpdate()
    {
        // Логика поведения при смерти
    }
    
    private void OnDeath()
    {
        // Останавливаем все движения
        isMoving = false;
        isRegroup = false;
        isAttackingAnim = false;
        isAttackingDashAnim = false;
<<<<<<< HEAD
        isDeathAnim = true;
        isDamagedAnim = false;
        isInDamageAnimation = false;
=======
        isDeathAnim = true; // ДОБАВЛЕНО: включаем анимацию смерти
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        
        // Обновляем аниматор
        UpdateAnimator();
        
        // Отключаем коллайдеры если нужно
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }
    }
    
    #endregion
    
    #region PLAYER DETECTION - Обнаружение игрока
    
    private bool CanSeePlayer()
    {
        if (player == null) return false;
        
        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        
        // Проверка расстояния
        if (distanceToPlayer > detectionRadius) return false;
        
        // Проверка угла обзора
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer > fieldOfViewAngle / 2) return false;
        
        // Проверка на препятствия
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
        return distanceToPlayer <= attackRange;
    }
    
    private bool IsPlayerTooFar()
    {
        if (player == null) return true;
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        return distanceToPlayer > detectionRadius * 2f; // В 2 раза больше радиуса обнаружения
    }
    
    #endregion
    
    #region PATROL - Патрулирование
    
    private void PatrolUpdate()
    {
        // Проверяем обнаружение игрока
        if (CanSeePlayer())
        {
            Debug.Log("Обнаружил игрока, начинаю преследование!");
            SwitchState(AIState.Chase);
            return;
        }
        
        if (patrolPoints.Count == 0) return;
        
        Transform targetPoint = patrolPoints[currentPatrolIndex];
        if (targetPoint == null) return;
        
        float distanceToPoint = Vector3.Distance(transform.position, targetPoint.position);
        
        if (distanceToPoint <= patrolPointThreshold)
        {
            // Ждем на точке
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
<<<<<<< HEAD
        if (patrolPoints.Count == 0 || isInDamageAnimation) return; // Не двигаемся во время анимации урона
=======
        if (patrolPoints.Count == 0) return;
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
        
        Transform targetPoint = patrolPoints[currentPatrolIndex];
        if (targetPoint == null) return;
        
        float distanceToPoint = Vector3.Distance(transform.position, targetPoint.position);
        
<<<<<<< HEAD
        if (distanceToPoint > patrolPointThreshold && !isInDamageAnimation) // Проверяем флаг урона
=======
        if (distanceToPoint > patrolPointThreshold)
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
        {
            // Двигаемся к точке
            isMoving = true;
            isRegroup = false;
            isAttackingAnim = false;
            isAttackingDashAnim = false;
<<<<<<< HEAD
            isDeathAnim = false;
=======
            isDeathAnim = false; // ДОБАВЛЕНО: убедимся что анимация смерти выключена
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
            MoveTowards(targetPoint.position);
            RotateTowards(targetPoint.position);
        }
        else
        {
            // Достигли точки патрулирования - останавливаемся
            isMoving = false;
            isRegroup = false;
            isAttackingAnim = false;
            isAttackingDashAnim = false;
<<<<<<< HEAD
            isDeathAnim = false;
=======
            isDeathAnim = false; // ДОБАВЛЕНО: убедимся что анимация смерти выключена
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
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
            // Игрок не найден, возвращаемся к патрулированию
            Debug.Log("Игрок не найден, возвращаюсь к патрулированию");
            SwitchState(AIState.Patrol);
            return;
        }
        
        // Сохраняем последнюю известную позицию игрока
        if (CanSeePlayer())
        {
            lastKnownPlayerPosition = player.position;
            hasLastKnownPosition = true;
            playerLostTimer = 0f; // Сбрасываем таймер если видим игрока
        }
        else
        {
            // Увеличиваем таймер потери игрока
            playerLostTimer += Time.deltaTime;
            
            // Если игрок потерян на определенное время, возвращаемся к патрулированию
            if (playerLostTimer >= returnToBaseTime)
            {
                Debug.Log($"Игрок потерян на {returnToBaseTime} секунд, возвращаюсь к патрулированию");
                SwitchState(AIState.Patrol);
                return;
            }
            
            // Проверяем, не ушел ли игрок слишком далеко
            if (IsPlayerTooFar())
            {
                Debug.Log("Игрок слишком далеко, возвращаюсь к патрулированию");
                SwitchState(AIState.Patrol);
                return;
            }
        }
        
        // Проверяем возможность атаки
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
<<<<<<< HEAD
        if (player == null || isDashing || isInDamageAnimation) // Проверяем флаг урона
        {
            if (isInDamageAnimation)
            {
                // Во время анимации урона не двигаемся
                isMoving = false;
                isRegroup = false;
                isAttackingAnim = false;
                isAttackingDashAnim = false;
                isDeathAnim = false;
            }
=======
        if (player == null || isDashing) 
        {
            isMoving = false;
            isRegroup = false;
            isAttackingAnim = false;
            isAttackingDashAnim = false;
            isDeathAnim = false; // ДОБАВЛЕНО: убедимся что анимация смерти выключена
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
            return;
        }
        
        // Если видим игрока - преследуем его с поддержанием оптимальной дистанции
        if (CanSeePlayer())
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            // Проверяем, на каком типе атаки мы должны сосредоточиться
            bool shouldUseDashAttack = IsAtOptimalDashAttackDistance();
            
            if (shouldUseDashAttack)
            {
                // Мы находимся в зоне для атаки с рывком
                // Стараемся держаться на оптимальной дистанции для рывка
                if (distanceToPlayer > dashAttackPreferredDistance + 0.5f)
                {
                    // Слишком далеко от оптимальной дистанции рывка - приближаемся
                    isMoving = true;
                    isRegroup = false;
                    isAttackingAnim = false;
                    isAttackingDashAnim = false;
<<<<<<< HEAD
                    isDeathAnim = false;
=======
                    isDeathAnim = false; // ДОБАВЛЕНО: убедимся что анимация смерти выключена
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
                    MoveTowards(player.position);
                    RotateTowards(player.position);
                }
                else if (distanceToPlayer < dashAttackMinDistance - 0.5f)
                {
                    // Слишком близко для рывка - отходим немного
                    isMoving = true;
                    isRegroup = false;
                    isAttackingAnim = false;
                    isAttackingDashAnim = false;
<<<<<<< HEAD
                    isDeathAnim = false;
=======
                    isDeathAnim = false; // ДОБАВЛЕНО: убедимся что анимация смерти выключена
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
                    MoveAwayFrom(player.position);
                    RotateTowards(player.position);
                }
                else
                {
                    // На оптимальной дистанции для рывка - готовимся к атаке
                    isMoving = false;
                    isRegroup = false;
                    isAttackingAnim = false;
                    isAttackingDashAnim = false;
<<<<<<< HEAD
                    isDeathAnim = false;
=======
                    isDeathAnim = false; // ДОБАВЛЕНО: убедимся что анимация смерти выключена
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
                    RotateTowards(player.position);
                }
            }
            else
            {
                // Мы не в зоне для атаки с рывком, используем обычную логику
                if (distanceToPlayer > preferredAttackDistance + 0.2f)
                {
                    // Слишком далеко - приближаемся
                    isMoving = true;
                    isRegroup = false;
                    isAttackingAnim = false;
                    isAttackingDashAnim = false;
<<<<<<< HEAD
                    isDeathAnim = false;
=======
                    isDeathAnim = false; // ДОБАВЛЕНО: убедимся что анимация смерти выключена
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
                    MoveTowards(player.position);
                    RotateTowards(player.position);
                }
                else if (distanceToPlayer < preferredAttackDistance - 0.2f)
                {
                    // Слишком близко - отступаем
                    isMoving = true;
                    isRegroup = false;
                    isAttackingAnim = false;
                    isAttackingDashAnim = false;
<<<<<<< HEAD
                    isDeathAnim = false;
=======
                    isDeathAnim = false; // ДОБАВЛЕНО: убедимся что анимация смерти выключена
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
                    MoveAwayFrom(player.position);
                    RotateTowards(player.position);
                }
                else
                {
                    // На оптимальной дистанции для обычной атаки - останавливаемся
                    isMoving = false;
                    isRegroup = false;
                    isAttackingAnim = false;
                    isAttackingDashAnim = false;
<<<<<<< HEAD
                    isDeathAnim = false;
=======
                    isDeathAnim = false; // ДОБАВЛЕНО: убедимся что анимация смерти выключена
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
                    RotateTowards(player.position);
                }
            }
        }
        // Иначе двигаемся к последней известной позиции
        else if (hasLastKnownPosition)
        {
            isMoving = true;
            isRegroup = false;
            isAttackingAnim = false;
            isAttackingDashAnim = false;
<<<<<<< HEAD
            isDeathAnim = false;
=======
            isDeathAnim = false; // ДОБАВЛЕНО: убедимся что анимация смерти выключена
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
            MoveTowards(lastKnownPlayerPosition);
            RotateTowards(lastKnownPlayerPosition);
            
            // Если достигли последней известной позиции
            float distanceToLastPosition = Vector3.Distance(transform.position, lastKnownPlayerPosition);
            if (distanceToLastPosition < 1f)
            {
                // Останавливаем движение
                isMoving = false;
                isRegroup = false;
                isAttackingAnim = false;
                isAttackingDashAnim = false;
<<<<<<< HEAD
                isDeathAnim = false;
=======
                isDeathAnim = false; // ДОБАВЛЕНО: убедимся что анимация смерти выключена
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
            }
        }
        else
        {
            // Нет цели для движения
            isMoving = false;
            isRegroup = false;
            isAttackingAnim = false;
            isAttackingDashAnim = false;
<<<<<<< HEAD
            isDeathAnim = false;
=======
            isDeathAnim = false; // ДОБАВЛЕНО: убедимся что анимация смерти выключена
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
        }
    }
    
    #endregion
    
    #region ATTACK - Атака
    
    private void AttackUpdate()
    {
<<<<<<< HEAD
        if (player == null || isInDamageAnimation) // Проверяем флаг урона
=======
        if (player == null)
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
        {
            SwitchState(AIState.Chase);
            return;
        }
        
        if (!isAttacking)
        {
            StartAttack();
        }
        
        // Поворачиваемся к игроку во время атаки
        RotateTowards(player.position);
        
        // Во время атаки не двигаемся физически, но управляем анимациями
        isMoving = false;
        isRegroup = false;
<<<<<<< HEAD
        isDeathAnim = false;
=======
        isDeathAnim = false; // ДОБАВЛЕНО: убедимся что анимация смерти выключена
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
        
        // Управление анимациями происходит в конкретных корутинах атаки
    }
    
    private void StartAttack()
    {
        isAttacking = true;
        
        // Выбор типа атаки на основе вероятностей и дистанции
        int randomValue = Random.Range(0, 100);
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Проверяем, находимся ли мы в зоне для атаки с рывком
        bool inDashAttackZone = distanceToPlayer >= dashAttackMinDistance && 
                               distanceToPlayer <= dashAttackPreferredDistance + 2f;
        
        // Если мы в зоне для рывка, увеличиваем шанс его использования
        int actualDashChance = inDashAttackZone ? dashAttackChance * 2 : dashAttackChance;
        actualDashChance = Mathf.Clamp(actualDashChance, 0, 100);
        
        if (randomValue < normalAttackChance && !inDashAttackZone)
        {
            StartCoroutine(NormalAttack());
        }
        else
        {
            StartCoroutine(DashAttack());
        }
        
        attackTimer = attackCooldown;
    }
    
    private IEnumerator NormalAttack()
    {
        Debug.Log("Обычная атака!");
        
        // Включаем анимацию обычной атаки
        isAttackingAnim = true;
        isAttackingDashAnim = false;
<<<<<<< HEAD
        isDeathAnim = false;
=======
        isDeathAnim = false; // ДОБАВЛЕНО: убедимся что анимация смерти выключена
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
        
        yield return new WaitForSeconds(0.3f); // Задержка перед ударом
        
        // Наносим урон игроку
        if (CanDamagePlayer())
        {
            DamagePlayer();
        }
        
        yield return new WaitForSeconds(0.2f); // Завершение анимации
        
        // Отключаем анимацию атаки
        isAttackingAnim = false;
        
        // Увеличиваем счетчик атак
        attackCounter++;
        Debug.Log($"Счетчик атак: {attackCounter}");
        
        // Проверяем возможность регруппировки после 3 атак
        if (attackCounter >= 3)
        {
            // Сбрасываем счетчик
            attackCounter = 0;
            
            // Проверяем шанс регруппировки
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
    
    private IEnumerator DashAttack()
    {
        Debug.Log("Атака с рывком!");
        
        // Включаем анимацию атаки с рывком
        isAttackingDashAnim = true;
        isAttackingAnim = false;
<<<<<<< HEAD
        isDeathAnim = false;
=======
        isDeathAnim = false; // ДОБАВЛЕНО: убедимся что анимация смерти выключена
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
        
        if (player != null)
        {
            dashDirection = (player.position - transform.position).normalized;
            isDashing = true;
            
            float dashTimer = 0f;
            while (dashTimer < dashDuration)
            {
                // Простое движение вместо MovePosition
                transform.position += dashDirection * dashSpeed * Time.deltaTime;
                dashTimer += Time.deltaTime;
                yield return null;
            }
            
            isDashing = false;
            
            // Наносим урон в конце рывка
            if (CanDamagePlayer())
            {
                DamagePlayer();
            }
        }
        
        // Ждем завершения анимации рывка
        yield return new WaitForSeconds(0.5f);
        
        // Отключаем анимацию атаки с рывком
        isAttackingDashAnim = false;
        
        // Увеличиваем счетчик атак
        attackCounter++;
        Debug.Log($"Счетчик атак: {attackCounter}");
        
        // Проверяем возможность регруппировки после 3 атак
        if (attackCounter >= 3)
        {
            // Сбрасываем счетчик
            attackCounter = 0;
            
            // Проверяем шанс регруппировки
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
    
    /// <summary>
    /// Вызывается из анимации для нанесения урона
    /// </summary>
    public void ApplyAttackDamage()
    {
        if (CanDamagePlayer())
        {
            DamagePlayer();
        }
    }
    
    /// <summary>
    /// Вызывается из анимации для нанесения урона рывком
    /// </summary>
    public void ApplyDashAttackDamage()
    {
        if (CanDamagePlayer())
        {
            DamagePlayer();
        }
    }
    
    /// <summary>
    /// Вызывается в конце анимации атаки
    /// </summary>
    public void OnAttackEnd()
    {
        isAttackingAnim = false;
    }
    
    /// <summary>
    /// Вызывается в конце анимации атаки с рывком
    /// </summary>
    public void OnDashAttackEnd()
    {
        isAttackingDashAnim = false;
    }
    
    #endregion
    
    #region REGROUP - Регруппировка
    
    private void RegroupUpdate()
    {
        if (!isRegrouping) return;
        
        regroupTimer += Time.deltaTime;
        
        // После завершения регруппировки возвращаемся к преследованию
        if (regroupTimer >= regroupDuration)
        {
            Debug.Log("Регруппировка завершена, возвращаюсь к преследованию");
            SwitchState(AIState.Chase);
        }
    }
    
    private void RegroupFixedUpdate()
    {
<<<<<<< HEAD
        if (player == null || !isRegrouping || isInDamageAnimation) // Проверяем флаг урона
=======
        if (player == null || !isRegrouping) 
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
        {
            isMoving = false;
            isRegroup = false;
            isAttackingAnim = false;
            isAttackingDashAnim = false;
<<<<<<< HEAD
            isDeathAnim = false;
=======
            isDeathAnim = false; // ДОБАВЛЕНО: убедимся что анимация смерти выключена
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
            return;
        }
        
        // Двигаемся ОТ игрока (отходим назад)
        Vector3 directionFromPlayer = (transform.position - player.position).normalized;
        Vector3 targetPosition = transform.position + directionFromPlayer * regroupDistance;
        
        isMoving = true; // Мы двигаемся во время регруппировки
        isRegroup = true; // Включаем анимацию регруппировки
        isAttackingAnim = false; // Выключаем анимацию атаки
        isAttackingDashAnim = false; // Выключаем анимацию атаки с рывком
<<<<<<< HEAD
        isDeathAnim = false;
=======
        isDeathAnim = false; // ДОБАВЛЕНО: убедимся что анимация смерти выключена
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
        
        MoveTowards(targetPosition);
        RotateTowards(targetPosition);
    }
    
    private void StartRegroup()
    {
        isRegrouping = true;
        isRegroup = true; // Включаем анимацию регруппировки
        isAttackingAnim = false; // Выключаем анимацию атаки
        isAttackingDashAnim = false; // Выключаем анимацию атаки с рывком
<<<<<<< HEAD
        isDeathAnim = false;
=======
        isDeathAnim = false; // ДОБАВЛЕНО: убедимся что анимация смерти выключена
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
        regroupTimer = 0f;
        Debug.Log("Начинаю регруппировку - отхожу назад!");
    }
    
    #endregion
    
    #region RETURN TO BASE - Возвращение на базу
    
    private void StartReturnToBase()
    {
        isReturningToBase = true;
        playerLostTimer = 0f;
        hasLastKnownPosition = false;
        Debug.Log("Начинаю возвращение на стартовую позицию (базу)");
    }
    
    private void ReturnToBaseUpdate()
    {
        // Если во время возврата на базу снова увидели игрока
        if (CanSeePlayer())
        {
            Debug.Log("Снова увидел игрока во время возврата на базу, возобновляю преследование");
            SwitchState(AIState.Chase);
            return;
        }
        
        // Проверяем, дошли ли до стартовой позиции
        float distanceToStart = Vector3.Distance(transform.position, startPosition);
        
        if (distanceToStart <= patrolPointThreshold)
        {
            // Останавливаем движение
            isMoving = false;
            isRegroup = false;
            isAttackingAnim = false;
            isAttackingDashAnim = false;
<<<<<<< HEAD
            isDeathAnim = false;
=======
            isDeathAnim = false; // ДОБАВЛЕНО: убедимся что анимация смерти выключена
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
            
            // Возвращаем начальное вращение
            transform.rotation = Quaternion.Lerp(transform.rotation, startRotation, 
                rotationSpeed * Time.deltaTime);
            
            // Если достаточно близко по вращению, переходим в патрулирование
            if (Quaternion.Angle(transform.rotation, startRotation) < 5f)
            {
                Debug.Log("Достиг стартовой позиции, начинаю патрулирование");
                SwitchState(AIState.Patrol);
            }
        }
    }
    
    private void ReturnToBaseFixedUpdate()
    {
<<<<<<< HEAD
        if (isDashing || isInDamageAnimation) return; // Проверяем флаг урона
=======
        if (isDashing) return;
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
        
        // Двигаемся к стартовой позиции
        isMoving = true;
        isRegroup = false;
        isAttackingAnim = false;
        isAttackingDashAnim = false;
<<<<<<< HEAD
        isDeathAnim = false;
=======
        isDeathAnim = false; // ДОБАВЛЕНО: убедимся что анимация смерти выключена
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
        MoveTowards(startPosition);
        RotateTowards(startPosition);
    }
    
    #endregion
    
    #region IDLE - Бездействие
    
    private void IdleUpdate()
    {
        // Проверяем обнаружение игрока
        if (CanSeePlayer())
        {
            SwitchState(AIState.Chase);
        }
    }
    
    #endregion
    
    #region MOVEMENT - Система перемещения
    
    private void MoveTowards(Vector3 targetPosition)
    {
<<<<<<< HEAD
        if (isDashing || !isAlive || isInDamageAnimation) return; // Не двигаемся во время анимации урона
=======
        if (isDashing || !isAlive) return;
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
        
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // Игнорируем вертикальную составляющую
        
        // Проверяем, нужно ли двигаться
        float distance = Vector3.Distance(transform.position, targetPosition);
        
        if (distance > patrolPointThreshold)
        {
            // Простое движение через transform.position (без физики)
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }
    
    private void MoveAwayFrom(Vector3 targetPosition)
    {
<<<<<<< HEAD
        if (isDashing || !isAlive || isInDamageAnimation) return; // Не двигаемся во время анимации урона
=======
        if (isDashing || !isAlive) return;
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
        
        Vector3 direction = (transform.position - targetPosition).normalized;
        direction.y = 0; // Игнорируем вертикальную составляющую
        
        // Проверяем, нужно ли двигаться
        float distance = Vector3.Distance(transform.position, targetPosition);
        
        if (distance < preferredAttackDistance - 0.2f)
        {
            // Отступаем от цели
            transform.position += direction * moveSpeed * Time.deltaTime;
        }
    }
    
    private void RotateTowards(Vector3 targetPosition)
    {
<<<<<<< HEAD
        if (!isAlive || isInDamageAnimation) return; // Не поворачиваемся во время анимации урона
=======
        if (!isAlive) return;
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
        
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
        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }
    }
    
    /// <summary>
    /// Сброс состояния противника (для перезапуска уровня)
    /// </summary>
    public void ResetEnemy()
    {
        currentHealth = maxHealth;
        isAlive = true;
        isMoving = false;
        isRegroup = false;
        isAttackingAnim = false;
        isAttackingDashAnim = false;
<<<<<<< HEAD
        isDeathAnim = false;
        isDamagedAnim = false;
        isInDamageAnimation = false;
=======
        isDeathAnim = false; // ДОБАВЛЕНО: сбрасываем анимацию смерти
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
        attackCounter = 0;
        playerLostTimer = 0f;
        hasLastKnownPosition = false;
        transform.position = startPosition;
        transform.rotation = startRotation;
        
        // Обновляем аниматор
<<<<<<< HEAD
        ForceAnimatorUpdate();
=======
        UpdateAnimator();
>>>>>>> 66c528309b7f71e3333276635de487da56eaa41d
        
        // Обновляем HP бар
        UpdateHealthBarValue();
        ShowHealthBarTemporarily(healthBarShowDuration);
        
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = true;
        }
        
        SwitchState(AIState.Patrol);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
    
    #endregion
}
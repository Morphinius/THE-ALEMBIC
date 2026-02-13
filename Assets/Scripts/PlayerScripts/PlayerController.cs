using System.Collections;
using UnityEngine;

public class Player1 : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkspeed = 5f;
    public float shiftspeed = 10f;
    
    [Header("Roll Settings")]
    public float rollDistance = 3f;
    public float rollSpeed = 10f;
    public float rollDuration = 0.5f;
    public float rollCooldown = 1f;
    public bool rollStopsAttack = true;
    public bool canRollDuringAttack = false;
    public bool isInvulnerableDuringRoll = true;
    public LayerMask rollObstacleMask;
    public KeyCode rollKey = KeyCode.F;
    
    [Header("Combat Settings")]
    public float attackRange = 2f;
    public float attackAngle = 90f;
    public float attackCooldown = 1f;
    public float attackDamage = 10f;
    public float attackHitDelay = 0.3f;
    public bool stopMovementDuringAttack = true;
    
    [Header("Attack Animation Settings")]
    [Tooltip("Шанс на первую анимацию атаки (IsAttacking1)")]
    [Range(0, 100)] public float attackAnimation1Chance = 60f;
    [Tooltip("Шанс на вторую анимацию атаки (IsAttacking2)")]
    [Range(0, 100)] public float attackAnimation2Chance = 20f;
    [Tooltip("Шанс на третью анимацию атаки (IsAttacking3)")]
    [Range(0, 100)] public float attackAnimation3Chance = 20f;
    [Tooltip("Задержка перед ударом для второй анимации")]
    public float attack2HitDelay = 0.4f;
    [Tooltip("Задержка перед ударом для третьей анимации")]
    public float attack3HitDelay = 0.5f;
    
    [Header("Attack Visual")]
    public Color attackZoneColor = new Color(1f, 0f, 0f, 0.3f);
    public bool showAttackZone = true;
    
    [Header("Roll Visual")]
    public Color rollTrailColor = new Color(0f, 1f, 1f, 0.5f);
    public GameObject rollEffectPrefab;
    public Transform rollEffectSpawnPoint;
    
    [Header("Sword Trail")]
    public SwordTrailController swordTrail;
    public bool enableSwordTrail = true;
    
    [Header("References")]
    public GameObject Chest1;
    public GameObject DistillCube;
    public LayerMask enemyLayer;
    public string enemyTag = "Enemy";
    
    // Private variables
    private Vector3 target;
    private Vector3 targetchest;
    private Vector3 targetDistillCube;
    private bool ischest;
    private bool isDistillCube;
    private bool pickup = false;
    private bool isMoving = false;
    private bool shiftCooldown = true;
    private bool canAttack = true;
    private bool isAttacking = false;
    private float originalWalkspeed;
    
    // Roll variables
    private bool isRolling = false;
    private bool canRoll = true;
    private TrailRenderer rollTrail;
    
    // Новые переменные для управления движением после действий
    private bool movementPaused = false;
    private Vector3 pausePosition;
    
    // Health reference
    private PlayerHealth playerHealth;
    
    // Components
    private Animator _anim;
    private Rigidbody _rb;
    private Camera _mainCamera;
    private Collider _playerCollider;

    // Новые переменные для управления движением
    private Vector3 movementInput;
    private bool isGrounded = true;

    private void Start()
    {
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
        _mainCamera = Camera.main;
        originalWalkspeed = walkspeed;
        
        // Нормализуем шансы анимаций
        NormalizeAttackAnimationChances();
        
        // Получаем ссылку на скрипт здоровья
        playerHealth = GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = GetComponentInChildren<PlayerHealth>();
            if (playerHealth == null)
            {
                Debug.LogWarning("PlayerHealth не найден! Неуязвимость во время переката может не работать.");
            }
        }
        
        _playerCollider = GetComponent<Collider>();
        if (_playerCollider == null)
            _playerCollider = GetComponentInChildren<Collider>();

        if (_rb == null)
        {
            _rb = gameObject.AddComponent<Rigidbody>();
        }
        _rb.freezeRotation = true;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.useGravity = true;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        
        CreateRollTrail();

        Chest1 = GameObject.FindGameObjectWithTag("Chest");
        DistillCube = GameObject.FindGameObjectWithTag("DistillCube");

        if (swordTrail == null && enableSwordTrail)
        {
            swordTrail = GetComponentInChildren<SwordTrailController>();
        }
    }

    private void Update()
    {
        HandleInput();
        HandleAnimations();
        HandleDash();
        HandlePickup();
        HandleAttack();
        HandleRoll();
    }

    private void FixedUpdate()
    {
        // Проверяем, что персонаж на земле
        CheckGrounded();
        
        if (!isRolling && !movementPaused)
        {
            HandleMovement();
        }
    }

    private void CheckGrounded()
    {
        RaycastHit hit;
        float groundCheckDistance = 0.2f;
        Vector3 rayStart = transform.position + Vector3.up * 0.1f;
        
        isGrounded = Physics.Raycast(rayStart, Vector3.down, out hit, groundCheckDistance);
        
        // Если персонаж в воздухе, применяем гравитацию
        if (!isGrounded)
        {
            _rb.AddForce(Vector3.down * 20f, ForceMode.Acceleration);
        }
    }

    private void HandleInput()
    {
        // Если персонаж атакует или перекатывается, не обрабатываем движение мышкой
        if ((isAttacking && stopMovementDuringAttack) || isRolling)
        {
            if (Input.GetMouseButtonDown(1))
            {
                ClearMovementTarget();
            }
            return;
        }
        
        // Если движение было приостановлено и мы получаем новый клик мышкой
        if (movementPaused && Input.GetMouseButtonDown(1))
        {
            // Снимаем паузу при новом указании цели
            movementPaused = false;
        }
        
        if (Input.GetMouseButton(1))
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit, 100))
            {
                if (hit.collider.CompareTag("Ground"))
                {
                    SetTarget(hit.point, false, false);
                }
                else if (hit.collider.CompareTag("Chest"))
                {
                    SetTarget(hit.point - transform.forward * 1f, true, false);
                    targetchest = hit.point;
                }
                else if (hit.collider.CompareTag("DistillCube"))
                {
                    SetTarget(hit.point - transform.forward * 1f, false, true);
                    targetDistillCube = hit.point;
                }
            }
        }
    }

    private void HandleRoll()
    {
        if (Input.GetKeyDown(rollKey) && canRoll && !isRolling)
        {
            if (isAttacking && !canRollDuringAttack)
            {
                return;
            }
            
            // Получаем направление переката
            Vector3 rollDirection = GetMouseDirection();
            
            if (rollDirection == Vector3.zero && isMoving)
            {
                // Если не навели мышкой, используем направление движения
                rollDirection = (target - transform.position).normalized;
            }
            
            if (rollDirection == Vector3.zero)
            {
                // Если все еще нулевое, используем направление взгляда
                rollDirection = transform.forward;
            }
            
            StartCoroutine(RollCoroutine(rollDirection.normalized));
        }
    }

    private IEnumerator RollCoroutine(Vector3 direction)
    {
        canRoll = false;
        isRolling = true;
        
        // Приостанавливаем движение перед перекатом
        PauseMovement();
        
        if (isAttacking && rollStopsAttack)
        {
            InterruptAttack();
        }
        
        // Разворачиваем персонажа в сторону направления переката
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
        
        // Устанавливаем неуязвимость
        if (isInvulnerableDuringRoll && playerHealth != null)
        {
            SetDamageImmunity(true);
        }
        
        _anim.SetBool("Roll", true);
        EnableRollTrail(true);
        
        if (rollEffectPrefab != null)
        {
            Vector3 spawnPoint = rollEffectSpawnPoint != null ? 
                rollEffectSpawnPoint.position : transform.position;
            GameObject effect = Instantiate(rollEffectPrefab, spawnPoint, Quaternion.identity);
            effect.transform.rotation = Quaternion.LookRotation(direction);
            Destroy(effect, 2f);
        }
        
        // Полностью останавливаем Rigidbody перед началом переката
        _rb.linearVelocity = Vector3.zero;
        
        // Вычисляем конечную позицию с проверкой препятствий
        Vector3 rollEndPosition = transform.position + direction * rollDistance;
        
        if (rollObstacleMask != 0)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 0.5f, direction, 
                out hit, rollDistance, rollObstacleMask))
            {
                // Останавливаемся перед препятствием с небольшим зазором
                rollEndPosition = hit.point - direction * 0.3f;
            }
        }
        
        // Убедимся, что конечная позиция не ниже земли
        RaycastHit groundHit;
        if (Physics.Raycast(rollEndPosition + Vector3.up * 1f, Vector3.down, out groundHit, 2f))
        {
            rollEndPosition.y = groundHit.point.y;
        }
        else
        {
            rollEndPosition.y = transform.position.y;
        }
        
        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;
        
        // Временное отключение гравитации для плавного переката
        bool wasUsingGravity = _rb.useGravity;
        _rb.useGravity = false;
        
        while (elapsedTime < rollDuration && isRolling)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / rollDuration;
            
            // Используем SmoothStep для плавного движения
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            
            Vector3 newPosition = Vector3.Lerp(startPosition, rollEndPosition, smoothT);
            
            // Плавное перемещение Rigidbody
            Vector3 velocity = (newPosition - _rb.position) / Time.deltaTime;
            _rb.MovePosition(newPosition);
            
            yield return null;
        }
        
        // Восстанавливаем гравитацию
        _rb.useGravity = wasUsingGravity;
        
        // Завершаем перекат
        _anim.SetBool("Roll", false);
        EnableRollTrail(false);
        
        // Снимаем неуязвимость
        if (isInvulnerableDuringRoll && playerHealth != null)
        {
            SetDamageImmunity(false);
        }
        
        // Останавливаем движение после переката
        _rb.linearVelocity = Vector3.zero;
        isRolling = false;
        
        yield return new WaitForSeconds(0.1f);
        
        // Начинаем восстановление переката
        yield return new WaitForSeconds(rollCooldown);
        canRoll = true;
    }

    private void HandleAttack()
    {
        if (Input.GetKeyDown(KeyCode.Q) && Input.GetKey(KeyCode.Q) && canAttack && !isRolling)
        {
            // Получаем направление к курсору мыши
            Vector3 attackDirection = GetMouseDirection();
            
            // Разворачиваем персонажа в сторону курсора
            if (attackDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(attackDirection);
            }
            
            StartCoroutine(AttackCoroutine());
        }
    }

    private IEnumerator AttackCoroutine()
    {
        canAttack = false;
        isAttacking = true;
        
        // Приостанавливаем движение перед атакой
        PauseMovement();

        // Плавный разворот в сторону курсора (если еще не развернулись полностью)
        Vector3 attackDirection = GetMouseDirection();
        if (attackDirection != Vector3.zero && Vector3.Angle(transform.forward, attackDirection) > 5f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(attackDirection);
            float rotationTime = 0.1f; // Время для плавного разворота
            float elapsedTime = 0f;
            Quaternion startRotation = transform.rotation;
            
            while (elapsedTime < rotationTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / rotationTime;
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                yield return null;
            }
        }

        if (enableSwordTrail && swordTrail != null)
        {
            swordTrail.StartTrailWithDelayAndAutoStop();
        }
        
        // Выбираем случайную анимацию атаки на основе настроенных шансов
        AttackAnimationType selectedAnimation = SelectRandomAttackAnimation();
        
        // Определяем задержку удара в зависимости от выбранной анимации
        float currentHitDelay = GetHitDelayForAnimation(selectedAnimation);
        
        if (stopMovementDuringAttack)
        {
            // Останавливаем движение
            _rb.linearVelocity = Vector3.zero;
            
            float tempSpeed = walkspeed;
            walkspeed = 0f;
            
            // Активируем выбранную анимацию атаки
            ActivateAttackAnimation(selectedAnimation, true);
            
            yield return new WaitForSeconds(currentHitDelay);
            
            CheckForEnemiesInAttackZone();
            
            yield return new WaitForSeconds(0.1f);

            // Деактивируем анимацию атаки
            ActivateAttackAnimation(selectedAnimation, false);

            if (enableSwordTrail && swordTrail != null)
            {
                swordTrail.StopTrail();
            }
            
            walkspeed = tempSpeed;
            
            yield return new WaitForSeconds(0.1f);
            isAttacking = false;
            
            float remainingCooldown = attackCooldown - currentHitDelay - 0.2f;
            if (remainingCooldown > 0)
            {
                yield return new WaitForSeconds(remainingCooldown);
            }
        }
        else
        {
            // Активируем выбранную анимацию атаки
            ActivateAttackAnimation(selectedAnimation, true);
            
            yield return new WaitForSeconds(currentHitDelay);
            
            CheckForEnemiesInAttackZone();
            
            yield return new WaitForSeconds(0.1f);
            
            // Деактивируем анимацию атаки
            ActivateAttackAnimation(selectedAnimation, false);
            isAttacking = false;
            
            float remainingCooldown = attackCooldown - currentHitDelay - 0.1f;
            if (remainingCooldown > 0)
            {
                yield return new WaitForSeconds(remainingCooldown);
            }

            if (enableSwordTrail && swordTrail != null)
            {
                swordTrail.StopTrail();
            }
        }
        
        canAttack = true;
        
        // После атаки снимаем паузу движения
        if (movementPaused)
        {
            ResumeMovement();
        }
    }

    // Новый метод: выбор случайной анимации атаки
    private AttackAnimationType SelectRandomAttackAnimation()
    {
        float randomValue = Random.Range(0f, 100f);
        float cumulativeChance = 0f;
        
        // Проверяем первую анимацию
        cumulativeChance += attackAnimation1Chance;
        if (randomValue <= cumulativeChance)
        {
            return AttackAnimationType.Attack1;
        }
        
        // Проверяем вторую анимацию
        cumulativeChance += attackAnimation2Chance;
        if (randomValue <= cumulativeChance)
        {
            return AttackAnimationType.Attack2;
        }
        
        // Если не первые две, то третья
        return AttackAnimationType.Attack3;
    }

    // Новый метод: активация/деактивация анимации атаки
    private void ActivateAttackAnimation(AttackAnimationType animationType, bool activate)
    {
        // Сначала деактивируем все анимации атаки
        _anim.SetBool("IsAttacking1", false);
        _anim.SetBool("IsAttacking2", false);
        _anim.SetBool("IsAttacking3", false);
        
        // Активируем нужную анимацию
        switch (animationType)
        {
            case AttackAnimationType.Attack1:
                _anim.SetBool("IsAttacking1", activate);
                break;
            case AttackAnimationType.Attack2:
                _anim.SetBool("IsAttacking2", activate);
                break;
            case AttackAnimationType.Attack3:
                _anim.SetBool("IsAttacking3", activate);
                break;
        }
    }

    // Новый метод: получение задержки удара для конкретной анимации
    private float GetHitDelayForAnimation(AttackAnimationType animationType)
    {
        switch (animationType)
        {
            case AttackAnimationType.Attack1:
                return attackHitDelay;
            case AttackAnimationType.Attack2:
                return attack2HitDelay;
            case AttackAnimationType.Attack3:
                return attack3HitDelay;
            default:
                return attackHitDelay;
        }
    }

    // Новый метод: нормализация шансов анимаций
    private void NormalizeAttackAnimationChances()
    {
        float totalChance = attackAnimation1Chance + attackAnimation2Chance + attackAnimation3Chance;
        
        if (totalChance == 0)
        {
            // Если все шансы равны 0, устанавливаем равные шансы
            attackAnimation1Chance = attackAnimation2Chance = attackAnimation3Chance = 33.33f;
        }
        else if (totalChance != 100f)
        {
            // Нормализуем шансы, чтобы сумма была равна 100%
            float multiplier = 100f / totalChance;
            attackAnimation1Chance *= multiplier;
            attackAnimation2Chance *= multiplier;
            attackAnimation3Chance *= multiplier;
        }
    }

    // Новый метод: приостановка движения
    private void PauseMovement()
    {
        if (isMoving)
        {
            movementPaused = true;
            pausePosition = transform.position;
            isMoving = false;
            _rb.linearVelocity = Vector3.zero;
        }
    }

    // Новый метод: возобновление движения (вызывается при новом указании цели)
    private void ResumeMovement()
    {
        movementPaused = false;
    }

    // Новый метод: получение направления к курсору мыши
    private Vector3 GetMouseDirection()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, 100))
        {
            Vector3 direction = (hit.point - transform.position).normalized;
            direction.y = 0;
            
            // Проверяем, что направление не нулевое
            if (direction.magnitude > 0.01f)
            {
                return direction;
            }
        }
        
        // Если не получилось определить направление, возвращаем текущее направление персонажа
        return transform.forward;
    }

    private void SetDamageImmunity(bool immune)
    {
        if (playerHealth == null) return;
        
        if (immune)
        {
            playerHealth.SetCanTakeDamage(false);
        }
        else
        {
            playerHealth.SetCanTakeDamage(true);
        }
    }

    /// <summary>
    /// Проверяет, неуязвим ли игрок в данный момент
    /// </summary>
    public bool IsInvulnerable()
    {
        return (isRolling && isInvulnerableDuringRoll) || (playerHealth != null && !playerHealth.CanTakeDamage());
    }

    /// <summary>
    /// Возвращает, можно ли наносить урон игроку
    /// </summary>
    public bool CanTakeDamage()
    {
        if (playerHealth != null)
        {
            return playerHealth.CanTakeDamage();
        }
        return true;
    }

    private void CreateRollTrail()
    {
        GameObject trailObject = new GameObject("RollTrail");
        trailObject.transform.SetParent(transform);
        trailObject.transform.localPosition = Vector3.zero;
        
        rollTrail = trailObject.AddComponent<TrailRenderer>();
        
        rollTrail.startWidth = 0.3f;
        rollTrail.endWidth = 0.1f;
        rollTrail.time = 0.3f;
        rollTrail.material = new Material(Shader.Find("Sprites/Default"));
        rollTrail.startColor = rollTrailColor;
        rollTrail.endColor = new Color(rollTrailColor.r, rollTrailColor.g, rollTrailColor.b, 0f);
        
        rollTrail.enabled = false;
    }

    private void EnableRollTrail(bool enable)
    {
        if (rollTrail != null)
        {
            rollTrail.enabled = enable;
            if (!enable)
            {
                rollTrail.Clear();
            }
        }
    }

    private void SetTarget(Vector3 point, bool chest, bool distillCube)
    {
        // Если движение было приостановлено, снимаем паузу
        if (movementPaused)
        {
            ResumeMovement();
        }
        
        target = new Vector3(point.x, transform.position.y, point.z);
        ischest = chest;
        isDistillCube = distillCube;
        isMoving = true;
    }

    private void HandleMovement()
    {
        // Если движение приостановлено, не двигаемся
        if (movementPaused || isRolling)
        {
            if (!isRolling)
            {
                _rb.linearVelocity = new Vector3(0, _rb.linearVelocity.y, 0);
            }
            return;
        }
        
        if (isAttacking && stopMovementDuringAttack)
        {
            _rb.linearVelocity = new Vector3(0, _rb.linearVelocity.y, 0);
            return;
        }
        
        if (!isMoving)
        {
            _rb.linearVelocity = new Vector3(0, _rb.linearVelocity.y, 0);
            return;
        }

        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            // Плавный разворот
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);
        }

        Vector3 horizontalPosition = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 horizontalTarget = new Vector3(target.x, 0, target.z);
        float distance = Vector3.Distance(horizontalPosition, horizontalTarget);

        if (distance < 0.2f)
        {
            StopMovement();
            return;
        }

        // Плавное движение к цели
        Vector3 moveVelocity = direction * walkspeed;
        moveVelocity.y = _rb.linearVelocity.y;
        
        // Применяем скорость с учетом интерполяции
        Vector3 newPosition = _rb.position + moveVelocity * Time.fixedDeltaTime;
        
        // Проверяем, что не движемся слишком быстро
        if (moveVelocity.magnitude > walkspeed * 1.5f)
        {
            moveVelocity = moveVelocity.normalized * walkspeed;
        }
        
        _rb.linearVelocity = moveVelocity;
    }

    private void StopMovement()
    {
        isMoving = false;
        _rb.linearVelocity = new Vector3(0, _rb.linearVelocity.y, 0);
        
        if (ischest)
        {
            Vector3 lookDirection = (targetchest - transform.position).normalized;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(lookDirection);
        }
        else if (isDistillCube)
        {
            Vector3 lookDirection = (targetDistillCube - transform.position).normalized;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }
    
    private void ClearMovementTarget()
    {
        isMoving = false;
        ischest = false;
        isDistillCube = false;
        _rb.linearVelocity = new Vector3(0, _rb.linearVelocity.y, 0);
        
        // Также снимаем паузу при явной отмене движения
        if (movementPaused)
        {
            ResumeMovement();
        }
    }

    private void HandleAnimations()
    {
        // Устанавливаем анимацию ходьбы только если не атакуем, не перекатываемся и движение не приостановлено
        _anim.SetBool("walk", isMoving && !isAttacking && !isRolling && !movementPaused);
    }

    private void HandleDash()
    {
        if (isAttacking || isRolling || movementPaused) return;
        
        if (isMoving && Input.GetKeyDown(KeyCode.LeftShift) && shiftCooldown)
        {
            StartCoroutine(DashCoroutine());
        }
    }

    private void HandlePickup()
    {
        if (isAttacking || isRolling || movementPaused) return;
        
        if (pickup && Input.GetKey(KeyCode.E))
        {
            _anim.SetBool("pickup", true);
            StartCoroutine(PickupCoroutine());
        }
    }

    private void CheckForEnemiesInAttackZone()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        
        foreach (GameObject enemy in enemies)
        {
            if (IsEnemyInAttackZone(enemy))
            {
                ApplyDamage(enemy);
            }
        }
    }

    private bool IsEnemyInAttackZone(GameObject enemy)
    {
        if (enemy == null) return false;
        
        Vector3 directionToEnemy = enemy.transform.position - transform.position;
        directionToEnemy.y = 0;
        
        float distanceToEnemy = directionToEnemy.magnitude;
        
        if (distanceToEnemy > attackRange) return false;
        
        float angleToEnemy = Vector3.Angle(transform.forward, directionToEnemy.normalized);
        
        if (angleToEnemy > attackAngle / 2) return false;
        
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        Vector3 rayDirection = enemy.transform.position - rayOrigin;
        
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, attackRange))
        {
            return hit.collider.gameObject == enemy || 
                   hit.collider.transform.IsChildOf(enemy.transform);
        }
        
        return false;
    }

    private void ApplyDamage(GameObject enemy)
    {
        enemy.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);
        Debug.Log($"Нанесено {attackDamage} урона врагу: {enemy.name}");
    }

    private IEnumerator DashCoroutine()
    {
        shiftCooldown = false;
        float originalSpeed = walkspeed;
        walkspeed = shiftspeed;
        _anim.SetBool("shift", true);
        
        yield return new WaitForSeconds(1f);
        
        walkspeed = originalSpeed;
        _anim.SetBool("shift", false);
        
        yield return new WaitForSeconds(5f);
        shiftCooldown = true;
    }

    private IEnumerator PickupCoroutine()
    {
        yield return new WaitForSeconds(1f);
        _anim.SetBool("pickup", false);
        yield return new WaitForSeconds(1f);
        pickup = false;
    }

    public void PerformAttack()
    {
        if (canAttack && !isRolling)
        {
            // Получаем направление к курсору мыши
            Vector3 attackDirection = GetMouseDirection();
            
            // Разворачиваем персонажа в сторону курсора
            if (attackDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(attackDirection);
            }
            
            StartCoroutine(AttackCoroutine());
        }
    }

    public bool CanAttack()
    {
        return canAttack;
    }

    public bool IsAttackingNow()
    {
        return isAttacking;
    }

    public void InterruptAttack()
    {
        if (isAttacking)
        {
            StopAllCoroutines();
            isAttacking = false;
            canAttack = true;
            
            // Деактивируем все анимации атаки
            _anim.SetBool("IsAttacking1", false);
            _anim.SetBool("IsAttacking2", false);
            _anim.SetBool("IsAttacking3", false);
            
            if (enableSwordTrail && swordTrail != null)
            {
                swordTrail.StopTrail();
            }

            if (stopMovementDuringAttack)
            {
                walkspeed = originalWalkspeed;
            }
            
            // При прерывании атаки также снимаем паузу движения
            if (movementPaused)
            {
                ResumeMovement();
            }
        }
    }

    public bool IsRolling()
    {
        return isRolling;
    }

    public bool CanRoll()
    {
        return canRoll;
    }

    // Перечисление для типов анимаций атаки
    private enum AttackAnimationType
    {
        Attack1,
        Attack2,
        Attack3
    }

    private void OnDrawGizmosSelected()
    {
        if (!showAttackZone) return;
        
        Gizmos.color = attackZoneColor;
        
        Vector3 forward = transform.forward * attackRange;
        Vector3 leftBoundary = Quaternion.Euler(0, -attackAngle / 2, 0) * forward;
        Vector3 rightBoundary = Quaternion.Euler(0, attackAngle / 2, 0) * forward;
        
        Gizmos.DrawRay(transform.position, leftBoundary);
        Gizmos.DrawRay(transform.position, rightBoundary);
        
        Vector3 previousPoint = transform.position + leftBoundary;
        for (int i = 1; i <= 20; i++)
        {
            float t = i / 20f;
            float angle = Mathf.Lerp(-attackAngle / 2, attackAngle / 2, t);
            Vector3 point = transform.position + Quaternion.Euler(0, angle, 0) * forward;
            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }
        
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
    }
    
    // Метод для обновления настроек анимаций в редакторе
    private void OnValidate()
    {
        NormalizeAttackAnimationChances();
    }
}
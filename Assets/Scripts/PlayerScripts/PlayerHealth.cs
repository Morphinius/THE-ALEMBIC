using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Основные настройки здоровья")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    
    [Header("Ссылки на UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;
    
    [Header("Визуальные эффекты")]
    [SerializeField] private Image damageFlash;
    [SerializeField] private float flashDuration = 0.2f;
    
    [Header("Регенерация здоровья")]
    [SerializeField] private bool enableRegeneration = true;
    [SerializeField] private float regenerationRate = 2f;
    [SerializeField] private float regenerationDelay = 5f;
    
    [Header("Защита от урона")]
    [SerializeField] public bool canTakeDamage = true;
    [SerializeField] private float damageCooldown = 1f;
    
    // Приватные переменные
    private bool isDead = false;
    private bool canBeDamaged = true;
    private float timeSinceLastDamage = 0f;
    private Coroutine regenerationCoroutine;
    private Coroutine damageCooldownCoroutine;
    
    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();
        
        if (damageFlash != null)
        {
            damageFlash.color = new Color(damageFlash.color.r, damageFlash.color.g, damageFlash.color.b, 0);
        }
        
        timeSinceLastDamage = regenerationDelay;
    }

    void Update()
    {
        // Тестовый урон по клавише K
        if (Input.GetKeyDown(KeyCode.K))
        {
            TakeDamage(10);
        }
        
        // Обновление таймера для регенерации
        if (!isDead && enableRegeneration)
        {
            timeSinceLastDamage += Time.deltaTime;
            
            if (timeSinceLastDamage >= regenerationDelay && currentHealth < maxHealth)
            {
                Heal(regenerationRate * Time.deltaTime);
            }
        }
    }

    // Основной метод получения урона
    public void TakeDamage(float damage)
    {
        // Проверяем, можно ли получать урон (например, во время переката)
        if (!canTakeDamage)
        {
            Debug.Log("Урон заблокирован: игрок неуязвим!");
            return;
        }
        
        if (isDead || !canBeDamaged) return;
        
        // Включаем кд на получение урона
        if (damageCooldownCoroutine != null)
        {
            StopCoroutine(damageCooldownCoroutine);
        }
        damageCooldownCoroutine = StartCoroutine(DamageCooldown());
        
        // Сбрасываем таймер регенерации
        timeSinceLastDamage = 0f;
        
        // Останавливаем активную корутину регенерации
        if (regenerationCoroutine != null)
        {
            StopCoroutine(regenerationCoroutine);
        }
        
        // Уменьшаем здоровье
        currentHealth -= damage;
        
        // Проверяем смерть
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }
        
        // Обновляем UI
        UpdateHealthUI();
        
        // Эффект получения урона
        if (damageFlash != null)
        {
            StartCoroutine(DamageFlashEffect());
        }
        
        // Запускаем регенерацию после задержки (если включена)
        if (enableRegeneration && !isDead)
        {
            regenerationCoroutine = StartCoroutine(DelayedRegeneration());
        }
        
        Debug.Log($"Получено урона: {damage}. Текущее здоровье: {currentHealth}");
    }

    // Кд между получениями урона
    IEnumerator DamageCooldown()
    {
        canBeDamaged = false;
        yield return new WaitForSeconds(damageCooldown);
        canBeDamaged = true;
        damageCooldownCoroutine = null;
    }

    // Регенерация после задержки
    IEnumerator DelayedRegeneration()
    {
        yield return new WaitForSeconds(regenerationDelay);
        
        while (currentHealth < maxHealth && !isDead)
        {
            Heal(regenerationRate * Time.deltaTime);
            yield return null;
        }
    }

    // Метод для лечения
    public void Heal(float healAmount)
    {
        if (isDead) return;
        
        currentHealth += healAmount;
        
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        
        UpdateHealthUI();
    }

    // Мгновенное лечение на определенное количество
    public void InstantHeal(float healAmount)
    {
        if (isDead) return;
        
        currentHealth += healAmount;
        
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        
        UpdateHealthUI();
        Debug.Log($"Вылечено: {healAmount}. Текущее здоровье: {currentHealth}");
    }

    // Обновление интерфейса
    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
        
        if (healthText != null)
        {
            healthText.text = $"{Mathf.Round(currentHealth)} / {maxHealth}";
        }
    }

    // Эффект мигания при получении урона
    private IEnumerator DamageFlashEffect()
    {
        if (damageFlash == null) yield break;
        
        damageFlash.color = new Color(1f, 0f, 0f, 0.3f);
        yield return new WaitForSeconds(flashDuration);
        damageFlash.color = new Color(1f, 0f, 0f, 0f);
    }

    // Обработка смерти
    private void Die()
    {
        isDead = true;
        Debug.Log("Игрок умер!");
    }

    // === ПУБЛИЧНЫЕ МЕТОДЫ ДЛЯ НАСТРОЙКИ ИЗ ДРУГИХ СКРИПТОВ ===
    
    // Геттеры
    public float GetCurrentHealth() { return currentHealth; }
    public float GetMaxHealth() { return maxHealth; }
    public bool IsDead() { return isDead; }
    public float GetRegenerationRate() { return regenerationRate; }
    public float GetRegenerationDelay() { return regenerationDelay; }
    public bool CanBeDamaged() { return canBeDamaged; }
    
    // Новый геттер для canTakeDamage
    public bool CanTakeDamage() { return canTakeDamage; }
    
    // Сеттеры для динамической настройки
    public void SetMaxHealth(float newMaxHealth)
    {
        maxHealth = newMaxHealth;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
        UpdateHealthUI();
    }
    
    public void SetRegenerationRate(float newRate)
    {
        regenerationRate = newRate;
    }
    
    public void SetRegenerationDelay(float newDelay)
    {
        regenerationDelay = newDelay;
    }
    
    public void EnableRegeneration(bool enable)
    {
        enableRegeneration = enable;
    }
    
    public void SetCanTakeDamage(bool canTake)
    {
        canTakeDamage = canTake;
        Debug.Log($"canTakeDamage установлен в: {canTakeDamage}");
    }
    
    public void SetDamageCooldown(float newCooldown)
    {
        damageCooldown = newCooldown;
    }
}
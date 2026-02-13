using UnityEngine;

public class SwordTrailController : MonoBehaviour
{
    [Header("Trail Settings")]
    public GameObject swordTip; // Точка кончика меча (должна быть присвоена в инспекторе)
    public float trailWidth = 0.1f;
    public float trailTime = 0.3f; // Длина трейла в секундах
    public Color trailStartColor = Color.white;
    public Color trailEndColor = new Color(1f, 1f, 1f, 0f);
    public Material trailMaterial;
    
    [Header("Timing Settings")]
    public float trailStartDelay = 0.1f; // Задержка перед началом трейла (синхронизация с анимацией)
    public float trailAutoStopTime = 0.3f; // Через сколько секунд автоматически остановить трейл
    public float trailFadeOutTime = 0.2f; // Время затухания трейла после остановки
    
    [Header("Debug")]
    public bool enableDebug = false; // Включить отладочные сообщения
    
    // Components
    private TrailRenderer trailRenderer;
    private Coroutine autoStopCoroutine;
    private Coroutine startWithDelayCoroutine;
    
    void Start()
    {
        // Создаем TrailRenderer если его нет
        trailRenderer = swordTip.GetComponent<TrailRenderer>();
        if (trailRenderer == null)
        {
            trailRenderer = swordTip.AddComponent<TrailRenderer>();
        }
        
        // Настраиваем TrailRenderer
        ConfigureTrailRenderer();
        
        // Отключаем трейл в начале
        DisableTrailImmediately();
    }
    
    void Update()
    {
        // Отладочное управление трейлом
        if (enableDebug)
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                StartTrailWithAutoStop();
            }
            
            if (Input.GetKeyDown(KeyCode.Y))
            {
                StopTrail();
            }
            
            if (Input.GetKeyDown(KeyCode.U))
            {
                StartTrailForDuration(1.0f);
            }
        }
    }
    
    /// <summary>
    /// Настройка TrailRenderer
    /// </summary>
    private void ConfigureTrailRenderer()
    {
        trailRenderer.time = trailTime;
        trailRenderer.startWidth = trailWidth;
        trailRenderer.endWidth = trailWidth * 0.1f;
        trailRenderer.minVertexDistance = 0.01f;
        
        // Настройка градиента цвета
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] 
            {
                new GradientColorKey(trailStartColor, 0.0f),
                new GradientColorKey(trailStartColor, 0.7f),
                new GradientColorKey(trailEndColor, 1.0f)
            },
            new GradientAlphaKey[] 
            {
                new GradientAlphaKey(1.0f, 0.0f),
                new GradientAlphaKey(1.0f, 0.7f),
                new GradientAlphaKey(0.0f, 1.0f)
            }
        );
        
        trailRenderer.colorGradient = gradient;
        
        // Настройка материала
        if (trailMaterial != null)
        {
            trailRenderer.material = trailMaterial;
        }
        else
        {
            // Используем стандартный материал если не назначен
            trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
        }
        
        // Дополнительные настройки
        trailRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        trailRenderer.receiveShadows = false;
        trailRenderer.autodestruct = false;
    }
    
    /// <summary>
    /// Запустить трейл меча
    /// </summary>
    public void StartTrail()
    {
        if (trailRenderer == null || swordTip == null) return;
        
        // Включаем трейл
        trailRenderer.emitting = true;
        trailRenderer.enabled = true;
        
        if (enableDebug) Debug.Log("Sword Trail started");
    }
    
    /// <summary>
    /// Запустить трейл с автоматической остановкой через заданное время
    /// </summary>
    public void StartTrailWithAutoStop()
    {
        StartTrail();
        
        // Запускаем корутину автоматической остановки
        if (autoStopCoroutine != null)
        {
            StopCoroutine(autoStopCoroutine);
        }
        
        autoStopCoroutine = StartCoroutine(AutoStopTrailCoroutine());
    }
    
    /// <summary>
    /// Запустить трейл на определенное время
    /// </summary>
    /// <param name="duration">Длительность трейла в секундах</param>
    public void StartTrailForDuration(float duration)
    {
        StartTrail();
        
        // Запускаем корутину остановки через указанное время
        if (autoStopCoroutine != null)
        {
            StopCoroutine(autoStopCoroutine);
        }
        
        autoStopCoroutine = StartCoroutine(StopTrailAfterDelay(duration));
    }
    
    /// <summary>
    /// Корутина для автоматической остановки трейла через заданное время
    /// </summary>
    private System.Collections.IEnumerator AutoStopTrailCoroutine()
    {
        // Ждем указанное время
        yield return new WaitForSeconds(trailAutoStopTime);
        
        // Останавливаем трейл
        StopTrail();
        
        autoStopCoroutine = null;
    }
    
    /// <summary>
    /// Корутина для остановки трейла через определенное время
    /// </summary>
    private System.Collections.IEnumerator StopTrailAfterDelay(float delay)
    {
        // Ждем указанное время
        yield return new WaitForSeconds(delay);
        
        // Останавливаем трейл
        StopTrail();
        
        autoStopCoroutine = null;
    }
    
    /// <summary>
    /// Остановить трейл меча с плавным затуханием
    /// </summary>
    public void StopTrail()
    {
        if (trailRenderer == null) return;
        
        // Отключаем эмиссию новых точек
        trailRenderer.emitting = false;
        
        if (enableDebug) Debug.Log("Sword Trail stopped");
        
        // Запускаем корутину для полного отключения после затухания
        StartCoroutine(DisableTrailAfterFade());
    }
    
    /// <summary>
    /// Немедленно отключить трейл (без затухания)
    /// </summary>
    public void DisableTrailImmediately()
    {
        if (trailRenderer == null) return;
        
        // Останавливаем все корутины
        if (autoStopCoroutine != null)
        {
            StopCoroutine(autoStopCoroutine);
            autoStopCoroutine = null;
        }
        
        if (startWithDelayCoroutine != null)
        {
            StopCoroutine(startWithDelayCoroutine);
            startWithDelayCoroutine = null;
        }
        
        trailRenderer.emitting = false;
        trailRenderer.enabled = false;
        trailRenderer.Clear(); // Очищаем существующие точки
        
        if (enableDebug) Debug.Log("Sword Trail disabled immediately");
    }
    
    /// <summary>
    /// Корутина для отключения трейла после затухания
    /// </summary>
    private System.Collections.IEnumerator DisableTrailAfterFade()
    {
        // Ждем время затухания
        yield return new WaitForSeconds(trailFadeOutTime);
        
        // Полностью отключаем трейл
        trailRenderer.enabled = false;
        trailRenderer.Clear();
    }
    
    /// <summary>
    /// Запустить трейл с задержкой (для синхронизации с анимацией)
    /// </summary>
    public void StartTrailWithDelay()
    {
        // Отменяем предыдущую корутину если она есть
        if (startWithDelayCoroutine != null)
        {
            StopCoroutine(startWithDelayCoroutine);
        }
        
        startWithDelayCoroutine = StartCoroutine(StartTrailDelayedCoroutine());
    }
    
    /// <summary>
    /// Запустить трейл с задержкой и автоматической остановкой
    /// </summary>
    public void StartTrailWithDelayAndAutoStop()
    {
        // Отменяем предыдущую корутину если она есть
        if (startWithDelayCoroutine != null)
        {
            StopCoroutine(startWithDelayCoroutine);
        }
        
        startWithDelayCoroutine = StartCoroutine(StartTrailDelayedWithAutoStopCoroutine());
    }
    
    private System.Collections.IEnumerator StartTrailDelayedCoroutine()
    {
        // Ждем задержку перед запуском
        yield return new WaitForSeconds(trailStartDelay);
        
        // Запускаем трейл
        StartTrail();
        
        startWithDelayCoroutine = null;
    }
    
    private System.Collections.IEnumerator StartTrailDelayedWithAutoStopCoroutine()
    {
        // Ждем задержку перед запуском
        yield return new WaitForSeconds(trailStartDelay);
        
        // Запускаем трейл с автоматической остановкой
        StartTrailWithAutoStop();
        
        startWithDelayCoroutine = null;
    }
    
    /// <summary>
    /// Изменить время автоматической остановки во время выполнения
    /// </summary>
    public void SetAutoStopTime(float newTime)
    {
        trailAutoStopTime = newTime;
    }
    
    /// <summary>
    /// Изменить время затухания трейла
    /// </summary>
    public void SetFadeOutTime(float newTime)
    {
        trailFadeOutTime = newTime;
    }
    
    /// <summary>
    /// Проверяет, активен ли трейл в данный момент
    /// </summary>
    public bool IsTrailActive()
    {
        return trailRenderer != null && trailRenderer.emitting;
    }
    
    /// <summary>
    /// Получить TrailRenderer компонент
    /// </summary>
    public TrailRenderer GetTrailRenderer()
    {
        return trailRenderer;
    }
    
    /// <summary>
    /// Очистить все корутины при уничтожении объекта
    /// </summary>
    void OnDestroy()
    {
        if (autoStopCoroutine != null)
        {
            StopCoroutine(autoStopCoroutine);
        }
        
        if (startWithDelayCoroutine != null)
        {
            StopCoroutine(startWithDelayCoroutine);
        }
    }
}
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreeTransparencyController : MonoBehaviour
{
    [Header("Настройки прозрачности")]
    [SerializeField] private LayerMask obstacleLayerMask = -1;
    [SerializeField] private LayerMask groundLayerMask = 1; // Слой земли
    [SerializeField] [Range(0.01f, 0.3f)] private float transparencyAlpha = 0.05f;
    [SerializeField] private float fadeDuration = 0.15f;
    
    [Header("Настройки обнаружения")]
    [SerializeField] private int verticalRays = 5; // Количество лучей по вертикали
    [SerializeField] private float playerHeight = 2f; // Высота персонажа
    [SerializeField] private float checkRadius = 0.5f; // Радиус сферы для лучшего обнаружения
    
    private Transform playerTransform;
    private Camera mainCamera;
    private Dictionary<Renderer, Material[]> originalMaterials = new Dictionary<Renderer, Material[]>();
    private HashSet<Renderer> currentTransparentObjects = new HashSet<Renderer>();
    
    void Start()
    {
        playerTransform = transform;
        mainCamera = Camera.main;
    }
    
    void Update()
    {
        CheckObstaclesWithVolume();
    }
    
    private void CheckObstaclesWithVolume()
    {
        Vector3 cameraPos = mainCamera.transform.position;
        Vector3 playerBasePos = playerTransform.position;
        
        HashSet<Renderer> newTransparentObjects = new HashSet<Renderer>();
        
        // Метод 1: Множественные лучи по высоте персонажа
        for (int i = 0; i < verticalRays; i++)
        {
            float heightRatio = (float)i / (verticalRays - 1);
            Vector3 rayStart = playerBasePos + Vector3.up * (heightRatio * playerHeight);
            Vector3 direction = cameraPos - rayStart;
            float distance = direction.magnitude;
            
            RaycastHit[] hits = Physics.RaycastAll(rayStart, direction.normalized, distance, obstacleLayerMask);
            
            foreach (RaycastHit hit in hits)
            {
                Renderer renderer = hit.collider.GetComponent<Renderer>();
                if (renderer != null && renderer.transform != playerTransform)
                {
                    newTransparentObjects.Add(renderer);
                    // Добавляем все рендереры от этого объекта
                    AddAllRenderersFromObject(hit.collider.gameObject, newTransparentObjects);
                }
            }
        }
        
        // Метод 2: SphereCast для объемного обнаружения
        Vector3 playerCenter = playerBasePos + Vector3.up * (playerHeight * 0.5f);
        Vector3 castDirection = cameraPos - playerCenter;
        float castDistance = castDirection.magnitude;
        
        RaycastHit[] sphereHits = Physics.SphereCastAll(playerCenter, checkRadius, castDirection.normalized, castDistance, obstacleLayerMask);
        
        foreach (RaycastHit hit in sphereHits)
        {
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            if (renderer != null && renderer.transform != playerTransform)
            {
                newTransparentObjects.Add(renderer);
                AddAllRenderersFromObject(hit.collider.gameObject, newTransparentObjects);
            }
        }
        
        // Метод 3: Проверка bounding box пересечения
        CheckBoundingBoxIntersection(cameraPos, playerBasePos, newTransparentObjects);
        
        // Обновляем прозрачность
        UpdateTransparency(newTransparentObjects);
    }

    private void AddAllRenderersFromObject(GameObject obj, HashSet<Renderer> renderers)
    {
        Renderer[] allRenderers = obj.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in allRenderers)
        {
            renderers.Add(renderer);
        }
    }
    private void CheckBoundingBoxIntersection(Vector3 cameraPos, Vector3 playerPos, HashSet<Renderer> renderers)
    {
        // Создаем луч от камеры к персонажу
        Ray cameraToPlayerRay = new Ray(cameraPos, (playerPos - cameraPos).normalized);
        float distanceToPlayer = Vector3.Distance(cameraPos, playerPos);
        
        // Проверяем все объекты на слое препятствий
        Collider[] allObstacles = Physics.OverlapSphere(playerPos, 10f, obstacleLayerMask);
        
        foreach (Collider collider in allObstacles)
        {
            if (collider.transform == playerTransform) continue;
            
            Renderer renderer = collider.GetComponent<Renderer>();
            if (renderer != null && renderer.bounds.IntersectRay(cameraToPlayerRay, out float intersectionDistance))
            {
                if (intersectionDistance < distanceToPlayer)
                {
                    renderers.Add(renderer);
                    AddAllRenderersFromObject(collider.gameObject, renderers);
                }
            }
        }
    }
    
    private void UpdateTransparency(HashSet<Renderer> newTransparentObjects)
    {
        // Восстанавливаем объекты, которые больше не перекрывают
        foreach (Renderer renderer in currentTransparentObjects)
        {
            if (!newTransparentObjects.Contains(renderer))
            {
                MakeOpaque(renderer);
            }
        }
        
        // Делаем прозрачными новые объекты
        foreach (Renderer renderer in newTransparentObjects)
        {
            if (!currentTransparentObjects.Contains(renderer))
            {
                MakeTransparent(renderer);
            }
        }
        
        currentTransparentObjects = newTransparentObjects;
    }
    
    private void MakeTransparent(Renderer renderer)
    {
        if (originalMaterials.ContainsKey(renderer)) return;
        
        originalMaterials[renderer] = renderer.materials;
        
        Material[] transparentMats = new Material[renderer.materials.Length];
        for (int i = 0; i < renderer.materials.Length; i++)
        {
            transparentMats[i] = CreateTransparentMaterial(renderer.materials[i]);
        }
        
        renderer.materials = transparentMats;
        
        StartCoroutine(FadeToAlpha(renderer, transparentMats, 1f, transparencyAlpha));
    }
    
    private Material CreateTransparentMaterial(Material original)
    {
        Material transparentMat = new Material(original);
        
        if (transparentMat.HasProperty("_Surface"))
        {
            transparentMat.SetFloat("_Surface", 1);
            transparentMat.SetOverrideTag("RenderType", "Transparent");
            
            if (transparentMat.HasProperty("_BaseColor"))
            {
                Color baseColor = transparentMat.GetColor("_BaseColor");
                baseColor.a = transparencyAlpha;
                transparentMat.SetColor("_BaseColor", baseColor);
            }
        }
        
        transparentMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        transparentMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        transparentMat.SetInt("_ZWrite", 0);
        transparentMat.EnableKeyword("_ALPHABLEND_ON");
        transparentMat.renderQueue = 3000;
        
        Color color = transparentMat.color;
        color.a = transparencyAlpha;
        transparentMat.color = color;
        
        return transparentMat;
    }
    
    private void MakeOpaque(Renderer renderer)
    {
        if (!originalMaterials.ContainsKey(renderer)) return;
        
        if (renderer != null)
        {
            renderer.materials = originalMaterials[renderer];
        }
        originalMaterials.Remove(renderer);
        }
    
    private IEnumerator FadeToAlpha(Renderer renderer, Material[] materials, float startAlpha, float targetAlpha)
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeDuration && renderer != null)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            
            foreach (Material material in materials)
            {
                if (material != null)
                {
                    SetMaterialAlpha(material, currentAlpha);
                }
            }
            
            yield return null;
        }
    }
    
    private void SetMaterialAlpha(Material material, float alpha)
    {
        if (material.HasProperty("_Color"))
        {
            Color color = material.color;
            color.a = alpha;
            material.color = color;
        }
        
        if (material.HasProperty("_BaseColor"))
        {
            Color baseColor = material.GetColor("_BaseColor");
            baseColor.a = alpha;
            material.SetColor("_BaseColor", baseColor);
        }
    }
    
    void OnDestroy()
    {
        foreach (var kvp in originalMaterials)
        {
            if (kvp.Key != null)
            {
                kvp.Key.materials = kvp.Value;
            }
        }
    }
    
    // Для отладки - визуализация лучей в Scene View
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;
        
        Vector3 cameraPos = mainCamera.transform.position;
        Vector3 playerBasePos = playerTransform.position;
        
        // Рисуем лучи
        Gizmos.color = Color.red;
        for (int i = 0; i < verticalRays; i++)
        {
            float heightRatio = (float)i / (verticalRays - 1);
            Vector3 rayStart = playerBasePos + Vector3.up * (heightRatio * playerHeight);
            Gizmos.DrawLine(rayStart, cameraPos);
        }
        
        // Рисуем SphereCast
        Gizmos.color = Color.yellow;
        Vector3 playerCenter = playerBasePos + Vector3.up * (playerHeight * 0.5f);
        Gizmos.DrawWireSphere(playerCenter, checkRadius);
    }
}
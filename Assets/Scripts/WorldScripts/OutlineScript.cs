using System.Collections.Generic;
using UnityEngine;

public class MaterialHighlightController : MonoBehaviour
{
    [System.Serializable]
    public class HighlightableObject
    {
        public GameObject targetObject;
        public Material highlightMaterial;
        [ColorUsage(true, true)]
        public Color highlightColor = Color.yellow;
        [Min(0.01f)]
        public float outlineWidth = 0.05f;
    }

    [Header("Highlight Settings")]
    public List<HighlightableObject> highlightableObjects = new List<HighlightableObject>();

    [Header("Raycast Settings")]
    public float raycastDistance = 100f;
    public LayerMask interactionLayer = -1;

    private Camera mainCamera;
    private GameObject currentHighlightedObject;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        HandleMouseHighlight();
    }

    void HandleMouseHighlight()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        GameObject hitObject = null;
        HighlightableObject highlightable = null;

        // Проверяем, попал ли луч в какой-либо объект
        if (Physics.Raycast(ray, out hit, raycastDistance, interactionLayer))
        {
            hitObject = hit.collider.gameObject;
            highlightable = GetHighlightableObject(hitObject);
        }

        // Если навели на новый объект из списка подсветки
        if (highlightable != null && hitObject != currentHighlightedObject)
        {
            // Убираем подсветку с предыдущего объекта
            RemoveHighlight();
            
            // Добавляем подсветку на новый объект
            AddHighlight(hitObject, highlightable);
            currentHighlightedObject = hitObject;
        }
        // Если убрали курсор с объекта или навели на объект не из списка
        else if ((highlightable == null && currentHighlightedObject != null) || 
                 (hitObject == null && currentHighlightedObject != null))
        {
            RemoveHighlight();
        }
    }

    HighlightableObject GetHighlightableObject(GameObject target)
    {
        foreach (var obj in highlightableObjects)
        {
            if (obj.targetObject == target)
                return obj;
            
            // Проверяем дочерние объекты
            if (target.transform.IsChildOf(obj.targetObject.transform))
                return obj;
        }
        return null;
    }

    void AddHighlight(GameObject target, HighlightableObject settings)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer != null && settings.highlightMaterial != null)
        {
            // Создаем копию материала с нужными настройками
            Material highlightMat = new Material(settings.highlightMaterial);
            highlightMat.color = settings.highlightColor;
            
            // Устанавливаем ширину обводки
            SetOutlineWidth(highlightMat, settings.outlineWidth);

            // Получаем текущие материалы
            Material[] currentMaterials = renderer.materials;
            
            // Создаем новый массив материалов с добавленным highlight материалом
            Material[] newMaterials = new Material[currentMaterials.Length + 1];
            
            // Копируем старые материалы
            for (int i = 0; i < currentMaterials.Length; i++)
            {
                newMaterials[i] = currentMaterials[i];
            }
            
            // Добавляем highlight материал в конец
            newMaterials[currentMaterials.Length] = highlightMat;
            
            // Применяем новые материалы
            renderer.materials = newMaterials;
        }
    }

    void RemoveHighlight()
    {
        if (currentHighlightedObject != null)
        {
            Renderer renderer = currentHighlightedObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material[] currentMaterials = renderer.materials;
                
                // Если есть больше одного материала, значит есть highlight
                if (currentMaterials.Length > 1)
                {
                    // Создаем массив без последнего материала (highlight)
                    Material[] originalMaterials = new Material[currentMaterials.Length - 1];
                    for (int i = 0; i < originalMaterials.Length; i++)
                    {
                        originalMaterials[i] = currentMaterials[i];
                    }
                    
                    // Уничтожаем highlight материал
                    if (currentMaterials[currentMaterials.Length - 1] != null)
                    {
                        DestroyImmediate(currentMaterials[currentMaterials.Length - 1]);
                    }
                    
                    // Применяем оригинальные материалы
                    renderer.materials = originalMaterials;
                }
            }
            currentHighlightedObject = null;
        }
    }

    void SetOutlineWidth(Material material, float width)
    {
        // Пробуем разные возможные названия свойств для ширины обводки
        if (material.HasProperty("_OutlineWidth"))
        {
            material.SetFloat("_OutlineWidth", width);
        }
        else if (material.HasProperty("_Width"))
        {
            material.SetFloat("_Width", width);
        }
        else if (material.HasProperty("_Thickness"))
        {
            material.SetFloat("_Thickness", width);
        }
        else if (material.HasProperty("_OutlineThickness"))
        {
            material.SetFloat("_OutlineThickness", width);
        }
        else if (material.HasProperty("_Outline"))
        {
            material.SetFloat("_Outline", width);
        }
        else if (material.HasProperty("_Size"))
        {
            material.SetFloat("_Size", width);
        }
    }

    // Методы для добавления/удаления объектов в runtime
    public void AddHighlightableObject(GameObject obj, Material material, Color color, float outlineWidth = 0.05f)
    {
        HighlightableObject newObj = new HighlightableObject
        {
            targetObject = obj,
            highlightMaterial = material,
            highlightColor = color,
            outlineWidth = outlineWidth
        };
        highlightableObjects.Add(newObj);
    }

    public void RemoveHighlightableObject(GameObject obj)
    {
        // Если объект сейчас подсвечен, убираем подсветку
        if (currentHighlightedObject == obj)
        {
            RemoveHighlight();
        }
        highlightableObjects.RemoveAll(x => x.targetObject == obj);
    }

    void OnDisable()
    {
        RemoveHighlight();
    }

    void OnDestroy()
    {
        RemoveHighlight();
    }
}
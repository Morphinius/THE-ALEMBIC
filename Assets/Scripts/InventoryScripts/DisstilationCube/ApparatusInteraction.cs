using UnityEngine;

public class ApparatusInteraction : MonoBehaviour
{
    [Header("Apparatus Reference")]
    [SerializeField] private DistillationApparatus apparatus;
    
    [Header("Interaction Settings")]
    [SerializeField] private float maxInteractionDistance = 10f;
    [SerializeField] private KeyCode interactionKey = KeyCode.Mouse0;
    [SerializeField] private LayerMask interactionLayerMask = ~0;
    
    private Camera playerCamera;
    private bool isPlayerInTrigger = false;
    private bool isMouseOverApparatus = false;

    private void Start()
    {
        // Пытаемся найти аппарат если не назначен
        if (apparatus == null)
        {
            apparatus = GetComponent<DistillationApparatus>();
            Debug.Log($"ApparatusInteraction: Автопоиск аппарата - {apparatus != null}");
        }
        
        if (apparatus == null)
        {
            Debug.LogError("ApparatusInteraction: DistillationApparatus не найден!");
        }
        else
        {
            Debug.Log($"ApparatusInteraction: аппарат найден - {apparatus.GetApparatusName()}");
        }
        
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            Debug.LogError("ApparatusInteraction: Основная камера не найдена!");
        }
        else
        {
            Debug.Log("ApparatusInteraction: камера найдена");
        }
    }

    private void Update()
    {
        if (playerCamera == null) return;
        
        CheckMouseOverApparatus();
        HandleInteractionInput();
    }

    private void CheckMouseOverApparatus()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        bool wasMouseOver = isMouseOverApparatus;
        isMouseOverApparatus = false;
        
        if (Physics.Raycast(ray, out hit, maxInteractionDistance, interactionLayerMask))
        {
            // Проверяем по имени объекта для надежности
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                isMouseOverApparatus = true;
                if (!wasMouseOver)
                {
                    Debug.Log("ApparatusInteraction: курсор над аппаратом");
                }
            }
        }
    }

    private void HandleInteractionInput()
    {
        if (isPlayerInTrigger && isMouseOverApparatus && Input.GetKeyDown(interactionKey))
        {
            Debug.Log("ApparatusInteraction: Условия выполнены - взаимодействие с аппаратом");
            InteractWithApparatus();
        }
        else if (Input.GetKeyDown(interactionKey))
        {
            Debug.Log($"ApparatusInteraction: Условия НЕ выполнены. InTrigger: {isPlayerInTrigger}, MouseOver: {isMouseOverApparatus}");
        }
    }

    public void SetPlayerInTrigger(bool inTrigger)
    {
        Debug.Log($"ApparatusInteraction: SetPlayerInTrigger({inTrigger}) вызван!");
        
        bool wasInTrigger = isPlayerInTrigger;
        isPlayerInTrigger = inTrigger;
        
        if (inTrigger && !wasInTrigger)
        {
            Debug.Log("ApparatusInteraction: ✅ Игрок в триггере аппарата");
        }
        else if (!inTrigger && wasInTrigger)
        {
            Debug.Log("ApparatusInteraction: ❌ Игрок вышел из триггера аппарата");
        }
        
        if (!inTrigger && apparatus != null && apparatus.IsApparatusOpen())
        {
            Debug.Log("ApparatusInteraction: Закрываем аппарат так как игрок вышел");
            apparatus.CloseApparatus();
        }
    }

    private void InteractWithApparatus()
    {
        if (apparatus == null) 
        {
            Debug.LogError("ApparatusInteraction: Apparatus is null!");
            return;
        }
        
        var inventorySystem = FindFirstObjectByType<InventorySystem>();
        if (inventorySystem == null) 
        {
            Debug.LogError("ApparatusInteraction: InventorySystem не найден!");
            return;
        }
        
        Debug.Log("ApparatusInteraction: Взаимодействие с аппаратом...");
        
        // Открываем инвентарь если закрыт
        if (!inventorySystem.IsInventoryOpen())
        {
            inventorySystem.OpenInventory();
            Debug.Log("ApparatusInteraction: Инвентарь открыт");
        }
        
        // Открываем/закрываем аппарат
        if (apparatus.IsApparatusOpen())
        {
            apparatus.CloseApparatus();
            Debug.Log("ApparatusInteraction: Аппарат закрыт");
        }
        else
        {
            apparatus.OpenApparatus();
            Debug.Log("ApparatusInteraction: Аппарат открыт");
        }
    }

    // Для отладки в редакторе
    private void OnMouseEnter()
    {
        Debug.Log("ApparatusInteraction: OnMouseEnter");
    }

    private void OnMouseExit()
    {
        Debug.Log("ApparatusInteraction: OnMouseExit");
    }

    private void OnMouseDown()
    {
        Debug.Log("ApparatusInteraction: OnMouseDown");
        // Принудительная проверка
        Debug.Log($"OnMouseDown - InTrigger: {isPlayerInTrigger}, MouseOver: {isMouseOverApparatus}");
    }
}
using UnityEngine;

public class ChestInteraction : MonoBehaviour
{
    [Header("Chest Systems")]
    [SerializeField] private ChestSystem chestSystem;
    [SerializeField] private InventorySystem inventorySystem;
    
    [Header("Interaction Settings")]
    [SerializeField] private float maxInteractionDistance = 10f;
    [SerializeField] private KeyCode interactionKey = KeyCode.Mouse0;
    [SerializeField] private LayerMask interactionLayerMask = ~0;
    
    [Header("Visual Feedback")]
    [SerializeField] private bool showDebugRay = true;
    [SerializeField] private Color debugRayColor = Color.green;
    
    private Camera playerCamera;
    private bool isPlayerInTrigger = false;
    private bool isMouseOverChest = false;

    private void Start()
    {
        if (chestSystem == null)
            chestSystem = FindObjectOfType<ChestSystem>();
        
        if (inventorySystem == null)
            inventorySystem = FindObjectOfType<InventorySystem>();
        
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            Debug.LogError("Основная камера не найдена!");
        }
    }

    private void Update()
    {
        if (playerCamera == null) return;
        
        CheckMouseOverChest();
        HandleInteractionInput();
    }

    private void CheckMouseOverChest()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        bool wasMouseOverChest = isMouseOverChest;
        isMouseOverChest = false;
        
        if (Physics.Raycast(ray, out hit, maxInteractionDistance, interactionLayerMask))
        {
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                isMouseOverChest = true;
            }
        }
    }

    private void HandleInteractionInput()
    {
        if (isPlayerInTrigger && isMouseOverChest && Input.GetKeyDown(interactionKey))
        {
            OpenChestWithInventory();
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseChestAndInventory();
        }
    }

    public void SetPlayerInTrigger(bool inTrigger)
    {
        isPlayerInTrigger = inTrigger;
        
        if (!inTrigger)
        {
            if (chestSystem != null && chestSystem.IsChestOpen())
            {
                chestSystem.CloseChest();
            }
        }
    }

    private void OpenChestWithInventory()
    {
        if (chestSystem == null || inventorySystem == null) return;
        
        // Открываем инвентарь если закрыт
        if (!inventorySystem.IsInventoryOpen())
        {
            inventorySystem.OpenInventory();
        }
        
        // Открываем сундук
        chestSystem.OpenChest();
        Debug.Log("Сундук открыт");
    }

    public void CloseChestAndInventory()
    {
        if (chestSystem != null && chestSystem.IsChestOpen())
        {
            chestSystem.CloseChest();
        }
    }

    // Визуализация в редакторе
    private void OnDrawGizmos()
    {
        if (!showDebugRay || playerCamera == null || !isPlayerInTrigger) return;
        
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, maxInteractionDistance, interactionLayerMask))
        {
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(ray.origin, hit.point);
                Gizmos.DrawWireSphere(hit.point, 0.1f);
            }
        }
    }

    public bool IsPlayerInTrigger()
    {
        return isPlayerInTrigger;
    }
    
    public bool IsMouseOverChest()
    {
        return isMouseOverChest;
    }
}
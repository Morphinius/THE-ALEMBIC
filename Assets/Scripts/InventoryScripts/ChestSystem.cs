using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ChestSystem : MonoBehaviour
{
    [Header("Chest Settings")]
    [SerializeField] private int chestSize = 12;
    
    [Header("UI References")]
    [SerializeField] private GameObject chestPanel;
    [SerializeField] private Transform slotGrid;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Canvas canvas;
    
    [Header("Drag & Drop")]
    [SerializeField] private Image dragIcon;
    [SerializeField] private CanvasGroup dragCanvasGroup;
    
    private InventoryItem[] items;
    private ChestSlot[] slots;
    
    private bool isDragging = false;
    private int dragStartIndex = -1;
    private InventoryItem dragItem;

    private InventorySystem inventory;

    private void Start()
    {
        inventory = FindObjectOfType<InventorySystem>();
        InitializeChest();
        
        if (dragIcon != null)
        {
            dragIcon.gameObject.SetActive(true);
            dragIcon.rectTransform.sizeDelta = new Vector2(80, 80);
            dragIcon.color = new Color(1, 1, 1, 0);
        }
        
        chestPanel.SetActive(false);
    }

    private void InitializeChest()
    {
        items = new InventoryItem[chestSize];
        slots = new ChestSlot[chestSize];
        
        foreach (Transform child in slotGrid)
        {
            Destroy(child.gameObject);
        }
        
        for (int i = 0; i < chestSize; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotGrid);
            ChestSlot slot = slotObj.GetComponent<ChestSlot>();
            slot.Initialize(this, i);
            slots[i] = slot;
            items[i] = null;
        }
    }

    private void Update()
    {
        if (isDragging)
        {
            UpdateDragPosition();
        }
    }

    public void OpenChest()
    {
        chestPanel.SetActive(true);
    }

    public void CloseChest()
    {
        chestPanel.SetActive(false);
    }

    // Drag & Drop методы
    public void StartDragItem(int slotIndex)
    {
        if (items[slotIndex] == null || isDragging) return;
        
        isDragging = true;
        dragStartIndex = slotIndex;
        dragItem = items[slotIndex];
        
        // НЕ удаляем предмет из слота при начале перетаскивания
        // Он останется на месте до успешного завершения перетаскивания
        
        dragIcon.sprite = dragItem.data.icon;
        dragIcon.color = new Color(1, 1, 1, 0.75f);
        dragIcon.rectTransform.sizeDelta = new Vector2(80, 80);
        dragCanvasGroup.alpha = 0.75f;
        dragCanvasGroup.blocksRaycasts = false;
        
        UpdateDragPosition();
        
        Debug.Log($"Начато перетаскивание из слота {slotIndex}: {dragItem.data.itemName}");
    }

    private void UpdateDragPosition()
    {
        if (!isDragging || dragIcon == null) return;
        
        dragIcon.transform.position = Input.mousePosition;
    }

    public void DropOnSlot(int targetIndex)
    {
        if (!isDragging || targetIndex < 0 || targetIndex >= items.Length) return;
        
        Debug.Log($"Перетаскивание в сундуке из {dragStartIndex} в {targetIndex}");

        // Если перетаскиваем на тот же слот - отмена
        if (targetIndex == dragStartIndex)
        {
            Debug.Log("Перетаскивание на тот же слот - отмена");
            EndDrag();
            return;
        }

        // Сохраняем копию dragItem для безопасной работы
        InventoryItem draggedItem = dragItem;
        
        // Стакивание предметов
        if (items[targetIndex] != null && items[targetIndex].CanStackWith(draggedItem))
        {
            int spaceLeft = items[targetIndex].GetSpaceLeft();
            int transferAmount = Mathf.Min(draggedItem.stackSize, spaceLeft);
            
            items[targetIndex].stackSize += transferAmount;
            
            // Обновляем исходный предмет
            if (draggedItem.stackSize - transferAmount <= 0)
            {
                items[dragStartIndex] = null;
            }
            else
            {
                draggedItem.stackSize -= transferAmount;
                items[dragStartIndex] = draggedItem;
            }
            
            Debug.Log($"Предметы стакнулись. Перенесено: {transferAmount}");
        }
        // Обмен предметами
        else
        {
            InventoryItem targetItem = items[targetIndex];
            items[targetIndex] = draggedItem;
            items[dragStartIndex] = targetItem;
            
            Debug.Log($"Обмен предметами между слотами {dragStartIndex} и {targetIndex}");
        }
        
        // Обновляем оба слота
        UpdateSlot(dragStartIndex);
        UpdateSlot(targetIndex);
        
        // Сбрасываем перетаскивание
        ResetDrag();
    }

    public void EndDrag()
    {
        if (!isDragging) return;
        
        Debug.Log("Завершение перетаскивания (отпустили вне слота)");

        // Проверяем, находится ли курсор над инвентарем
        if (!IsPointerOverChest() && inventory != null && inventory.IsInventoryOpen())
        {
            // Пытаемся переместить предмет в инвентарь
            if (inventory.AddItem(dragItem.data, dragItem.stackSize))
            {
                // Если удалось добавить в инвентарь, удаляем из сундука
                items[dragStartIndex] = null;
                UpdateSlot(dragStartIndex);
                Debug.Log($"Предмет перемещен в инвентарь: {dragItem.data.itemName}");
            }
            else
            {
                Debug.Log($"Не удалось переместить предмет в инвентарь: {dragItem.data.itemName}");
            }
        }
        // Если не над инвентарем и не над сундуком, предмет остается на месте
        // (мы не удаляли его из исходного слота)
        
        ResetDrag();
    }

    private bool IsPointerOverChest()
    {
        var pointerEventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, results);
        
        foreach (var result in results)
        {
            if (result.gameObject.transform.IsChildOf(chestPanel.transform))
                return true;
        }
        
        return false;
    }

    private void ResetDrag()
    {
        isDragging = false;
        dragStartIndex = -1;
        dragItem = null;
        
        if (dragIcon != null)
        {
            dragIcon.sprite = null;
            dragIcon.color = new Color(1, 1, 1, 0);
        }
        
        if (dragCanvasGroup != null)
        {
            dragCanvasGroup.blocksRaycasts = true;
        }
    }

    private void UpdateSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < slots.Length)
        {
            slots[slotIndex].UpdateSlot(items[slotIndex]);
            Debug.Log($"Обновлен слот {slotIndex}: {(items[slotIndex] != null ? items[slotIndex].data.itemName : "пусто")}");
        }
    }

    public bool AddItem(ItemData itemData, int amount = 1)
    {
        int remainingAmount = amount;
        
        for (int i = 0; i < items.Length && remainingAmount > 0; i++)
        {
            if (items[i] != null && items[i].data != null && items[i].data.id == itemData.id)
            {
                int spaceLeft = items[i].GetSpaceLeft();
                if (spaceLeft > 0)
                {
                    int addAmount = Mathf.Min(remainingAmount, spaceLeft);
                    items[i].stackSize += addAmount;
                    remainingAmount -= addAmount;
                    UpdateSlot(i);
                }
            }
        }
        
        for (int i = 0; i < items.Length && remainingAmount > 0; i++)
        {
            if (items[i] == null)
            {
                int addAmount = Mathf.Min(remainingAmount, itemData.maxStack);
                items[i] = new InventoryItem(itemData, addAmount);
                remainingAmount -= addAmount;
                UpdateSlot(i);
            }
        }
        
        return remainingAmount == 0;
    }

    public void RemoveItem(int slotIndex, int amount = 1)
    {
        if (slotIndex < 0 || slotIndex >= items.Length) return;
        if (items[slotIndex] == null) return;
        
        items[slotIndex].stackSize -= amount;
        
        if (items[slotIndex].stackSize <= 0)
        {
            items[slotIndex] = null;
        }
        
        UpdateSlot(slotIndex);
    }

    public bool MoveFromInventory(InventoryItem item)
    {
        if (item == null) return false;
        return AddItem(item.data, item.stackSize);
    }

    public void MoveToInventory(int slotIndex)
    {
        if (items[slotIndex] == null || inventory == null) return;
        
        if (inventory.AddItem(items[slotIndex].data, items[slotIndex].stackSize))
        {
            items[slotIndex] = null;
            UpdateSlot(slotIndex);
        }
    }

    public bool IsChestOpen()
    {
        return chestPanel.activeInHierarchy;
    }
}
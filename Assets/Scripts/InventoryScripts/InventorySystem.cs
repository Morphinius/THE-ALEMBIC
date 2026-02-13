using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySystem : MonoBehaviour
{
    [Header("Inventory Settings")]
    [SerializeField] private int inventorySize = 20;
    [SerializeField] private List<ItemData> availableItems = new List<ItemData>();
    
    [Header("UI References")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform slotGrid;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Canvas canvas;
    
    [Header("Drag & Drop")]
    [SerializeField] private Image dragIcon;
    [SerializeField] private CanvasGroup dragCanvasGroup;

    [Header("Chest System")]
    [SerializeField] private ChestSystem chest;
    
    private InventoryItem[] items;
    private InventorySlot[] slots;
    
    private bool isDragging = false;
    private int dragStartIndex = -1;
    private InventoryItem dragItem;

    private void Start()
    {
        InitializeInventory();
        
        if (dragIcon != null)
        {
            dragIcon.gameObject.SetActive(true);
            dragIcon.rectTransform.sizeDelta = new Vector2(80, 80);
            dragIcon.color = new Color(1, 1, 1, 0);
        }
        
        inventoryPanel.SetActive(false);
    }

    private void InitializeInventory()
    {
        items = new InventoryItem[inventorySize];
        slots = new InventorySlot[inventorySize];
        
        for (int i = 0; i < inventorySize; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotGrid);
            InventorySlot slot = slotObj.GetComponent<InventorySlot>();
            slot.Initialize(this, i);
            slots[i] = slot;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            AddRandomItem();
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            RemoveRandomItem();
        }
        
        if (isDragging)
        {
            UpdateDragPosition();
        }
    }

    public void ToggleInventory()
    {
        bool newState = !inventoryPanel.activeInHierarchy;
        inventoryPanel.SetActive(newState);
        
        // ИСПРАВЛЕНИЕ: При нажатии I закрываем сундук, но не открываем его
        if (chest != null && newState == false)
        {
            // Если закрываем инвентарь - закрываем и сундук
            chest.CloseChest();
        }
        // Не открываем сундук автоматически при открытии инвентаря на I
    }

    // Открыть инвентарь (без открытия сундука)
    public void OpenInventory()
    {
        inventoryPanel.SetActive(true);
        // Не открываем сундук автоматически
    }

    // Закрыть инвентарь (и сундук если открыт)
    public void CloseInventory()
    {
        inventoryPanel.SetActive(false);
        if (chest != null)
        {
            chest.CloseChest();
        }
    }

    // Проверить открыт ли инвентарь
    public bool IsInventoryOpen()
    {
        return inventoryPanel.activeInHierarchy;
    }

    private void AddRandomItem()
    {
        if (availableItems.Count == 0) return;
        
        ItemData randomItem = availableItems[Random.Range(0, availableItems.Count)];
        int randomAmount = Random.Range(1, 11);
        
        AddItem(randomItem, randomAmount);
    }

    private void RemoveRandomItem()
    {
        List<int> nonEmptySlots = new List<int>();
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null)
            {
                nonEmptySlots.Add(i);
            }
        }
        
        if (nonEmptySlots.Count > 0)
        {
            int randomSlot = nonEmptySlots[Random.Range(0, nonEmptySlots.Count)];
            int removeAmount = Random.Range(1, 11);
            
            RemoveItem(randomSlot, removeAmount);
        }
    }

    public bool AddItem(ItemData itemData, int amount = 1)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] != null && items[i].data.id == itemData.id)
            {
                int spaceLeft = items[i].GetSpaceLeft();
                if (spaceLeft > 0)
                {
                    int addAmount = Mathf.Min(amount, spaceLeft);
                    items[i].stackSize += addAmount;
                    amount -= addAmount;
                    UpdateSlot(i);
                    
                    if (amount <= 0) return true;
                }
            }
        }
        
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                int addAmount = Mathf.Min(amount, itemData.maxStack);
                items[i] = new InventoryItem(itemData, addAmount);
                amount -= addAmount;
                UpdateSlot(i);
                
                if (amount <= 0) return true;
            }
        }
        
        return amount <= 0;
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

    private void UpdateSlot(int slotIndex)
    {
        slots[slotIndex].UpdateSlot(items[slotIndex]);
    }

    // Drag & Drop методы
    public void StartDragItem(int slotIndex)
    {
        if (items[slotIndex] == null) return;
        
        isDragging = true;
        dragStartIndex = slotIndex;
        dragItem = items[slotIndex];
        
        dragIcon.sprite = dragItem.data.icon;
        dragIcon.color = new Color(1, 1, 1, 0.75f);
        dragIcon.rectTransform.sizeDelta = new Vector2(80, 80);
        dragCanvasGroup.alpha = 0.75f;
        dragCanvasGroup.blocksRaycasts = false;
        
        UpdateDragPosition();
    }

    private void UpdateDragPosition()
    {
        if (!isDragging || dragIcon == null) return;
        
        dragIcon.transform.position = Input.mousePosition;
    }

    public void DropOnSlot(int targetIndex)
    {
        if (!isDragging || targetIndex < 0 || targetIndex >= items.Length) return;
        
        Debug.Log($"DropOnSlot: {dragStartIndex} -> {targetIndex}");

        if (targetIndex == dragStartIndex)
        {
            ResetDrag();
            return;
        }

        InventoryItem sourceItem = dragItem;
        InventoryItem targetItem = items[targetIndex];

        if (targetItem != null && targetItem.CanStackWith(sourceItem))
        {
            int spaceLeft = targetItem.GetSpaceLeft();
            int transferAmount = Mathf.Min(sourceItem.stackSize, spaceLeft);
            
            targetItem.stackSize += transferAmount;
            sourceItem.stackSize -= transferAmount;
            
            if (sourceItem.stackSize <= 0)
            {
                items[dragStartIndex] = null;
            }
        }
        else
        {
            items[targetIndex] = sourceItem;
            items[dragStartIndex] = targetItem;
        }

        UpdateSlot(dragStartIndex);
        UpdateSlot(targetIndex);
        
        ResetDrag();
    }

    public void EndDrag()
    {
        if (!isDragging) return;

        if (!IsPointerOverInventory() && chest != null && chest.IsChestOpen())
        {
            if (chest.MoveFromInventory(dragItem))
            {
                items[dragStartIndex] = null;
                UpdateSlot(dragStartIndex);
                Debug.Log($"Предмет перемещен в сундук: {dragItem.data.itemName}");
            }
        }

        ResetDrag();
    }

    private bool IsPointerOverInventory()
    {
        var pointerEventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, results);
        
        foreach (var result in results)
        {
            if (result.gameObject.GetComponent<InventorySlot>() != null)
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

    public void UseItem(int slotIndex)
    {
        if (items[slotIndex] == null) return;
        
        items[slotIndex].stackSize--;
        if (items[slotIndex].stackSize <= 0)
        {
            items[slotIndex] = null;
        }
        
        UpdateSlot(slotIndex);
    }
}
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI textAmount;
    
    private InventorySystem inventory;
    private int slotIndex;
    private InventoryItem item;

    public void Initialize(InventorySystem inventorySystem, int index)
    {
        inventory = inventorySystem;
        slotIndex = index;
        ClearSlot();
    }

    public void UpdateSlot(InventoryItem newItem)
    {
        item = newItem;
        
        if (item != null && item.data != null)
        {
            icon.sprite = item.data.icon;
            icon.color = Color.white;
            textAmount.text = item.stackSize > 1 ? item.stackSize.ToString() : "";
        }
        else
        {
            ClearSlot();
        }
    }

    private void ClearSlot()
    {
        item = null;
        icon.sprite = null;
        icon.color = Color.clear;
        textAmount.text = "";
    }

    // Обработчики Drag & Drop
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item == null) return;
        
        inventory.StartDragItem(slotIndex);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Drag обрабатывается в InventorySystem
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        inventory.EndDrag();
    }

    public void OnDrop(PointerEventData eventData)
    {
        inventory.DropOnSlot(slotIndex);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Двойной клик для использования предмета
        if (eventData.button == PointerEventData.InputButton.Left && eventData.clickCount == 2)
        {
            inventory.UseItem(slotIndex);
        }
    }
}
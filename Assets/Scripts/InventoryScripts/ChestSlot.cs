using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ChestSlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI textAmount;
    
    private ChestSystem chest;
    private int slotIndex;
    private InventoryItem item;

    public void Initialize(ChestSystem chestSystem, int index)
    {
        chest = chestSystem;
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
        chest.StartDragItem(slotIndex);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Drag обрабатывается в ChestSystem
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        chest.EndDrag();
    }

    public void OnDrop(PointerEventData eventData)
    {
        chest.DropOnSlot(slotIndex);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Двойной клик для быстрого перемещения
        if (eventData.button == PointerEventData.InputButton.Left && eventData.clickCount == 2)
        {
            // Можно добавить логику быстрого перемещения в инвентарь
        }
    }
}
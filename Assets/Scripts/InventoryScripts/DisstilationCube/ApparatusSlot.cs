using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ApparatusSlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI textAmount;
    [SerializeField] private Image slotBackground;
    
    private DistillationApparatus apparatus;
    private int slotIndex;
    private bool isInputSlot;
    private InventoryItem item;

    public void Initialize(DistillationApparatus apparatusSystem, int index, bool inputSlot)
    {
        apparatus = apparatusSystem;
        slotIndex = index;
        isInputSlot = inputSlot;
        ClearSlot();

        // Визуальное отличие входных и выходных слотов
        if (slotBackground != null)
        {
            slotBackground.color = isInputSlot ? 
                new Color(0.8f, 0.8f, 1f, 0.5f) : // Голубоватый для входных
                new Color(1f, 0.8f, 0.8f, 0.5f);  // Розоватый для выходных
        }
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
        apparatus.StartDragItem(slotIndex, isInputSlot);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Drag обрабатывается в DistillationApparatus
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        apparatus.EndDrag();
    }

    public void OnDrop(PointerEventData eventData)
    {
        apparatus.DropOnSlot(slotIndex, isInputSlot);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Двойной клик на выходном слоте - забрать эссенцию
        if (!isInputSlot && eventData.button == PointerEventData.InputButton.Left && eventData.clickCount == 2)
        {
            apparatus.TakeEssence();
        }
    }
}
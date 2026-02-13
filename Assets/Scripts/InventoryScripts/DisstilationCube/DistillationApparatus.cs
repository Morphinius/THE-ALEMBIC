using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DistillationApparatus : MonoBehaviour
{
    [Header("Apparatus Settings")]
    [SerializeField] private string apparatusName = "Перегонный аппарат";
    
    [Header("UI References")]
    [SerializeField] private GameObject apparatusPanel;
    [SerializeField] private Transform inputSlotsGrid;
    [SerializeField] private Transform outputSlot;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Button distillButton;
    
    [Header("Drag & Drop")]
    [SerializeField] private Image dragIcon;
    [SerializeField] private CanvasGroup dragCanvasGroup;
    [SerializeField] private Canvas canvas;
    
    [Header("Distillation Settings")]
    [SerializeField] private int inputSlotsCount = 5;
    [SerializeField] private ItemData essenceItem; // Результирующая эссенция
    
    private InventoryItem[] inputItems;
    private InventoryItem outputItem;
    private ApparatusSlot[] inputSlots;
    private ApparatusSlot outputSlotComponent;
    
    private bool isDragging = false;
    private int dragStartIndex = -1;
    private InventoryItem dragItem;
    private bool isInputSlotDrag = true;

    private InventorySystem inventory;

    private void Start()
    {
        inventory = FindFirstObjectByType<InventorySystem>();
        InitializeApparatus();

        if (dragIcon != null)
        {
            dragIcon.gameObject.SetActive(true);
            dragIcon.rectTransform.sizeDelta = new Vector2(80, 80);
            dragIcon.color = new Color(1, 1, 1, 0);
        }

        if (distillButton != null)
        {
            distillButton.onClick.AddListener(StartDistillation);
            // Скрываем кнопку при старте
            distillButton.gameObject.SetActive(false);
        }

        CloseApparatus();

        Debug.Log($"Аппарат {apparatusName} инициализирован. Panel: {apparatusPanel != null}");
    }
    
    private void InitializeApparatus()
    {
        // Инициализируем входные слоты
        inputItems = new InventoryItem[inputSlotsCount];
        inputSlots = new ApparatusSlot[inputSlotsCount];
        
        if (inputSlotsGrid != null && slotPrefab != null)
        {
            Debug.Log("Создаю входные слоты...");
            
            // Очищаем старые слоты если есть
            foreach (Transform child in inputSlotsGrid)
            {
                Destroy(child.gameObject);
            }
            
            // Создаем входные слоты
            for (int i = 0; i < inputSlotsCount; i++)
            {
                GameObject slotObj = Instantiate(slotPrefab, inputSlotsGrid);
                ApparatusSlot slot = slotObj.GetComponent<ApparatusSlot>();
                
                if (slot != null)
                {
                    slot.Initialize(this, i, true);
                    inputSlots[i] = slot;
                    inputItems[i] = null;
                    slotObj.name = $"InputSlot_{i}";
                }
                else
                {
                    Debug.LogError("ApparatusSlot компонент не найден на префабе слота!");
                }
            }
        }
        else
        {
            Debug.LogError($"InputSlotsGrid или SlotPrefab не назначены! Grid: {inputSlotsGrid != null}, Prefab: {slotPrefab != null}");
        }
        
        // Инициализируем выходной слот
        if (outputSlot != null && slotPrefab != null)
        {
            Debug.Log("Создаю выходной слот...");
            
            // Очищаем старый слот
            foreach (Transform child in outputSlot)
            {
                Destroy(child.gameObject);
            }
            
            // Создаем выходной слот
            GameObject outputSlotObj = Instantiate(slotPrefab, outputSlot);
            outputSlotComponent = outputSlotObj.GetComponent<ApparatusSlot>();
            
            if (outputSlotComponent != null)
            {
                outputSlotComponent.Initialize(this, -1, false);
                outputItem = null;
                outputSlotObj.name = "OutputSlot";
            }
            else
            {
                Debug.LogError("ApparatusSlot компонент не найден на префабе для выходного слота!");
            }
        }
        else
        {
            Debug.LogError($"OutputSlot или SlotPrefab не назначены! OutputSlot: {outputSlot != null}, Prefab: {slotPrefab != null}");
        }
        
        Debug.Log($"Перегонный аппарат инициализирован: {inputSlotsCount} входных слотов");
    }

    private void Update()
    {
        if (isDragging)
        {
            UpdateDragPosition();
        }
    }

    public void OpenApparatus()
    {
        if (apparatusPanel != null)
        {
            apparatusPanel.SetActive(true);
            if (distillButton != null)
            {
                distillButton.gameObject.SetActive(true);
            }
            Debug.Log($"Открыт {apparatusName}");
        }
        else
        {
            Debug.LogError($"ApparatusPanel не назначен для аппарата {apparatusName}!");
        }
    }

    public void CloseApparatus()
    {
        if (apparatusPanel != null)
        {
            apparatusPanel.SetActive(false);
        }
        
        if (distillButton != null)
        {
            distillButton.gameObject.SetActive(false);
        }
        
        if (isDragging)
        {
            ResetDrag();
        }
        
        Debug.Log($"Закрыт {apparatusName}");
    }

    // Drag & Drop методы
    public void StartDragItem(int slotIndex, bool isInputSlot)
    {
        InventoryItem item = isInputSlot ? 
            (slotIndex >= 0 && slotIndex < inputItems.Length ? inputItems[slotIndex] : null) : 
            outputItem;
            
        if (item == null) return;
        
        isDragging = true;
        dragStartIndex = slotIndex;
        isInputSlotDrag = isInputSlot;
        dragItem = item;
        
        if (dragIcon != null)
        {
            dragIcon.sprite = dragItem.data.icon;
            dragIcon.color = new Color(1, 1, 1, 0.75f);
            dragIcon.rectTransform.sizeDelta = new Vector2(80, 80);
        }
        
        if (dragCanvasGroup != null)
        {
            dragCanvasGroup.alpha = 0.75f;
            dragCanvasGroup.blocksRaycasts = false;
        }
        
        UpdateDragPosition();
    }

    private void UpdateDragPosition()
    {
        if (!isDragging || dragIcon == null) return;
        dragIcon.transform.position = Input.mousePosition;
    }

    public void DropOnSlot(int targetIndex, bool isInputSlot)
    {
        if (!isDragging) return;

        // Если перетаскиваем из входного слота в выходной или наоборот
        if (isInputSlotDrag != isInputSlot)
        {
            // Перемещение между разными типами слотов не разрешено
            ResetDrag();
            return;
        }

        // Если перетаскиваем на тот же слот
        if (isInputSlotDrag && targetIndex == dragStartIndex)
        {
            ResetDrag();
            return;
        }

        if (isInputSlotDrag)
        {
            // Перетаскивание между входными слотами
            HandleInputToInputDrag(targetIndex);
        }
        else
        {
            // Перетаскивание выходного предмета (можно только забрать в инвентарь)
            ResetDrag();
            return;
        }
        
        ResetDrag();
    }

    private void HandleInputToInputDrag(int targetIndex)
    {
        if (targetIndex < 0 || targetIndex >= inputItems.Length) return;

        InventoryItem sourceItem = dragItem;
        InventoryItem targetItem = inputItems[targetIndex];

        // Если целевой слот пустой - перемещаем предмет
        if (targetItem == null)
        {
            inputItems[targetIndex] = sourceItem;
            inputItems[dragStartIndex] = null;
        }
        // Если в целевом слоте тот же предмет - стакаем
        else if (targetItem.CanStackWith(sourceItem))
        {
            int spaceLeft = targetItem.GetSpaceLeft();
            int transferAmount = Mathf.Min(sourceItem.stackSize, spaceLeft);
            
            targetItem.stackSize += transferAmount;
            sourceItem.stackSize -= transferAmount;
            
            if (sourceItem.stackSize <= 0)
            {
                inputItems[dragStartIndex] = null;
            }
        }
        // Если разные предметы - обмен
        else
        {
            inputItems[targetIndex] = sourceItem;
            inputItems[dragStartIndex] = targetItem;
        }

        // Обновляем оба слота
        UpdateInputSlot(dragStartIndex);
        UpdateInputSlot(targetIndex);
    }

    public void EndDrag()
    {
        if (!isDragging) return;

        // Если предмет перетащили за пределы аппарата - пытаемся переместить в инвентарь
        if (!IsPointerOverApparatus() && inventory != null && inventory.IsInventoryOpen())
        {
            if (inventory.AddItem(dragItem.data, dragItem.stackSize))
            {
                // Удаляем предмет из аппарата
                if (isInputSlotDrag)
                {
                    inputItems[dragStartIndex] = null;
                    UpdateInputSlot(dragStartIndex);
                }
                else
                {
                    outputItem = null;
                    UpdateOutputSlot();
                }
            }
        }

        ResetDrag();
    }

    private bool IsPointerOverApparatus()
    {
        return apparatusPanel != null && apparatusPanel.activeInHierarchy;
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

    private void UpdateInputSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < inputSlots.Length && inputSlots[slotIndex] != null)
        {
            inputSlots[slotIndex].UpdateSlot(inputItems[slotIndex]);
        }
    }

    private void UpdateOutputSlot()
    {
        if (outputSlotComponent != null)
        {
            outputSlotComponent.UpdateSlot(outputItem);
        }
    }

    // НОВЫЙ МЕТОД: Добавление предмета из инвентаря
    public bool AddItemFromInventory(ItemData itemData, int amount)
    {
        if (itemData == null) return false;

        // Сначала пытаемся стакать с существующими предметами
        for (int i = 0; i < inputItems.Length; i++)
        {
            if (inputItems[i] != null && inputItems[i].data == itemData)
            {
                int spaceLeft = inputItems[i].GetSpaceLeft();
                if (spaceLeft > 0)
                {
                    int transferAmount = Mathf.Min(amount, spaceLeft);
                    inputItems[i].stackSize += transferAmount;
                    UpdateInputSlot(i);
                    
                    Debug.Log($"Добавлено {transferAmount} {itemData.itemName} в слот {i} (стакание)");
                    return transferAmount == amount;
                }
            }
        }

        // Ищем пустой слот
        for (int i = 0; i < inputItems.Length; i++)
        {
            if (inputItems[i] == null)
            {
                inputItems[i] = new InventoryItem(itemData, amount);
                UpdateInputSlot(i);
                
                Debug.Log($"Добавлено {amount} {itemData.itemName} в слот {i}");
                return true;
            }
        }

        Debug.Log("Нет свободных слотов в аппарате!");
        return false;
    }

    // НОВЫЙ МЕТОД: Получить предмет из слота
    public InventoryItem GetItemFromSlot(int slotIndex, bool isInputSlot)
    {
        if (isInputSlot)
        {
            if (slotIndex >= 0 && slotIndex < inputItems.Length)
                return inputItems[slotIndex];
        }
        else
        {
            return outputItem;
        }
        return null;
    }

    // Основной метод перегонки
    public void StartDistillation()
    {
        if (essenceItem == null)
        {
            Debug.LogError("EssenceItem не назначен для перегонного аппарата!");
            return;
        }

        // Проверяем, есть ли предметы для перегонки
        bool hasIngredients = false;
        foreach (var item in inputItems)
        {
            if (item != null)
            {
                hasIngredients = true;
                break;
            }
        }

        if (!hasIngredients)
        {
            Debug.Log("Нет ингредиентов для перегонки!");
            return;
        }

        // Проверяем, свободен ли выходной слот
        if (outputItem != null)
        {
            Debug.Log("Выходной слот занят! Заберите эссенцию.");
            return;
        }

        // Вычисляем количество эссенции (простая логика - сумма всех ингредиентов / 2)
        int totalEssence = 0;
        foreach (var ingredient in inputItems)
        {
            if (ingredient != null)
            {
                totalEssence += ingredient.stackSize;
            }
        }

        totalEssence = Mathf.Max(1, totalEssence / 2); // Минимум 1 эссенция

        // Создаем эссенцию
        outputItem = new InventoryItem(essenceItem, totalEssence);
        
        // Очищаем входные слоты
        for (int i = 0; i < inputItems.Length; i++)
        {
            inputItems[i] = null;
            UpdateInputSlot(i);
        }

        // Обновляем выходной слот
        UpdateOutputSlot();

        Debug.Log($"Перегонка завершена! Получено {totalEssence} эссенции");
    }

    // Забрать эссенцию из выходного слота
    public bool TakeEssence()
    {
        if (outputItem == null) return false;
        
        if (inventory != null && inventory.AddItem(outputItem.data, outputItem.stackSize))
        {
            outputItem = null;
            UpdateOutputSlot();
            return true;
        }
        
        return false;
    }

    public bool IsApparatusOpen()
    {
        return apparatusPanel != null && apparatusPanel.activeInHierarchy;
    }

    public string GetApparatusName()
    {
        return apparatusName;
    }
}
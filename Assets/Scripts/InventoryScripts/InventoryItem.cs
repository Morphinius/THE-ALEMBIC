using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public ItemData data;
    public int stackSize;

    public InventoryItem(ItemData itemData, int amount = 1)
    {
        data = itemData;
        stackSize = Mathf.Min(amount, itemData.maxStack);
    }

    public bool CanStackWith(InventoryItem other)
    {
        return other != null && data != null && other.data != null && 
               data.id == other.data.id && stackSize < data.maxStack;
    }

    public int GetSpaceLeft()
    {
        return data.maxStack - stackSize;
    }
}
// Сохраните в: Assets/Scripts/Inventory/InventoryItem.cs
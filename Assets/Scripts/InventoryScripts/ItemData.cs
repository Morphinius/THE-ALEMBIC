using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    public int id;
    public string itemName;
    public Sprite icon;
    public int maxStack = 1;
    public ItemType type;
}

[System.Serializable]
public enum ItemType
{
    Weapon,
    Armor,
    Potion,
    Material,
    Quest
}
using UnityEngine;

[System.Serializable]
public enum ItemType
{
    Melee,
    Pistol
}

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public ItemType itemType;
    public int indexWeapons;
    public string itemName;
    
    [TextArea(3, 10)]
    public string descriptionItem;

    [TextArea(5, 10)]
    public string statsItem;
    public Sprite icon;
}
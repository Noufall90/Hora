using UnityEngine;

[System.Serializable]
public enum PotionType
{
    Healer,
    Shield
}

[CreateAssetMenu(fileName = "New Item Potion", menuName = "Inventory/Item Potion")]
public class PotionItem : ScriptableObject
{
    public PotionType type;
    public string itemName;
    public int effectAmount = 50;

    [TextArea(3, 10)]
    public string descriptionItem;

    [TextArea(5, 10)]
    public string statsItem;
    public Sprite icon;
}
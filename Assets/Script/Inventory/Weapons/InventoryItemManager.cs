using UnityEngine;
using UnityEngine.UI;

public class InventoryItemManager : MonoBehaviour
{
    public Item item;
    public Image equipIcon;

    private Button itemButton;

    private void Awake()
    {
        itemButton = GetComponent<Button>();
        if (itemButton != null)
        {
            itemButton.onClick.AddListener(OnItemClicked);
        }
    }

    public void Setup(Item newItem)
    {
        item = newItem;

        Transform iconTr = transform.Find("ItemIcon");
        if (iconTr != null)
        {
            Image itemIcon = iconTr.GetComponent<Image>();
            if (itemIcon != null && item != null && item.icon != null)
            {
                itemIcon.sprite = item.icon;
            }
        }

        if (equipIcon == null)
        {
            Transform equipTr = transform.Find("EquipIcon");
            if (equipTr != null)
            {
                equipIcon = equipTr.GetComponent<Image>();
            }
        }
    }

    public void OnItemClicked()
    {
        if (item != null && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.SelectItem(item, this);
        }
    }

    public void SetEquipped(bool isEquipped)
    {
        if (equipIcon == null)
        {
            Transform equipTr = transform.Find("EquipIcon");
            if (equipTr != null)
            {
                equipIcon = equipTr.GetComponent<Image>();
            }
        }

        if (equipIcon != null)
        {
            equipIcon.gameObject.SetActive(isEquipped);
        }
    }
}
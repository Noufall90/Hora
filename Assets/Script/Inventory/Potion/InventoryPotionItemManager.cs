using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryPotionItemManager : MonoBehaviour
{
    public PotionItem potionItem;
    public Image itemIcon;
    public TMP_Text quantityText;

    private Button itemButton;

    private void Awake()
    {
        itemButton = GetComponent<Button>();
        if (itemButton != null)
        {
            itemButton.onClick.AddListener(OnPotionClicked);
        }
    }

    public void Setup(PotionItem newPotion, int count = 1)
    {
        potionItem = newPotion;

        if (itemIcon == null)
        {
            Transform iconTr = transform.Find("ItemIcon");
            if (iconTr != null) itemIcon = iconTr.GetComponent<Image>();
        }

        if (itemIcon != null && potionItem != null && potionItem.icon != null)
        {
            itemIcon.sprite = potionItem.icon;
        }

        if (quantityText == null)
        {
            Transform countTr = transform.Find("QuantityText");
            if (countTr != null) quantityText = countTr.GetComponent<TMP_Text>();
        }

        if (quantityText != null)
        {
            quantityText.text = count.ToString();
            quantityText.gameObject.SetActive(count >= 1);
        }
    }

    public void OnPotionClicked()
    {
        if (potionItem != null && InventoryPotionManager.Instance != null)
        {
            InventoryPotionManager.Instance.SelectPotion(potionItem, this);
        }
    }
}

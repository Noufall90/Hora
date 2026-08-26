using UnityEngine;

[System.Serializable]
public struct BuyPotionData
{
    public PotionItem potionItem;
    public GameObject buyButtonItem;
    public int price;
    public bool destroyButtonOnBuy;
}

public class BuyPotion : MonoBehaviour
{
    [Header("List Item Potion")]
    public BuyPotionData[] buyPotionItems;

    public void Buy(int index)
    {
        if (buyPotionItems == null || buyPotionItems.Length == 0)
        {
            return;
        }

        if (index < 0 || index >= buyPotionItems.Length)
        {
            return;
        }

        BuyPotionData data = buyPotionItems[index];

        if (CoinCounter.Instance != null)
        {
            if (!CoinCounter.Instance.DecreaseCoin(data.price))
            {
                return;
            }
        }
        else
        {
            Debug.LogWarning("[BuyPotion] CoinCounter.Instance tidak ditemukan di Scene!");
            return;
        }

        if (data.potionItem != null && InventoryPotionManager.Instance != null)
        {
            InventoryPotionManager.Instance.Add(data.potionItem);
            Debug.Log($"[BuyPotion] Berhasil membeli Potion '{data.potionItem.itemName}' ({data.potionItem.type})!");
        }
        else
        {
            if (data.potionItem == null)
            {
                Debug.LogWarning($"[BuyPotion] Field 'potionItem' pada index {index} belum di-assign di Unity Inspector!");
            }
            if (InventoryPotionManager.Instance == null)
            {
                Debug.LogWarning("[BuyPotion] InventoryPotionManager.Instance tidak ditemukan di Scene!");
            }
        }

        if (data.destroyButtonOnBuy && data.buyButtonItem != null)
        {
            Destroy(data.buyButtonItem);
        }
    }

    public void Buy()
    {
        Buy(0);
    }
}

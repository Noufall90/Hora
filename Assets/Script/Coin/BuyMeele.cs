using UnityEngine;
using PlayerWeapons;

[System.Serializable]
public struct BuyMeeleData
{
    public GameObject buyButtonItem;
    public int price;
    public int meleeIndex;
    public Item item;
}

public class BuyMeele : MonoBehaviour
{
    [Header("List Item Melee")]
    public BuyMeeleData[] buyMeeleItems;

    public void Buy(int index)
    {
        if (buyMeeleItems == null || buyMeeleItems.Length == 0)
        {
            return;
        }

        if (index < 0 || index >= buyMeeleItems.Length)
        {
            return;
        }

        BuyMeeleData itemData = buyMeeleItems[index];

        if (CoinCounter.Instance != null)
        {
            if (!CoinCounter.Instance.DecreaseCoin(itemData.price))
            {
                return;
            }
        }

        if (itemData.item == null)
        {
            Debug.LogWarning($"[BuyMeele] Field 'item' pada index {index} belum di-assign di Unity Inspector!");
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[BuyMeele] InventoryManager.Instance tidak ditemukan di Scene!");
        }

        if (itemData.item != null && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.Add(itemData.item);
            Debug.Log($"[BuyMeele] Berhasil membeli {itemData.item.itemName} dan ditambahkan ke Inventory!");
        }

        if (itemData.buyButtonItem != null)
        {
            Destroy(itemData.buyButtonItem);
        }
    }

    public void Buy()
    {
        Buy(0);
    }
}

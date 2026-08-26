using UnityEngine;
using PlayerWeapons;

[System.Serializable]
public struct BuyPistolData
{
    public GameObject buyButtonItem;
    public int price;
    public int pistolIndex;
    public Item item;
}

public class BuyPistol : MonoBehaviour
{
    [Header("List Item Pistol")]
    public BuyPistolData[] buyPistolItems;

    public void Buy(int index)
    {
        if (buyPistolItems == null || buyPistolItems.Length == 0)
        {
            return;
        }

        if (index < 0 || index >= buyPistolItems.Length)
        {
            return;
        }

        BuyPistolData itemData = buyPistolItems[index];

        if (CoinCounter.Instance != null)
        {
            if (!CoinCounter.Instance.DecreaseCoin(itemData.price))
            {
                return;
            }
        }

        if (itemData.item == null)
        {
            Debug.LogWarning($"[BuyPistol] Field 'item' pada index {index} belum di-assign di Unity Inspector!");
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[BuyPistol] InventoryManager.Instance tidak ditemukan di Scene!");
        }

        if (itemData.item != null && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.Add(itemData.item);
            Debug.Log($"[BuyPistol] Berhasil membeli {itemData.item.itemName} dan ditambahkan ke Inventory!");
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
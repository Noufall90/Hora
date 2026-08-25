using UnityEngine;
using PlayerWeapons;

[System.Serializable]
public struct BuyPistolData
{
    public GameObject buyButtonItem;
    public int price;
    public int pistolIndex;
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

        BuyPistolData item = buyPistolItems[index];

        if (CoinCounter.Instance != null)
        {
            if (!CoinCounter.Instance.DecreaseCoin(item.price))
            {
                return;
            }
        }

        if (WeaponsManager.Instance != null)
        {
            WeaponsManager.Instance.EquipPistol(item.pistolIndex);
        }

        if (item.buyButtonItem != null)
        {
            Destroy(item.buyButtonItem);
        }
    }

    public void Buy()
    {
        Buy(0);
    }
}
using UnityEngine;
using PlayerWeapons;

[System.Serializable]
public struct BuyMeeleData
{
    public GameObject buyButtonItem;
    public int price;
    public int meleeIndex;
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

        BuyMeeleData item = buyMeeleItems[index];

        if (CoinCounter.Instance != null)
        {
            if (!CoinCounter.Instance.DecreaseCoin(item.price))
            {
                return;
            }
        }

        if (WeaponsManager.Instance != null)
        {
            WeaponsManager.Instance.EquipMelee(item.meleeIndex);
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

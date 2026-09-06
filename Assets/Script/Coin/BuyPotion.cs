using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("Notification UI")]
    [SerializeField] private GameObject notifTerbeli;
    [SerializeField] private GameObject notifKoinTidakCukup;
    [SerializeField] private float notifDuration = 2f;

    private Coroutine _notifCoroutine;

    private void Start()
    {
        if (notifTerbeli != null) notifTerbeli.SetActive(false);
        if (notifKoinTidakCukup != null) notifKoinTidakCukup.SetActive(false);

        CheckPurchasedState();
    }

    /// <summary>
    /// Memeriksa status pembelian potion untuk item one-time buy jika destroyButtonOnBuy = true.
    /// </summary>
    public void CheckPurchasedState()
    {
        if (buyPotionItems == null || buyPotionItems.Length == 0) return;

        for (int i = 0; i < buyPotionItems.Length; i++)
        {
            BuyPotionData data = buyPotionItems[i];
            if (data.destroyButtonOnBuy && data.potionItem != null)
            {
                if (SaveDataJson.Instance != null && SaveDataJson.Instance.HasSaveFile)
                {
                    if (SaveDataJson.Instance.IsPotionPurchased(data.potionItem.name) && data.buyButtonItem != null)
                    {
                        data.buyButtonItem.SetActive(false);
                    }
                }
            }
        }
    }

    public void Buy(int index)
    {
        if (buyPotionItems == null || buyPotionItems.Length == 0) return;
        if (index < 0 || index >= buyPotionItems.Length) return;

        BuyPotionData data = buyPotionItems[index];

        if (CoinCounter.Instance != null)
        {
            if (!CoinCounter.Instance.DecreaseCoin(data.price))
            {
                ShowNotification(notifKoinTidakCukup);
                return;
            }
        }
        else
        {
            Debug.LogWarning("[BuyPotion] CoinCounter.Instance tidak ditemukan di Scene!");
            ShowNotification(notifKoinTidakCukup);
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

        // Catat ke SaveDataJson
        if (SaveDataJson.Instance != null && data.potionItem != null)
        {
            if (!SaveDataJson.Instance.Data.purchasedPotionNames.Contains(data.potionItem.name))
            {
                SaveDataJson.Instance.Data.purchasedPotionNames.Add(data.potionItem.name);
            }
            SaveDataJson.Instance.SaveGame();
        }

        ShowNotification(notifTerbeli);

        if (data.destroyButtonOnBuy && data.buyButtonItem != null)
        {
            if (data.buyButtonItem == gameObject)
            {
                var btn = GetComponent<Button>();
                if (btn != null) btn.interactable = false;
                Destroy(data.buyButtonItem, notifDuration + 0.1f);
            }
            else
            {
                data.buyButtonItem.SetActive(false);
                Destroy(data.buyButtonItem, 0.2f);
            }
        }
    }

    public void Buy()
    {
        Buy(0);
    }

    private void ShowNotification(GameObject notifObj)
    {
        if (notifObj == null) return;

        if (_notifCoroutine != null)
        {
            StopCoroutine(_notifCoroutine);
        }

        if (notifTerbeli != null) notifTerbeli.SetActive(false);
        if (notifKoinTidakCukup != null) notifKoinTidakCukup.SetActive(false);

        if (gameObject.activeInHierarchy)
        {
            _notifCoroutine = StartCoroutine(NotificationRoutine(notifObj));
        }
        else if (CoinCounter.Instance != null && CoinCounter.Instance.gameObject.activeInHierarchy)
        {
            CoinCounter.Instance.StartCoroutine(NotificationRoutine(notifObj));
        }
    }

    private IEnumerator NotificationRoutine(GameObject notifObj)
    {
        notifObj.SetActive(true);
        yield return new WaitForSecondsRealtime(notifDuration);
        if (notifObj != null)
        {
            notifObj.SetActive(false);
        }
        _notifCoroutine = null;
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
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
    /// Memeriksa apakah pistol sudah dibeli sebelumnya dari SaveDataJson.
    /// Jika sudah terbeli, tombol pembelian akan dinonaktifkan / disembunyikan.
    /// </summary>
    public void CheckPurchasedState()
    {
        if (buyPistolItems == null || buyPistolItems.Length == 0) return;

        for (int i = 0; i < buyPistolItems.Length; i++)
        {
            BuyPistolData itemData = buyPistolItems[i];
            if (itemData.item == null) continue;

            bool isOwned = false;

            // Cek di SaveDataJson
            if (SaveDataJson.Instance != null && SaveDataJson.Instance.HasSaveFile)
            {
                if (SaveDataJson.Instance.IsPistolPurchased(itemData.item.name) ||
                    SaveDataJson.Instance.Data.inventoryItemNames.Contains(itemData.item.name))
                {
                    isOwned = true;
                }
            }

            // Cek di active InventoryManager
            if (!isOwned && InventoryManager.Instance != null && InventoryManager.Instance.items.Contains(itemData.item))
            {
                isOwned = true;
            }

            if (isOwned && itemData.buyButtonItem != null)
            {
                itemData.buyButtonItem.SetActive(false);
            }
        }
    }

    public void Buy(int index)
    {
        if (buyPistolItems == null || buyPistolItems.Length == 0) return;
        if (index < 0 || index >= buyPistolItems.Length) return;

        BuyPistolData itemData = buyPistolItems[index];

        // Validasi apakah sudah dimiliki
        if (itemData.item != null)
        {
            if (SaveDataJson.Instance != null && (SaveDataJson.Instance.IsPistolPurchased(itemData.item.name) ||
                SaveDataJson.Instance.Data.inventoryItemNames.Contains(itemData.item.name)))
            {
                Debug.LogWarning($"[BuyPistol] {itemData.item.itemName} sudah dimiliki!");
                if (itemData.buyButtonItem != null) itemData.buyButtonItem.SetActive(false);
                return;
            }
        }

        if (CoinCounter.Instance != null)
        {
            if (!CoinCounter.Instance.DecreaseCoin(itemData.price))
            {
                ShowNotification(notifKoinTidakCukup);
                return;
            }
        }
        else
        {
            Debug.LogWarning("[BuyPistol] CoinCounter.Instance tidak ditemukan di Scene!");
            ShowNotification(notifKoinTidakCukup);
            return;
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

        // Catat pembelian di SaveDataJson
        if (SaveDataJson.Instance != null && itemData.item != null)
        {
            if (!SaveDataJson.Instance.Data.purchasedPistolNames.Contains(itemData.item.name))
            {
                SaveDataJson.Instance.Data.purchasedPistolNames.Add(itemData.item.name);
            }
            SaveDataJson.Instance.SaveGame();
        }

        ShowNotification(notifTerbeli);

        if (itemData.buyButtonItem != null)
        {
            if (itemData.buyButtonItem == gameObject)
            {
                var btn = GetComponent<Button>();
                if (btn != null) btn.interactable = false;
                Destroy(itemData.buyButtonItem, notifDuration + 0.1f);
            }
            else
            {
                itemData.buyButtonItem.SetActive(false);
                Destroy(itemData.buyButtonItem, 0.2f);
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
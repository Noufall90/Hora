using System.Collections;
using UnityEngine;
using TMPro;

public class CoinCounter : MonoBehaviour
{
    public static CoinCounter Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private GameObject panelCoin;
    [SerializeField] private TextMeshProUGUI coinNambah;

    [Header("Coin Data")]
    [SerializeField] private int coin;

    private Coroutine coinNambahCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Load data koin dari SaveDataJson jika file save ada
        if (SaveDataJson.Instance != null && SaveDataJson.Instance.HasSaveFile)
        {
            coin = SaveDataJson.Instance.Data.totalCoin;
        }

        RefreshUI();

        if (panelCoin != null) panelCoin.SetActive(false);
        if (coinNambah != null) coinNambah.gameObject.SetActive(false);
    }

    // ── Property ──────────────────────────────────────────

    public int Coin
    {
        get => coin;
        set
        {
            coin = value;
            RefreshUI();
            SyncAndSave(); // Pastikan tersimpan setiap kali Coin diubah
        }
    }

    // ── Public API ────────────────────────────────────────

    public void SetCoin(int amount)
    {
        Coin = amount;
    }

    public void IncreaseCoin(int v)
    {
        coin += v;
        RefreshUI();

        if (coinNambahCoroutine != null)
        {
            StopCoroutine(coinNambahCoroutine);
        }
        coinNambahCoroutine = StartCoroutine(ShowCoinNambahRoutine(v));

        // Sync data ke SaveDataJson lalu Auto-save
        SyncAndSave();
    }

    public bool DecreaseCoin(int v)
    {
        if (coin >= v)
        {
            coin -= v;
            RefreshUI();
            Debug.Log($"[CoinCounter] Berhasil mengurangi {v} koin. Sisa: {coin}");

            // Sync data ke SaveDataJson lalu Auto-save
            SyncAndSave();
            return true;
        }

        Debug.LogWarning($"[CoinCounter] Koin tidak cukup! Saat ini: {coin}, dibutuhkan: {v}");
        return false;
    }

    // ── Private Helpers ────────────────────────────────────

    private void SyncAndSave()
    {
        if (SaveDataJson.Instance != null)
        {
            // PENTING: Update struct/class Data di SaveDataJson terlebih dahulu
            SaveDataJson.Instance.Data.totalCoin = coin;
            SaveDataJson.Instance.SaveGame();
        }
    }

    private void RefreshUI()
    {
        if (coinText != null)
        {
            coinText.text = coin.ToString();
        }
    }

    private IEnumerator ShowCoinNambahRoutine(int addedAmount)
    {
        if (coinNambah != null)
        {
            coinNambah.text = "+" + addedAmount.ToString();
            coinNambah.gameObject.SetActive(true);
        }
        if (panelCoin != null)
        {
            panelCoin.SetActive(true);
        }

        yield return new WaitForSeconds(2f);

        if (coinNambah != null)
        {
            coinNambah.gameObject.SetActive(false);
        }
        if (panelCoin != null)
        {
            panelCoin.SetActive(false);
        }
    }
}
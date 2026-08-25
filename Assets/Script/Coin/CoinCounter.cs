using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CoinCounter : MonoBehaviour
{
    public static CoinCounter Instance { get; private set; }
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private GameObject panelCoin;
    [SerializeField] private TextMeshProUGUI coinNambah;
    [SerializeField] private int coin;
    
    void Awake()
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

    private Coroutine coinNambahCoroutine;

    private void Start()
    {
        if (coinText != null)
        {
            coinText.text = coin.ToString();
        }

        if (panelCoin != null)
        {
            panelCoin.SetActive(false);
        }
        if (coinNambah != null)
        {
            coinNambah.gameObject.SetActive(false);
        }
    }
    
    public int Coin
    {
        get => coin;
        set
        {
            coin = value;
            if (coinText != null)
            {
                coinText.text = coin.ToString();
            }
        }
    }

    public void SetCoin(int amount)
    {
        Coin = amount;
    }

    public void IncreaseCoin(int v)
    {
        coin += v;
        if (coinText != null)
        {
            coinText.text = coin.ToString();
        }

        if (coinNambahCoroutine != null)
        {
            StopCoroutine(coinNambahCoroutine);
        }
        coinNambahCoroutine = StartCoroutine(ShowCoinNambahRoutine(v));
    }

    public bool DecreaseCoin(int v)
    {
        if (coin >= v)
        {
            coin -= v;
            if (coinText != null)
            {
                coinText.text = coin.ToString();
            }
            Debug.Log($"[CoinCounter] Berhasil mengurangi {v} koin. Sisa koin: {coin}");
            return true;
        }
        Debug.LogWarning($"[CoinCounter] Koin tidak cukup! Koin saat ini: {coin}, dibutuhkan: {v}");
        return false;
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

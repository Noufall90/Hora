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
        Instance = this;
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

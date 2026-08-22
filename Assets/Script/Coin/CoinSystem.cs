using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinSystem : MonoBehaviour
{
    [SerializeField] private int value;

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            CoinCounter.Instance.IncreaseCoin(value);
            Destroy(gameObject);
        }
    }
}

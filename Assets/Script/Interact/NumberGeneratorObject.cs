using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayerData;

public class NumberGeneratorObject : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Debug.Log("Berhasil interaksi");
        Debug.Log(Random.Range(1, 100));
    }
}

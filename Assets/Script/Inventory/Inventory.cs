using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (inventoryPanel.activeSelf)
            {
                InventoryClose();
            }
            else
            {
                InventoryOpen();
            }
        }
    }

    public void InventoryOpen()
    {
        inventoryPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void InventoryClose()
    {
        inventoryPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}
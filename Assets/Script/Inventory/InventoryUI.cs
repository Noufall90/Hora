using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject inventoryPanel;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (inventoryPanel != null && inventoryPanel.activeSelf)
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
        if (inventoryPanel == null) return;

        UIHandler.OpenWindow(inventoryPanel);

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ListItems();
        }
    }

    public void InventoryClose()
    {
        if (inventoryPanel == null) return;

        UIHandler.CloseWindow(inventoryPanel);
    }
}
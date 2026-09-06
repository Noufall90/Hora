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
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D("Button");
        }
        if (inventoryPanel == null) return;

        UIHandler.OpenWindow(inventoryPanel);

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ListItems();
        }
    }

    public void InventoryClose()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound2D("Button");
        }
        if (inventoryPanel == null) return;

        UIHandler.CloseWindow(inventoryPanel);
    }
}
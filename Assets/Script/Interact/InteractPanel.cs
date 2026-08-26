using UnityEngine;

public class InteractPanel : MonoBehaviour
{
    [SerializeField] private GameObject openPanel;
    [SerializeField] private GameObject quadObject;
    
    private bool playerInRange = false;
    private bool isOpen = false;

    private void Start()
    {
        if (openPanel != null) openPanel.SetActive(false);
        if (quadObject != null) quadObject.SetActive(false);
    }

    private void Update()
    {
        // Tekan E untuk membuka
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isOpen)
        {
            OpenPanel();
        }

        // Tekan ESC untuk menutup
        if (isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            ClosePanel();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (quadObject != null) quadObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (quadObject != null) quadObject.SetActive(false);
            if (isOpen) ClosePanel();
        }
    }

    public void OpenPanel()
    {
        if (openPanel == null) return;

        isOpen = true;
        UIHandler.OpenWindow(openPanel);
    }

    public void ClosePanel()
    {
        if (openPanel == null) return;

        isOpen = false;
        UIHandler.CloseWindow(openPanel);
    }
}
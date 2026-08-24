using UnityEngine;

public class InteractPanel : MonoBehaviour
{
    [SerializeField] private GameObject openPanel;
    [SerializeField] private GameObject quadObject;
    public Collider triggerCollider;
    
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

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (quadObject != null) quadObject.SetActive(true);
        }
    }

    public void OnTriggerExit(Collider other)
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
        if (openPanel != null) openPanel.SetActive(true);

        isOpen = true;
        Time.timeScale = 0f;

        // Mouse bisa digunakan untuk UI
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ClosePanel()
    {
        if (openPanel != null) openPanel.SetActive(false);

        isOpen = false;
        Time.timeScale = 1f;

        // Jika game kamu menggunakan cursor terkunci
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
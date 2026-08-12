using UnityEngine;

public class About : MonoBehaviour
{
    // ============ VARIABLES ============
    [Header("References")]
    [Tooltip("About Us panel this script controls.")]
    [SerializeField] private GameObject aboutMenu;

    // ============ PUBLIC METHODS ============
    public void CloseAbout()
    {
        aboutMenu.SetActive(false);
        Debug.Log($"[About] [{gameObject.name}] closed the about panel.");
    }
}
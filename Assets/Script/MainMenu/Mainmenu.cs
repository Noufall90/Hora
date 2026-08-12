using UnityEngine;

public class MainMenu : MonoBehaviour
{
    // ============ VARIABLES ============
    [Header("Scene")]
    [Tooltip("Name of the scene loaded when the player presses Play. Must be added in Build Settings.")]
    [SerializeField] private string gameScene = "GameScene";

    [Header("References")]
    [Tooltip("Settings panel shown when the player presses Option.")]
    [SerializeField] private GameObject settingsMenu;

    [Tooltip("About Us panel shown when the player presses About Us.")]
    [SerializeField] private GameObject aboutMenu;

    // ============ PUBLIC METHODS ============
    public void Play()
    {
        SceneLoader.Instance.LoadScene(gameScene);
        Debug.Log($"[MainMenu] [{gameObject.name}] loading scene {gameScene}.");
    }

    public void Continue()
    {
        Debug.Log($"[MainMenu] [{gameObject.name}] continue pressed, connect this to your save system.");
    }

    public void OpenSettings()
    {
        settingsMenu.SetActive(true);
    }

    public void OpenAbout()
    {
        aboutMenu.SetActive(true);
    }

    public void ExitGame()
    {
        Debug.Log($"[MainMenu] [{gameObject.name}] exiting game.");
        Application.Quit();
    }
}
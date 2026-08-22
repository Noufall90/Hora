using UnityEngine;

public class MainMenu : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string gameScene = "GameScene";

    [Header("References")]
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject aboutMenu;

    public void Play()
    {
        SceneLoader.Instance.LoadScene(gameScene);
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
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; 
#endif
    }
}
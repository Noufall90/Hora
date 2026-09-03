using UnityEngine;
using EasyTransition;

public class MainMenu : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string gameScene = "GameScene";

    [Header("Transition Settings")]
    [SerializeField] private TransitionSettings transition;
    [SerializeField] private float startDelay = 0.5f;

    [Header("Spawn Settings")]
    [SerializeField] private string targetSpawnID;

    [Header("References")]
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject helpMenu;

    public void NewGame()
    {
        PointLocation.SetSpawnTarget(targetSpawnID, UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        LoadGameScene();
    }

    public void Play()
    {
        NewGame();
    }

    public void Continue()
    {
        PointLocation.SetSpawnTarget(targetSpawnID, UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        LoadGameScene();
    }

    private void LoadGameScene()
    {
        if (TransitionManager.Instance() != null && transition != null)
        {
            TransitionManager.Instance().Transition(gameScene, transition, startDelay);
        }
        else if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadScene(gameScene);
        }
        else if (!string.IsNullOrEmpty(gameScene))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(gameScene);
        }
    }

    public void OpenSettings()
    {
        if (settingsMenu != null) settingsMenu.SetActive(true);
    }

    public void OpenHelp()
    {
        if (helpMenu != null) helpMenu.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsMenu != null) settingsMenu.SetActive(false);
    }

    public void CloseHelp()
    {
        if (helpMenu != null) helpMenu.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; 
#endif
    }
}
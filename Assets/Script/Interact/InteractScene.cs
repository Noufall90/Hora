using UnityEngine;
using EasyTransition;

public class InteractScene : MonoBehaviour
{
    [SerializeField] private GameObject quadObject;

    [Header("Transition Settings")]
    public TransitionSettings transition;
    public float startDelay = 0.5f;
    public string sceneName;

    [Header("Spawn Settings")]
    [Tooltip("ID spawn point di scene tujuan (contoh: SpawnID2). Player akan spawn di PointLocation dengan ID ini.")]
    [SerializeField] private string targetSpawnID;

    private bool playerInRange = false;

    private void Start()
    {
        if (quadObject != null) quadObject.SetActive(false);
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            LoadedScene();
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
        }
    }

    public void LoadedScene()
    {
        PointLocation.SetSpawnTarget(targetSpawnID, UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

        if (TransitionManager.Instance() != null && transition != null)
        {
            TransitionManager.Instance().Transition(sceneName, transition, startDelay);
        }
        else if (!string.IsNullOrEmpty(sceneName))
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
    }
}

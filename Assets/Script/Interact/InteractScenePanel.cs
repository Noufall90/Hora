using UnityEngine;
using EasyTransition;

public class InteractScenePanel : MonoBehaviour
{
    [SerializeField] private GameObject quadObject;
    [SerializeField] private GameObject panelMapScene;

    public TransitionSettings transition;
    public float startDelay = 1f;
    public string sceneName;

    private bool playerInRange = false;
    private bool isOpen = false;

    private void Start()
    {
        if (quadObject != null) quadObject.SetActive(false);
        if (panelMapScene != null) panelMapScene.SetActive(false);
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E) && !isOpen)
        {
            OpenPanel();
        }

        if (Input.GetKeyDown(KeyCode.Escape) && isOpen)
        {
            ClosePanel();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !isOpen)
        {
            playerInRange = true;
            if (quadObject != null) quadObject.SetActive(true);
            if (panelMapScene != null) panelMapScene.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (quadObject != null) quadObject.SetActive(false);
            if (panelMapScene != null) panelMapScene.SetActive(false);
        }
    }

    public void OpenPanel()
    {
        if (panelMapScene != null) panelMapScene.SetActive(true);
        isOpen = true;
    }

    public void ClosePanel()
    {
        if (panelMapScene != null) panelMapScene.SetActive(false);
        isOpen = false;
    }

    public void LoadedScene()
    {
        TransitionManager.Instance().Transition(sceneName, transition, startDelay);
    }
}

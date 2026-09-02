using UnityEngine;
using EasyTransition;

public class InteractScene : MonoBehaviour
{
    [SerializeField] private GameObject quadObject;

    public TransitionSettings transition;
    public float startDelay = 1f;
    public string sceneName;

    private bool playerInRange = false;
    private bool isOpen = false;

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
        TransitionManager.Instance().Transition(sceneName, transition, startDelay);
    }
}

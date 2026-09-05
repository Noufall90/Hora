using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Data")]
    [SerializeField] private Dialogue dialogue;

    [Header("Interaction")]
    [SerializeField] private GameObject quadObject;

    private bool playerInRange;

    public Dialogue DialogueData => dialogue;

    private void Start()
    {
        if (quadObject != null)
        {
            quadObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!playerInRange)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryStartDialogue();
        }
    }

    private void TryStartDialogue()
    {
        if (dialogue == null)
        {
            Debug.LogWarning(
                $"Dialogue belum diisi pada {gameObject.name}",
                gameObject
            );

            return;
        }

        if (IsDialogueActive())
        {
            return;
        }

        if (quadObject != null)
        {
            quadObject.SetActive(false);
        }

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(dialogue);
            return;
        }

        if (DialogueDefault.Instance != null)
        {
            DialogueDefault.Instance.StartDialogue(dialogue);
        }
    }

    private bool IsDialogueActive()
    {
        if (DialogueManager.Instance != null &&
            DialogueManager.Instance.isDialogueActive)
        {
            return true;
        }

        if (DialogueDefault.Instance != null &&
            DialogueDefault.Instance.isDialogueActive)
        {
            return true;
        }

        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInRange = true;

        if (!IsDialogueActive() && quadObject != null)
        {
            quadObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInRange = false;

        if (quadObject != null)
        {
            quadObject.SetActive(false);
        }
    }

    public void DisplayNextDialogueLine()
    {
        if (DialogueManager.Instance != null &&
            DialogueManager.Instance.isDialogueActive)
        {
            DialogueManager.Instance.DisplayNextDialogueLine();
            return;
        }

        if (DialogueDefault.Instance != null &&
            DialogueDefault.Instance.isDialogueActive)
        {
            DialogueDefault.Instance.DisplayNextDialogueLine();
        }
    }
}
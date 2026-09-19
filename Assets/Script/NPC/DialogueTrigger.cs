using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Data")]
    [SerializeField] private Dialogue dialogue;

    [Header("Interaction")]
    [SerializeField] private GameObject quadObject;

    private bool playerInRange;

    public Dialogue DialogueData => dialogue;

    private static float s_lastDialogueEndTime = -1f;
    private const float INTERACTION_COOLDOWN = 0.35f;

    public static void NotifyDialogueEnded()
    {
        s_lastDialogueEndTime = Time.unscaledTime;
    }

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

        if (IsDialogueActive())
        {
            if (quadObject != null && quadObject.activeSelf)
            {
                quadObject.SetActive(false);
            }
            return;
        }

        if (Time.unscaledTime - s_lastDialogueEndTime < INTERACTION_COOLDOWN)
        {
            if (quadObject != null && quadObject.activeSelf)
            {
                quadObject.SetActive(false);
            }
            return;
        }

        if (quadObject != null && !quadObject.activeSelf)
        {
            quadObject.SetActive(true);
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

        if (Time.unscaledTime - s_lastDialogueEndTime < INTERACTION_COOLDOWN)
        {
            return;
        }

        if (quadObject != null)
        {
            quadObject.SetActive(false);
        }

        DialogueManager localManager = GetComponent<DialogueManager>();
        if (localManager != null && localManager.gameObject.activeInHierarchy)
        {
            localManager.StartDialogue(dialogue);
            return;
        }

        DialogueDefault localDefault = GetComponent<DialogueDefault>();
        if (localDefault != null && localDefault.gameObject.activeInHierarchy)
        {
            localDefault.StartDialogue(dialogue);
            return;
        }

        if (DialogueManager.Instance != null && DialogueManager.Instance.gameObject.activeInHierarchy)
        {
            DialogueManager.Instance.StartDialogue(dialogue);
            return;
        }

        if (DialogueDefault.Instance != null && DialogueDefault.Instance.gameObject.activeInHierarchy)
        {
            DialogueDefault.Instance.StartDialogue(dialogue);
        }
    }

    private bool IsDialogueActive()
    {
        DialogueManager localManager = GetComponent<DialogueManager>();
        if (localManager != null && localManager.gameObject.activeInHierarchy && localManager.isDialogueActive)
        {
            return true;
        }

        DialogueDefault localDefault = GetComponent<DialogueDefault>();
        if (localDefault != null && localDefault.gameObject.activeInHierarchy && localDefault.isDialogueActive)
        {
            return true;
        }

        if (DialogueManager.Instance != null &&
            DialogueManager.Instance.gameObject.activeInHierarchy &&
            DialogueManager.Instance.isDialogueActive)
        {
            return true;
        }

        if (DialogueDefault.Instance != null &&
            DialogueDefault.Instance.gameObject.activeInHierarchy &&
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

        if (!IsDialogueActive() &&
            Time.unscaledTime - s_lastDialogueEndTime >= INTERACTION_COOLDOWN &&
            quadObject != null)
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
        DialogueManager localManager = GetComponent<DialogueManager>();
        if (localManager != null && localManager.gameObject.activeInHierarchy && localManager.isDialogueActive)
        {
            localManager.DisplayNextDialogueLine();
            return;
        }

        DialogueDefault localDefault = GetComponent<DialogueDefault>();
        if (localDefault != null && localDefault.gameObject.activeInHierarchy && localDefault.isDialogueActive)
        {
            localDefault.DisplayNextDialogueLine();
            return;
        }

        if (DialogueManager.Instance != null &&
            DialogueManager.Instance.gameObject.activeInHierarchy &&
            DialogueManager.Instance.isDialogueActive)
        {
            DialogueManager.Instance.DisplayNextDialogueLine();
            return;
        }

        if (DialogueDefault.Instance != null &&
            DialogueDefault.Instance.gameObject.activeInHierarchy &&
            DialogueDefault.Instance.isDialogueActive)
        {
            DialogueDefault.Instance.DisplayNextDialogueLine();
        }
    }
}
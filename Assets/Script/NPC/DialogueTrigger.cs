using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueCharacter
{
    public string name;
}

[System.Serializable]
public class DialogueLine
{
    public DialogueCharacter character;
    [TextArea(3, 10)]
    public string line;
}

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Dialogue Data")]
public class Dialogue : ScriptableObject
{
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();
}

public class DialogueTrigger : MonoBehaviour
{
    public static DialogueTrigger Instance;

    [Header("Dialogue ScriptableObject")]
    public Dialogue dialogue;

    [Header("Interaction")]
    [SerializeField] private GameObject quadObject;

    private bool playerInRange = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
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
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            bool isAnyDialogueActive =
                (DialogueManager.Instance != null &&
                 DialogueManager.Instance.isDialogueActive) ||

                (DialogueDefault.Instance != null &&
                 DialogueDefault.Instance.isDialogueActive);

            if (!isAnyDialogueActive)
            {
                TriggerDialogue();
            }
        }
    }

    public void TriggerDialogue()
    {
        if (dialogue == null)
            return;

        if (quadObject != null)
        {
            quadObject.SetActive(false);
        }

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(dialogue);
        }
        else if (DialogueDefault.Instance != null)
        {
            DialogueDefault.Instance.StartDialogue(dialogue);
        }
    }

    public void DisplayNextDialogueLine()
    {
        if (DialogueManager.Instance != null &&
            DialogueManager.Instance.isDialogueActive)
        {
            DialogueManager.Instance.DisplayNextDialogueLine();
        }
        else if (DialogueDefault.Instance != null &&
                 DialogueDefault.Instance.isDialogueActive)
        {
            DialogueDefault.Instance.DisplayNextDialogueLine();
        }
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        playerInRange = true;

        bool isAnyDialogueActive =
            (DialogueManager.Instance != null &&
             DialogueManager.Instance.isDialogueActive) ||

            (DialogueDefault.Instance != null &&
             DialogueDefault.Instance.isDialogueActive);

        if (quadObject != null && !isAnyDialogueActive)
        {
            quadObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        playerInRange = false;

        if (quadObject != null)
        {
            quadObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
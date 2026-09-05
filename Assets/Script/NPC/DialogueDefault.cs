using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using PlayerData;

public class DialogueDefault : MonoBehaviour
{
    public static DialogueDefault Instance;

    [Header("Reference")]
    [SerializeField] private DialogueManager dialogueManager;
    [SerializeField] private DialogueTrigger dialogueTrigger;

    [Header("UI Component")]
    [SerializeField] private Button nextButton;
    [SerializeField] private TextMeshProUGUI characterName;
    [SerializeField] private TextMeshProUGUI dialogueArea;

    [Header("UI dan Collider")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject colArea;
    [SerializeField] private Collider colTrigger;

    [Header("Dialogue Settings")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private float typingSpeed = 0.2f;

    private Queue<DialogueLine> lines;
    private bool isTyping;
    private string currentSentence = "";

    private CursorLockMode previousCursorLockState;
    private bool previousCursorVisible;

    public bool isDialogueActive = false;

    private bool playerInRange = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        lines = new Queue<DialogueLine>();

        if (dialogueManager == null)
        {
            dialogueManager = DialogueManager.Instance;
        }

        if (dialogueManager != null &&
            dialogueManager.gameObject != gameObject)
        {
            gameObject.SetActive(false);
            return;
        }

        if (dialogueTrigger == null)
        {
            dialogueTrigger = GetComponent<DialogueTrigger>();
        }
    }

    private void Start()
    {
        if (dialogueManager == null)
        {
            dialogueManager = DialogueManager.Instance;
        }

        if (dialogueTrigger == null)
        {
            dialogueTrigger = GetComponent<DialogueTrigger>();
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (colArea != null)
        {
            colArea.SetActive(false);
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(DisplayNextDialogueLine);
            nextButton.onClick.AddListener(DisplayNextDialogueLine);
        }
    }

    private void Update()
    {
        if (DialogueManager.Instance != null &&
            DialogueManager.Instance.isDialogueActive)
        {
            return;
        }

        if (playerInRange &&
            Input.GetKeyDown(KeyCode.E) &&
            !isDialogueActive)
        {
            Dialogue targetDialogue =
                dialogueTrigger != null
                    ? dialogueTrigger.DialogueData
                    : null;

            if (targetDialogue != null)
            {
                StartDialogue(targetDialogue);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInRange = false;
    }

    public void StartDialogue(Dialogue dialogue = null)
    {
        if (dialogue == null && dialogueTrigger != null)
        {
            dialogue = dialogueTrigger.DialogueData;
        }

        if (dialogue == null ||
            dialogue.dialogueLines == null ||
            dialogue.dialogueLines.Count == 0)
        {
            return;
        }

        isDialogueActive = true;
        isTyping = false;
        currentSentence = "";

        StartPlayerTalking();

        if (PlayerAnimAttack.Instance != null)
        {
            PlayerAnimAttack.Instance.CanUseWeapons = false;
        }

        if (dialoguePanel != null)
        {
            UIHandler.OpenDialogue(dialoguePanel);
        }

        if (colArea != null)
        {
            colArea.SetActive(true);
        }

        lines.Clear();

        foreach (DialogueLine dialogueLine in dialogue.dialogueLines)
        {
            lines.Enqueue(dialogueLine);
        }

        DisplayNextDialogueLine();
    }

    public void DisplayNextDialogueLine()
    {
        if (!isDialogueActive)
        {
            return;
        }

        if (isTyping)
        {
            StopAllCoroutines();

            if (dialogueArea != null)
            {
                dialogueArea.text = currentSentence;
            }

            isTyping = false;
            return;
        }

        if (lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine currentLine = lines.Dequeue();

        if (characterName != null)
        {
            if (currentLine.character != null)
            {
                characterName.text = currentLine.character.name;
            }
            else
            {
                characterName.text = "";
            }
        }

        currentSentence = currentLine.line ?? "";

        StopAllCoroutines();
        StartCoroutine(TypeSentence(currentSentence));
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;

        if (dialogueArea != null)
        {
            dialogueArea.text = "";

            foreach (char letter in sentence)
            {
                dialogueArea.text += letter;
                yield return new WaitForSecondsRealtime(typingSpeed);
            }
        }

        isTyping = false;
    }

    private void StartPlayerTalking()
    {
        previousCursorLockState = Cursor.lockState;
        previousCursorVisible = Cursor.visible;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerAnimator != null)
        {
            playerAnimator.SetBool("Idle", true);
            playerAnimator.SetTrigger("IsTalking");
        }
    }

    private void StopPlayerTalking()
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("Idle", false);
        }

        Cursor.lockState = previousCursorLockState;
        Cursor.visible = previousCursorVisible;
    }

    public void EndDialogue()
    {
        isDialogueActive = false;
        isTyping = false;

        StopAllCoroutines();
        StopPlayerTalking();

        if (PlayerAnimAttack.Instance != null)
        {
            PlayerAnimAttack.Instance.CanUseWeapons = true;
        }

        if (dialoguePanel != null)
        {
            UIHandler.CloseDialogue(dialoguePanel);
        }

        if (colArea != null)
        {
            colArea.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();

        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(DisplayNextDialogueLine);
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
}
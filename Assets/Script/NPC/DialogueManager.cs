using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using PlayerData;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("Reference")]
    [SerializeField] private DialogueTrigger dialogueTrigger;
    [SerializeField] private DialogueDefault dialogueDefault;

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
    [SerializeField] private float typingSpeed = 0.04f;

    private Queue<DialogueLine> lines;
    private bool isTyping;
    private string currentSentence = "";

    private CursorLockMode previousCursorLockState;
    private bool previousCursorVisible;

    public bool isDialogueActive = false;

    public string DialogueId
    {
        get
        {
            if (transform.parent != null)
            {
                return transform.parent.name + "_" + gameObject.name;
            }
            return gameObject.name;
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        lines = new Queue<DialogueLine>();

        if (dialogueTrigger == null)
        {
            dialogueTrigger = GetComponent<DialogueTrigger>();
        }

        if (dialogueDefault == null)
        {
            dialogueDefault = DialogueDefault.Instance;
        }

        CheckDialogueState();
    }

    private void Start()
    {
        CheckDialogueState();

        if (nextButton != null)
        {
            nextButton.onClick.RemoveListener(DisplayNextDialogueLine);
            nextButton.onClick.AddListener(DisplayNextDialogueLine);
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        if (colArea != null)
        {
            colArea.SetActive(false);
        }
    }

    public void CheckDialogueState()
    {
        string id1 = DialogueId;
        string id2 = gameObject.name;

        bool isCompleted = false;
        if (SaveDataJson.Instance != null)
        {
            isCompleted = SaveDataJson.Instance.IsDialogueCompleted(id1) || SaveDataJson.Instance.IsDialogueCompleted(id2);
        }
        else
        {
            isCompleted = DialogueSaveRegistry.IsCompleted(id1) || DialogueSaveRegistry.IsCompleted(id2);
        }

        if (isCompleted)
        {
            if (dialogueDefault == null)
            {
                dialogueDefault = DialogueDefault.Instance;
            }

            if (dialogueDefault != null)
            {
                dialogueDefault.gameObject.SetActive(true);
            }

            gameObject.SetActive(false);
            return;
        }
        else
        {
            if (dialogueDefault == null)
            {
                dialogueDefault = DialogueDefault.Instance;
            }

            if (dialogueDefault != null && dialogueDefault.gameObject != gameObject)
            {
                dialogueDefault.gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        if (isDialogueActive)
        {
            if (Cursor.lockState != CursorLockMode.None || !Cursor.visible)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                DisplayNextDialogueLine();
            }
        }
    }

    public void StartDialogue(Dialogue dialogue = null)
    {
        if (dialogue == null && dialogueTrigger != null)
        {
            dialogue = dialogueTrigger.DialogueData;
        }

        if (dialogue == null || dialogue.dialogueLines == null || dialogue.dialogueLines.Count == 0)
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
            playerAnimator.SetTrigger("IsTalking");
        }
    }

    private void StopPlayerTalking()
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("IsIdle", true);
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

        string id1 = DialogueId;
        string id2 = gameObject.name;

        DialogueSaveRegistry.MarkCompleted(id1);
        DialogueSaveRegistry.MarkCompleted(id2);

        if (SaveDataJson.Instance != null)
        {
            SaveDataJson.Instance.MarkDialogueCompleted(id1);
            SaveDataJson.Instance.MarkDialogueCompleted(id2);
        }

        if (dialogueDefault == null)
        {
            dialogueDefault = DialogueDefault.Instance;
        }

        if (dialogueDefault != null)
        {
            dialogueDefault.gameObject.SetActive(true);
        }
        gameObject.SetActive(false);
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
using System.Collections.Generic;
using UnityEngine;

public static class UIHandler
{
    private static bool autoClosePreviousWindow = true;
    private static List<GameObject> activeWindows = new List<GameObject>();
    private static List<GameObject> dialogueWindows = new List<GameObject>();
    public static bool IsDialogueOpen => dialogueWindows.Count > 0;

    public static void OpenWindow(GameObject windowPanel, bool closeOthers = true)
    {
        if (windowPanel == null) return;

        if (closeOthers && autoClosePreviousWindow)
        {
            CloseAllExcept(windowPanel);
        }

        if (!activeWindows.Contains(windowPanel))
        {
            activeWindows.Add(windowPanel);
        }

        windowPanel.SetActive(true);
        UpdateUIState();
    }

    public static void CloseWindow(GameObject windowPanel)
    {
        if (windowPanel == null) return;

        if (activeWindows.Contains(windowPanel))
        {
            activeWindows.Remove(windowPanel);
        }

        windowPanel.SetActive(false);
        UpdateUIState();
    }

    public static void ToggleWindow(GameObject windowPanel, bool closeOthers = true)
    {
        if (windowPanel == null) return;

        if (windowPanel.activeSelf)
        {
            CloseWindow(windowPanel);
        }
        else
        {
            OpenWindow(windowPanel, closeOthers);
        }
    }

    public static void OpenDialogue(GameObject dialoguePanel)
    {
        if (dialoguePanel == null) return;

        if (!dialogueWindows.Contains(dialoguePanel))
            dialogueWindows.Add(dialoguePanel);

        dialoguePanel.SetActive(true);
        UpdateUIState();
    }

    public static void CloseDialogue(GameObject dialoguePanel)
    {
        if (dialoguePanel == null) return;

        dialogueWindows.Remove(dialoguePanel);
        dialoguePanel.SetActive(false);
        UpdateUIState();
    }

    public static void CloseAll()
    {
        for (int i = activeWindows.Count - 1; i >= 0; i--)
        {
            if (activeWindows[i] != null)
            {
                activeWindows[i].SetActive(false);
            }
        }
        activeWindows.Clear();
        UpdateUIState();
    }

    public static void CloseAllExcept(GameObject keepOpenWindow)
    {
        for (int i = activeWindows.Count - 1; i >= 0; i--)
        {
            GameObject window = activeWindows[i];
            if (window != null && window != keepOpenWindow)
            {
                window.SetActive(false);
                activeWindows.RemoveAt(i);
            }
        }
        UpdateUIState();
    }

    public static bool IsAnyWindowOpen()
    {
        for (int i = activeWindows.Count - 1; i >= 0; i--)
        {
            if (activeWindows[i] == null || !activeWindows[i].activeSelf)
            {
                activeWindows.RemoveAt(i);
            }
        }
        return activeWindows.Count > 0;
    }

    public static void UpdateUIState()
    {
        bool anyWindow = IsAnyWindowOpen();

        for (int i = dialogueWindows.Count - 1; i >= 0; i--)
        {
            if (dialogueWindows[i] == null || !dialogueWindows[i].activeSelf)
            {
                dialogueWindows.RemoveAt(i);
            }
        }
        bool anyDialogue = dialogueWindows.Count > 0;
        bool anyOpen = anyWindow || anyDialogue;

        Time.timeScale = anyWindow ? 0f : 1f;

        Cursor.lockState = anyOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = anyOpen;
    }
}

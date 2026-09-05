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

    // ── Dialogue-specific helpers ─────────────────────────────────────────
    /// <summary>
    /// Register a dialogue panel as open.  Does NOT close other windows and
    /// does NOT freeze time or hide the cursor – dialogue runs in real-time.
    /// </summary>
    public static void OpenDialogue(GameObject dialoguePanel)
    {
        if (dialoguePanel == null) return;

        if (!dialogueWindows.Contains(dialoguePanel))
            dialogueWindows.Add(dialoguePanel);

        dialoguePanel.SetActive(true);
        // Dialogue does not change timeScale / cursor – game keeps running.
    }

    /// <summary>
    /// Unregister a dialogue panel.  Re-evaluates overall UI state afterwards.
    /// </summary>
    public static void CloseDialogue(GameObject dialoguePanel)
    {
        if (dialoguePanel == null) return;

        dialogueWindows.Remove(dialoguePanel);
        dialoguePanel.SetActive(false);
        UpdateUIState();
    }

    // ── Standard window helpers ───────────────────────────────────────────
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
        // NOTE: dialogue windows are intentionally NOT closed here.
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

    private static void UpdateUIState()
    {
        bool anyOpen = IsAnyWindowOpen();

        // Dialogue windows do NOT freeze the game – only regular (pause-like) windows do.
        Time.timeScale = anyOpen ? 0f : 1f;

        Cursor.lockState = anyOpen ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = anyOpen;
    }
}

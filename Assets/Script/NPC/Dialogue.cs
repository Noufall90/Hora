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

[CreateAssetMenu(
    fileName = "New Dialogue",
    menuName = "Dialogue/Dialogue Data"
)]
public class Dialogue : ScriptableObject
{
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();
}
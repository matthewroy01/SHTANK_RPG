using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Conversation", menuName = "Dialogue/Conversation", order = 1)]
public class DialogueDefinition : ScriptableObject
{
    public List<DialogueSentence> sentences = new List<DialogueSentence>();
}

[System.Serializable]
public class DialogueSentence
{
    public DialogueSpeaker speaker;
    [TextArea(2, 2)]
    public string content;
    public bool left;
}
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Node")]
public class DialogueNode : ScriptableObject
{
    public string speakerName;
    [Tooltip("Optional Voice Blip")]
    public AudioClip voiceBlip;
    [TextArea(3,6)] public List<string> lines;
    public List<DialogueChoice> choices;

    [Serializable]
    public class DialogueChoice
    {
        public string choice;
        public DialogueNode nextNode;
    }
}

using UnityEngine;

[System.Serializable]
public class DialogueAudioReferences
{
    public AudioSource AudioSource;
    [Tooltip("Add a backup voice incase DialogueNode doesn't supply one.")]
    public AudioClip DefaultBlip;
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Speaker", menuName = "Dialogue/Speaker", order = 1)]
public class DialogueSpeaker : ScriptableObject
{
    public string speakerName;

    public ManagedAudio voiceBlip;

    [Header("Emotion Sprites")]
    public Sprite normal;
    public Sprite happy;
    public Sprite sad;
    public Sprite angry;
    public Sprite sigh;
}

public enum DialogueSpeaker_Emotion { normal, happy, sad, angry, sigh };
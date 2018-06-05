using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue")]
public class ObjDialogue : ScriptableObject
{
	[Header("Write conversation here...")]
	public Dialogue[] listOfText;
}

[System.Serializable]
public class Dialogue
{
	public string whoIsSpeaking = "";
	[TextArea(2, 10)]
	public string text;
	public Effects effects;
}

[System.Serializable]
public class Effects
{
	public TextEffect effect = TextEffect.normal;
	public float fontSizeMultiplier = 1;
	public Sprite characterSprite;
}

public enum TextEffect {normal, shaking, wavy};
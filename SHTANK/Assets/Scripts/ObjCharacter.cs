using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "Character")]
public class ObjCharacter : ScriptableObject
{
	[Header("Name and description")]
	// writing "new" here allows the creation of a string called "name" because gameobject.name already exists
	public new string name;
	public string role;
	[TextArea(3, 10)]
	public string description;

	[Header("Character stats, mouse over for description")]
	public Stats stats;

	[Header("Default sprite for debugging")]
	public Sprite defaultSprite;
}

[System.Serializable]
public class Stats
{
	[Tooltip("If a character's HP hits 0, they die")]
	public int hp;
	[Tooltip("A measure of a character's strength")]
	public int pow;
	[Tooltip("A measure of a character's endurance when taking hits")]
	public int def;
	[Tooltip("How quick a character is")]
	public int spd;
	[Tooltip("How many spaces a character can move in combat")]
	public int mov;
}
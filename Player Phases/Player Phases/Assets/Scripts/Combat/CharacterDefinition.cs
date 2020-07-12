using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Character Definition", menuName = "Character Definition", order = 1)]
public class CharacterDefinition : ScriptableObject
{
    public string characterName;
    [TextArea(3, 10)]
    public string characterDescription;
    public Sprite portrait;

    [Header("Stats")]
    public int healthMax;
    public int attackMod;
    public int defenseMod;
    [Range(0, 100)]
    public int nashbalm;
    public uint movementRange;

    [Header("Abilities")]
    public Moveset moveset;

    [Header("Movement Dialogue")]
    public MovementDialogue movementDialogue;
    public AudioClip movementDialogueSound;

    [Header("Navigable Terrain")]
    public List<GridSpace_TerrainType> terrainTypes;

    [Header("Passive Functionality to Include on Start")]
    public Passive passiveFunctionality;

    [Header("TEMP SPRITE")]
    public Sprite sprite;
}
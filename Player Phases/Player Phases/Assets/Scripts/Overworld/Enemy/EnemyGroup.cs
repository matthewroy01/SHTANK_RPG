using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Group", menuName = "Enemy Group", order = 1)]
public class EnemyGroup : ScriptableObject
{
    [Header("Percentage of enemies to randomly spawn (1 = all enemies, 0 = none")]
    [Range(0.0f, 1.0f)]
    public float spawnPercentage = 1.0f;

    [Header("List of Enemies")]
    public List<CharacterDefinition> characterDefinitions;

    [Header("Music for this group")]
    public ManagedAudio music;
}

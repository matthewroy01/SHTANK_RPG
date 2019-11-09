using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Path Ability", menuName = "Movesets/Path Ability", order = 1)]
public class PathAbility : Ability
{
    [Header("List of spacial directions to have the ability move in")]
    public List<AbilityPath> path;
}

[System.Serializable]
public class AbilityPath
{
    public AbilityDirection directions;
    public int amount;
}
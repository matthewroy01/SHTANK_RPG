using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Copy Ability", menuName = "Movesets/Copy Ability", order = 1)]
public class CopyAbility : Ability
{
    [Header("How many abilities to copy from the target character's moveset (-1 for all)")]
    public int num = -1;
}

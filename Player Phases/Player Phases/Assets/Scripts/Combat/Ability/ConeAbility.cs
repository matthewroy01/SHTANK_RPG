using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Cone Ability", menuName = "Movesets/Cone Ability", order = 1)]
public class ConeAbility : Ability
{
    [Header("Length of the cone as it spreads outwards in a direction")]
    public uint length;

    [Header("How many spaces to expand by (higher num = thinner, lower num = wider")]
    public float angle;
}
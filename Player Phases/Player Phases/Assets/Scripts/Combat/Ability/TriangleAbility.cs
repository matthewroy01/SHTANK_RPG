using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Triangle Ability", menuName = "Movesets/Triangle Ability", order = 1)]
public class TriangleAbility : Ability
{
    [Header("Length of this triangle (-1 is default and will complete the triangle)")]
    public int length = -1;

    [Header("Half of the width of the base of the triangle")]
    public uint halfWidth;

    [Header("The angle at which this triangle expands out to its point")]
    public float angle;
}

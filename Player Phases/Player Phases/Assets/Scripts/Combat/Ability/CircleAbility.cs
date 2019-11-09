using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Circle Ability", menuName = "Movesets/Circle Ability", order = 1)]
public class CircleAbility : Ability
{
    [Header("Radius around a center point to extend from, 0 indicates just the center point")]
    public uint radius;
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Rectangle Ability", menuName = "Movesets/Rectangle Ability", order = 1)]
public class RectangleAbility : Ability
{
    [Header("Corners of the rectangle, relative to the character's position")]
    public Vector2Int bottomLeft;
    public Vector2Int topRight;
}
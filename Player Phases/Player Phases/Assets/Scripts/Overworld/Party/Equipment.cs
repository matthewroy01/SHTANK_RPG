using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Equipment", menuName = "Equipment", order = 1)]
public class Equipment : ScriptableObject
{
    [TextArea(3, 10)]
    public string description;

    [Header("Stats")]
    public int attack;
    public int defense;
    public int movement;
    public int nashbalm;
}

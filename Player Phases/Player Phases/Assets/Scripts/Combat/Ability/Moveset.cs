using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Moveset", menuName = "Movesets/Moveset", order = 1)]
public class Moveset : ScriptableObject
{
    public Ability ability1;
    public Ability ability2;
    public Ability ability3;
    public Ability ability4;
}
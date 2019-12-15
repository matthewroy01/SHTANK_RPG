using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : ScriptableObject
{
    [Header("Move the character along the path? Otherwise, the attack is ranged")]
    public bool moveCharacter;
    [Header("Whether or not the ability should stop at walls")]
    public bool ignoreWalls;
    [Header("Whether or not the ability can be triggered away from its user")]
    public bool ranged;
    public uint range;
    [Header("Effects to apply when striking another character")]
    public List<Effect> effects = new List<Effect>();
}

public enum AbilityDirection { forwards, sideways, sidewaysOpposite, backwards };
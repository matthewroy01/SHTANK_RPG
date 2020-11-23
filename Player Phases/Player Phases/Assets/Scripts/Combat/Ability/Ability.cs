using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : ScriptableObject
{
    [Header("Text description for UI")]
    [TextArea(2, 3)]
    public string description;
    [Header("Move the character along the path?")]
    public bool moveCharacter;
    [Header("Whether or not the ability should stop at walls")]
    public bool ignoreWalls;
    [Header("Whether or not to take the modifiers into account when dealing damage")]
    public bool ignoreAttackMod;
    public bool ignoreDefenseMod;
    [Header("Whether or not the ability can be triggered away from its user")]
    public bool ranged;
    public uint range;
    [Header("Number of times this ability can be used (-1 for unlimited)")]
    public int uses = 1;
    public bool endTurn = true;
    [Header("Effects to apply when striking another character")]
    public List<Effect> effects = new List<Effect>();

    public void ApplySourceInfo(Character newSource)
    {
        for (int i = 0; i < effects.Count; ++i)
        {
            effects[i].source = newSource;

            if (effects[i].id == Effect_ID.damage)
            {
                effects[i].pierceDefense = ignoreDefenseMod;
                effects[i].trueDamage = ignoreAttackMod;
            }
        }
    }

    public int GetTotalDamage()
    {
        int result = 0;

        for (int i = 0; i < effects.Count; ++i)
        {
            if (effects[i].id == Effect_ID.damage)
            {
                result += effects[i].value;
            }
        }

        return result;
    }

    public int GetTotalHealing()
    {
        int result = 0;

        for (int i = 0; i < effects.Count; ++i)
        {
            if (effects[i].id == Effect_ID.healing)
            {
                result += effects[i].value;
            }
        }

        return result;
    }
}

public class AbilityData
{
    public int availableUses;
}

public enum AbilityDirection { forwards, sideways, sidewaysOpposite, backwards };
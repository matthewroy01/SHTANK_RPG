using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CharacterUI
{
    public static string GetStatsUI(Character character)
    {
        string result = "";

        if (character != null)
        {
            string plusOrMinusAtk = "", plusOrMinusDef = "";

            if (character.attackMod >= 0)
            {
                plusOrMinusAtk = "+";
            }
            else
            {
                plusOrMinusAtk = "-";
            }

            if (character.defenseMod >= 0)
            {
                plusOrMinusDef = "+";
            }
            else
            {
                plusOrMinusDef = "-";
            }

            result = character.name + " • HP " + character.healthCurrent + "/" + character.healthMax + "\n" +
                "Atk " + plusOrMinusAtk + character.attackMod + " • Def " + plusOrMinusDef + character.defenseMod + "\n" +
                "NB " + character.nashbalm + "% • Mov " + character.movementRangeCurrent;
        }

        return result;
    }
}

public class CharacterUIDefinition
{
    public string stats;
    public string abil1;
    public string abil2;
    public string abil3;
    public string abil4;
}
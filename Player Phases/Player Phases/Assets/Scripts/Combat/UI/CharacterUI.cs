using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

    public static CharacterAbilityUI GetAbilityUI(PlayerBase character)
    {
        CharacterAbilityUI result = new CharacterAbilityUI();

        if (character != null)
        {
            Moveset moveset = character.moveset;

            if (moveset != null)
            {
                if (moveset.ability1 != null)
                {
                    result.abil1 += CreateAbilityUI(moveset.ability1, 1);
                }

                if (moveset.ability2 != null)
                {
                    result.abil2 += CreateAbilityUI(moveset.ability2, 2);
                }

                if (moveset.ability3 != null)
                {
                    result.abil3 += CreateAbilityUI(moveset.ability3, 3);
                }

                if (moveset.ability4 != null)
                {
                    result.abil4 += CreateAbilityUI(moveset.ability4, 4);
                }
            }
        }

        return result;
    }

    private static string CreateAbilityUI(Ability abil, int num)
    {
        string result = "";

        int damageNum = 0;
        int healingNum = 0;
        List<string> effectNames = new List<string>();
        string effects = "";

        for (int i = 0; i < abil.effects.Count; ++i)
        {
            if (abil.effects[i].id == Effect_ID.damage)
            {
                damageNum += abil.effects[i].value;
            }
            else if (abil.effects[i].id == Effect_ID.healing)
            {
                healingNum += abil.effects[i].value;
            }
            else
            {
                string tmp = Enum.GetName(typeof(Effect_ID), abil.effects[i].id);

                if (!effectNames.Contains(tmp))
                {
                    effectNames.Add(tmp);
                }
            }
        }

        if (damageNum > 0)
        {
            effects += damageNum.ToString() + " Damage";
        }

        if (healingNum > 0)
        {
            effects += healingNum.ToString() + " Healing";
        }

        for (int i = 0; i < effectNames.Count; ++i)
        {
            if (effects != "")
            {
                effects += ", ";
            }

            effects += "applies " + effectNames[i];
        }

        if (abil.moveCharacter)
        {
            if (effects != "")
            {
                effects += ", ";
            }

            effects += "moves character";
        }

        if (abil.ranged)
        {
            if (effects != "")
            {
                effects += ", ";
            }

            effects += "ranged";
        }

        result = num.ToString() + " • " + abil.name + "\n" + effects;// + "\n" + abil.description;
        return result;
    }
}

public class CharacterAbilityUI
{
    public string abil1;
    public string abil2;
    public string abil3;
    public string abil4;
}
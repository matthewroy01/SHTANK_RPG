using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AbilityUI : MonoBehaviour
{
    public Button buttonSelectAbility;
    public Image imageOverlay;
    public TextMeshProUGUI textMeshAbilityName;
    public TextMeshProUGUI textMeshDetails;
    public TextMeshProUGUI textMeshDescription;

    public RectTransform refRectTransform;

    public void SetActive(bool val)
    {
        buttonSelectAbility.gameObject.SetActive(val);
        textMeshAbilityName.gameObject.SetActive(val);
        textMeshDetails.gameObject.SetActive(val);
        textMeshDescription.gameObject.SetActive(val);
    }
}

[System.Serializable]
public class AbilityUIDefinition
{
    public AbilityUIStrings strings1;
    public AbilityUIStrings strings2;
    public AbilityUIStrings strings3;
    public AbilityUIStrings strings4;

    public AbilityUIDefinition()
    {
        strings1 = new AbilityUIStrings();
        strings2 = new AbilityUIStrings();
        strings3 = new AbilityUIStrings();
        strings4 = new AbilityUIStrings();
    }

    public void SetDefinitions(Moveset moveset)
    {
        if (moveset != null)
        {
            if (moveset.ability1 != null)
            {
                strings1.name = moveset.ability1.name;
                strings1.details = SetDetails(moveset.ability1);
                strings1.description = moveset.ability1.description;
            }

            if (moveset.ability2 != null)
            {
                strings2.name = moveset.ability2.name;
                strings2.details = SetDetails(moveset.ability2);
                strings2.description = moveset.ability2.description;
            }

            if (moveset.ability3 != null)
            {
                strings3.name = moveset.ability3.name;
                strings3.details = SetDetails(moveset.ability3);
                strings3.description = moveset.ability3.description;
            }

            if (moveset.ability4 != null)
            {
                strings4.name = moveset.ability4.name;
                strings4.details = SetDetails(moveset.ability4);
                strings4.description = moveset.ability4.description;
            }
        }
    }

    private string SetDetails(Ability abil)
    {
        int damageNum = 0;
        int healingNum = 0;
        int aggroNum = 0;
        List<Effect_ID> effectNames = new List<Effect_ID>();
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
            else if (abil.effects[i].id == Effect_ID.aggro)
            {
                aggroNum += abil.effects[i].value;
            }
            else
            {
                if (!effectNames.Contains(abil.effects[i].id))
                {
                    effectNames.Add(abil.effects[i].id);
                }
            }
        }

        if (damageNum > 0)
        {
            effects += "<color=#ff0000>" + damageNum.ToString() + " Damage" + "</color>";
        }

        if (healingNum > 0)
        {
            if (effects != "")
            {
                effects += "/";
            }

            effects += "<color=#00ff00>" + healingNum.ToString() + " Healing" + "</color>";
        }

        if (aggroNum > 0)
        {
            if (effects != "")
            {
                effects += "/";
            }

            effects += "<color=#ff0000>" + aggroNum.ToString() + " Aggro" + "</color>";
        }

        for (int i = 0; i < effectNames.Count; ++i)
        {
            if (effects != "")
            {
                effects += "/";
            }

            effects += GetEffectText(effectNames[i]);
        }

        if (abil.moveCharacter)
        {
            if (effects != "")
            {
                effects += "/";
            }

            effects += "Movement";
        }

        if (abil.ranged)
        {
            if (effects != "")
            {
                effects += "/";
            }

            effects += "Ranged";
        }

        if (abil.ignoreAttackMod)
        {
            if (effects != "")
            {
                effects += "/";
            }

            effects += "Ignores Attack";
        }

        if (abil.ignoreDefenseMod)
        {
            if (effects != "")
            {
                effects += "/";
            }

            effects += "Ignores Defense";
        }

        if (abil.effects.Count > 0 && abil.effects[0].probability < 1.0f)
        {
            if (effects != "")
            {
                effects += "/";
            }

            effects += "Can Miss";
        }

        return "<b>" + effects + "</b>";
    }

    private string GetEffectText(Effect_ID id)
    {
        switch(id)
        {
            case(Effect_ID.frosty):
            {
                return "<color=#00ffff>" + "Frosty" + "</color>";
            }
            case(Effect_ID.aggroDispel):
            {
                return "<color=#7F7F7F>" + "Dispels Aggro" + "</color>";
            }
            case (Effect_ID.shadowWall):
            {
                return "<color=#3C003C>" + "Shadow Wall" + "</color>";
            }
            case (Effect_ID.toasty):
            {
                return "<color=#FF7F00>" + "Toasty" + "</color>";
            }
            default:
            {
                return Enum.GetName(typeof(Effect_ID), id);
            }
        }
    }
}

public class AbilityUIStrings
{
    public string name;
    public string details;
    public string description;

    public AbilityUIStrings()
    {
        name = "";
        details = "";
        description = "";
    }
}
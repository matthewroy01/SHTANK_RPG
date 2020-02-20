using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

public class CharacterUI : MonoBehaviour
{
    private Character currentCharacter;

    [Header("Stats")]
    public Image imagePortrait;
    public TextMeshProUGUI textMeshCharacterName;
    public TextMeshProUGUI textMeshHP;
    public TextMeshProUGUI textMeshAttackModifier;
    public TextMeshProUGUI textMeshDefenseModifier;
    public TextMeshProUGUI textMeshMovement;
    public TextMeshProUGUI textMeshNashbalm;

    [Header("Abilities")]
    public AbilityUI abilityUI1;
    public AbilityUI abilityUI2;
    public AbilityUI abilityUI3;
    public AbilityUI abilityUI4;

    public void UpdateCharacterUI(Character character)
    {
        // character portrait, name, and HP
        imagePortrait.sprite = character.portrait;
        textMeshCharacterName.text = character.name;
        textMeshHP.text = "HP " + character.healthCurrent + "/" + character.healthMax;

        // decide whether or not to use '+' or '-' when describing attack and defense modifiers
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

        // attack and defense modifiers, movement, and nashbalm
        textMeshAttackModifier.text = "Atk " + plusOrMinusAtk + character.attackMod;
        textMeshDefenseModifier.text = "Def " + plusOrMinusDef + character.defenseMod;
        textMeshMovement.text = "Mov " + character.movementRangeCurrent;
        textMeshNashbalm.text = "NB " + character.nashbalm + "%";

        // ability text
        abilityUI1.SetActive(character.abilityUIDefinition.strings1.name != "");
        Debug.Log("Ability name is " + character.abilityUIDefinition.strings1.name);
        UpdateAbilityUI(abilityUI1, character.abilityUIDefinition.strings1);

        abilityUI2.SetActive(character.abilityUIDefinition.strings2.name != "");
        Debug.Log("Ability name is " + character.abilityUIDefinition.strings2.name);
        UpdateAbilityUI(abilityUI2, character.abilityUIDefinition.strings2);

        abilityUI3.SetActive(character.abilityUIDefinition.strings3.name != "");
        Debug.Log("Ability name is " + character.abilityUIDefinition.strings3.name);
        UpdateAbilityUI(abilityUI3, character.abilityUIDefinition.strings3);

        abilityUI4.SetActive(character.abilityUIDefinition.strings4.name != "");
        Debug.Log("Ability name is " + character.abilityUIDefinition.strings4.name);
        UpdateAbilityUI(abilityUI4, character.abilityUIDefinition.strings4);
    }

    private void UpdateAbilityUI(AbilityUI ui, AbilityUIStrings strings)
    {
        ui.textMeshAbilityName.text = strings.name;
        ui.textMeshDetails.text = strings.details;
    }

    public AbilityUIDefinition InitializeAbilityUI(PlayerBase character)
    {
        AbilityUIDefinition result = new AbilityUIDefinition();

        if (character != null)
        {
            Moveset moveset = character.moveset;

            if (moveset != null)
            {
                if (moveset.ability1 != null)
                {
                    result.strings1.name = moveset.ability1.name;
                    result.strings1.details = CreateAbilityUI(moveset.ability1);
                }

                if (moveset.ability2 != null)
                {
                    result.strings2.name = moveset.ability2.name;
                    result.strings2.details = CreateAbilityUI(moveset.ability2);
                }

                if (moveset.ability3 != null)
                {
                    result.strings3.name = moveset.ability3.name;
                    result.strings3.details = CreateAbilityUI(moveset.ability3);
                }

                if (moveset.ability4 != null)
                {
                    result.strings4.name = moveset.ability4.name;
                    result.strings4.details = CreateAbilityUI(moveset.ability4);
                }
            }
        }

        return result;
    }

    private string CreateAbilityUI(Ability abil)
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

        result = effects + "\n" + abil.description;
        return result;
    }
}

[System.Serializable]
public class AbilityUI
{
    public Button buttonSelectAbility;
    public TextMeshProUGUI textMeshAbilityName;
    public TextMeshProUGUI textMeshDetails;

    public void SetActive(bool val)
    {
        buttonSelectAbility.gameObject.SetActive(val);
        textMeshAbilityName.gameObject.SetActive(val);
        textMeshDetails.gameObject.SetActive(val);
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
}

public class AbilityUIStrings
{
    public string name;
    public string details;

    public AbilityUIStrings()
    {
        name = "";
        details = "";
    }
}
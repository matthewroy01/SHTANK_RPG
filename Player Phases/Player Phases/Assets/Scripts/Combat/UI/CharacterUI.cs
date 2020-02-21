﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

public class CharacterUI : MonoBehaviour
{
    private Character currentCharacter;

    public GameObject parentToToggle;

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
    private AbilityUI abilityUI2;
    private AbilityUI abilityUI3;
    private AbilityUI abilityUI4;

    private void Start()
    {
        CharacterSelector refCharacterSelector = FindObjectOfType<CharacterSelector>();

        abilityUI1.buttonSelectAbility.onClick.AddListener(delegate { refCharacterSelector.SelectAbility(1); });

        abilityUI2 = Instantiate(abilityUI1.gameObject, abilityUI1.transform.parent).GetComponent<AbilityUI>();
        abilityUI2.name = "Ability 2";

        abilityUI2.buttonSelectAbility.onClick.AddListener(delegate { refCharacterSelector.SelectAbility(2); });

        abilityUI3 = Instantiate(abilityUI2.gameObject, abilityUI2.transform.parent).GetComponent<AbilityUI>();
        abilityUI3.name = "Ability 3";

        abilityUI3.buttonSelectAbility.onClick.AddListener(delegate { refCharacterSelector.SelectAbility(3); });

        abilityUI4 = Instantiate(abilityUI3.gameObject, abilityUI2.transform.parent).GetComponent<AbilityUI>();
        abilityUI4.name = "Ability 4";

        abilityUI4.buttonSelectAbility.onClick.AddListener(delegate { refCharacterSelector.SelectAbility(4); });
    }

    public void ToggleUI(bool val)
    {
        parentToToggle.SetActive(val);
    }

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
        ui.textMeshDescription.text = strings.description;
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
                    result.strings1.description = moveset.ability1.description;
                }

                if (moveset.ability2 != null)
                {
                    result.strings2.name = moveset.ability2.name;
                    result.strings2.details = CreateAbilityUI(moveset.ability2);
                    result.strings2.description = moveset.ability2.description;
                }

                if (moveset.ability3 != null)
                {
                    result.strings3.name = moveset.ability3.name;
                    result.strings3.details = CreateAbilityUI(moveset.ability3);
                    result.strings3.description = moveset.ability3.description;
                }

                if (moveset.ability4 != null)
                {
                    result.strings4.name = moveset.ability4.name;
                    result.strings4.details = CreateAbilityUI(moveset.ability4);
                    result.strings4.description = moveset.ability4.description;
                }
            }
        }

        return result;
    }

    private string CreateAbilityUI(Ability abil)
    {
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

        return effects;
    }
}
using System.Collections;
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
    public Color overlaySelectedColor;
    public Color overlayUnselectedColor;
    private AbilityUI abilityUI2;
    private AbilityUI abilityUI3;
    private AbilityUI abilityUI4;

    [Header("Statuses")]
    public Image defaultStatus;
    public int maxStatuses;
    public float pixelsBetweenStatuses;
    private List<Image> imageListStatus = new List<Image>();

    [Header("Status Definitions")]
    public StatusUIDefinition statusDefFrosty;
    public StatusUIDefinition statusDefToasty;
    public StatusUIDefinition statusDefHoneyed;

    private void Start()
    {
        CharacterSelector refCharacterSelector = FindObjectOfType<CharacterSelector>();

        // set up Ability 1's UI
        abilityUI1.buttonSelectAbility.onClick.AddListener(delegate { refCharacterSelector.AbilitySelect(1); });

        // set up Ability 2's UI
        abilityUI2 = Instantiate(abilityUI1.gameObject, abilityUI1.transform.parent).GetComponent<AbilityUI>();
        abilityUI2.name = "Ability 2";

        abilityUI2.buttonSelectAbility.onClick.AddListener(delegate { refCharacterSelector.AbilitySelect(2); });

        // set up Ability 3's UI
        abilityUI3 = Instantiate(abilityUI2.gameObject, abilityUI2.transform.parent).GetComponent<AbilityUI>();
        abilityUI3.name = "Ability 3";

        abilityUI3.buttonSelectAbility.onClick.AddListener(delegate { refCharacterSelector.AbilitySelect(3); });

        // set up Ability 4's UI
        abilityUI4 = Instantiate(abilityUI3.gameObject, abilityUI2.transform.parent).GetComponent<AbilityUI>();
        abilityUI4.name = "Ability 4";

        abilityUI4.buttonSelectAbility.onClick.AddListener(delegate { refCharacterSelector.AbilitySelect(4); });

        // set up status icons
        imageListStatus.Add(defaultStatus);

        for (int i = 1; i < maxStatuses; ++i)
        {
            imageListStatus.Add(Instantiate(defaultStatus, defaultStatus.transform.parent));
            RectTransform tmp = imageListStatus[imageListStatus.Count - 1].rectTransform;
            tmp.name = "Status " + (i + 1).ToString();
            tmp.localPosition = new Vector2(tmp.localPosition.x + (pixelsBetweenStatuses * i), tmp.localPosition.y);
        }
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
        UpdateAbilityUI(abilityUI1, character.abilityUIDefinition.strings1);

        abilityUI2.SetActive(character.abilityUIDefinition.strings2.name != "");
        UpdateAbilityUI(abilityUI2, character.abilityUIDefinition.strings2);

        abilityUI3.SetActive(character.abilityUIDefinition.strings3.name != "");
        UpdateAbilityUI(abilityUI3, character.abilityUIDefinition.strings3);

        abilityUI4.SetActive(character.abilityUIDefinition.strings4.name != "");
        UpdateAbilityUI(abilityUI4, character.abilityUIDefinition.strings4);

        // statuses
        UpdateStatusUI(character);
    }

    private void UpdateAbilityUI(AbilityUI ui, AbilityUIStrings strings)
    {
        ui.textMeshAbilityName.text = strings.name;
        ui.textMeshDetails.text = strings.details;
        ui.textMeshDescription.text = strings.description;
    }

    public void SetSelectedAbilityColor(int selected)
    {
        switch(selected)
        {
            case 1:
            {
                // ability 1 selected
                abilityUI1.imageOverlay.color = overlaySelectedColor;// CrossFadeColor(overlaySelectedColor, 0.25f, true, true);

                abilityUI2.imageOverlay.color = overlayUnselectedColor;// CrossFadeColor(overlayUnselectedColor, 0.25f, true, true);
                abilityUI3.imageOverlay.color = overlayUnselectedColor;// CrossFadeColor(overlayUnselectedColor, 0.25f, true, true);
                abilityUI4.imageOverlay.color = overlayUnselectedColor;// CrossFadeColor(overlayUnselectedColor, 0.25f, true, true);

                break;
            }
            case 2:
            {
                // ability 2 selected
                abilityUI2.imageOverlay.color = overlaySelectedColor;// CrossFadeColor(overlaySelectedColor, 0.25f, true, true);

                abilityUI1.imageOverlay.color = overlayUnselectedColor;// CrossFadeColor(overlayUnselectedColor, 0.25f, true, true);
                abilityUI3.imageOverlay.color = overlayUnselectedColor;// CrossFadeColor(overlayUnselectedColor, 0.25f, true, true);
                abilityUI4.imageOverlay.color = overlayUnselectedColor;// CrossFadeColor(overlayUnselectedColor, 0.25f, true, true);

                break;
            }
            case 3:
            {
                // ability 3 selected
                abilityUI3.imageOverlay.color = overlaySelectedColor;// CrossFadeColor(overlaySelectedColor, 0.25f, true, true);

                abilityUI1.imageOverlay.color = overlayUnselectedColor;// CrossFadeColor(overlayUnselectedColor, 0.25f, true, true);
                abilityUI2.imageOverlay.color = overlayUnselectedColor;// CrossFadeColor(overlayUnselectedColor, 0.25f, true, true);
                abilityUI4.imageOverlay.color = overlayUnselectedColor;// CrossFadeColor(overlayUnselectedColor, 0.25f, true, true);

                break;
            }
            case 4:
            {
                // ability 4 selected
                abilityUI4.imageOverlay.color = overlaySelectedColor;// CrossFadeColor(overlaySelectedColor, 0.25f, true, true);

                abilityUI1.imageOverlay.color = overlayUnselectedColor;// CrossFadeColor(overlayUnselectedColor, 0.25f, true, true);
                abilityUI2.imageOverlay.color = overlayUnselectedColor;// CrossFadeColor(overlayUnselectedColor, 0.25f, true, true);
                abilityUI3.imageOverlay.color = overlayUnselectedColor;// CrossFadeColor(overlayUnselectedColor, 0.25f, true, true);

                break;
            }
            default:
            {
                // no ability selected
                abilityUI1.imageOverlay.color = overlayUnselectedColor;// CrossFadeColor(overlayUnselectedColor, 0.25f, true, true);
                abilityUI2.imageOverlay.color = overlayUnselectedColor;// CrossFadeColor(overlayUnselectedColor, 0.25f, true, true);
                abilityUI3.imageOverlay.color = overlayUnselectedColor;// CrossFadeColor(overlayUnselectedColor, 0.25f, true, true);
                abilityUI4.imageOverlay.color = overlayUnselectedColor;// CrossFadeColor(overlayUnselectedColor, 0.25f, true, true);

                break;
            }
        }
    }

    public AbilityUIDefinition InitializeAbilityUI(Character character)
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

    private void UpdateStatusUI(Character character)
    {
        List<StatusUIDefinition> definitions = new List<StatusUIDefinition>();

        // get statuses from passives first

        // process universal statuses
        if (character.statusFrosty)
        {
            definitions.Add(statusDefFrosty);
        }
        if (character.statusToasty)
        {
            definitions.Add(statusDefToasty);
        }
        if (character.statusHoneyed)
        {
            definitions.Add(statusDefHoneyed);
        }

        // assign status sprites
        for (int i = 0; i < imageListStatus.Count; ++i)
        {
            if (i < definitions.Count)
            {
                imageListStatus[i].sprite = definitions[i].sprite;
                imageListStatus[i].gameObject.SetActive(true);
            }
            else
            {
                imageListStatus[i].sprite = null;
                imageListStatus[i].gameObject.SetActive(false);
            }
        }
    }
}
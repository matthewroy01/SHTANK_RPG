using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.UI;

using DG.Tweening;

public class CharacterUI : MonoBehaviour
{
    private Character currentCharacter;

    public GameObject parentToToggle;

    private bool active;
    private Tweener activeTweener;

    [Header("Stats")]
    public Image imagePortrait;
    public TextMeshProUGUI textMeshCharacterName;
    public TextMeshProUGUI textMeshHP;
    public TextMeshProUGUI textMeshAttack;
    public TextMeshProUGUI textMeshDefense;
    public TextMeshProUGUI textMeshStagger;
    public TextMeshProUGUI textMeshNashbalm;
    public TextMeshProUGUI textMeshMovement;

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

    [Header("Aggro Target")]
    public TextMeshProUGUI textMeshAggroTarget;
    public Image aggroTargetPortrait;

    [Header("Status Definitions")]
    public StatusUIDefinition statusDefStunned;
    public StatusUIDefinition statusDefFrosty;
    public StatusUIDefinition statusDefToasty;
    public StatusUIDefinition statusDefHoneyed;

    private void Start()
    {
        CharacterSelector refCharacterSelector = FindObjectOfType<CharacterSelector>();

        // set up Ability 1's UI
        /*abilityUI1.buttonSelectAbility.onClick.AddListener(delegate { refCharacterSelector.AbilitySelect(1); });

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

        abilityUI4.buttonSelectAbility.onClick.AddListener(delegate { refCharacterSelector.AbilitySelect(4); });*/

        // set up status icons
        imageListStatus.Add(defaultStatus);

        for (int i = 1; i < maxStatuses; ++i)
        {
            imageListStatus.Add(Instantiate(defaultStatus, defaultStatus.transform.parent));
            RectTransform tmp = imageListStatus[imageListStatus.Count - 1].rectTransform;
            tmp.name = "Status " + (i + 1).ToString();
            tmp.localPosition = new Vector2(tmp.localPosition.x + (pixelsBetweenStatuses * i), tmp.localPosition.y);
            tmp.GetComponent<MaskableGraphic>().enabled = false;
            //tmp.gameObject.SetActive(false);
        }

        // slide off the screen to our default position upon starting
        parentToToggle.transform.DOLocalMoveX(-1000, 0.0f, false);
    }

    public void ToggleUI(bool val)
    {
        if (active == true && val == false)
        {
            activeTweener.Kill();
            activeTweener = parentToToggle.transform.DOLocalMoveX(-1000, 0.15f, false);
        }

        if (active == false && val == true)
        {
            activeTweener.Kill();
            activeTweener = parentToToggle.transform.DOLocalMoveX(0, 0.25f, false);
        }

        active = val;
    }

    public void UpdateCharacterUI(Character character)
    {
        // character portrait, name, and HP
        imagePortrait.sprite = character.portrait;
        TooltipDetector tmp1 = null, tmp2 = null;
        if (imagePortrait.TryGetComponent(out tmp1))
        {
            tmp1.tooltipOverride = character.characterDescription;
        }
        textMeshCharacterName.text = character.name;
        if (textMeshCharacterName.TryGetComponent(out tmp2))
        {
            tmp2.tooltipOverride = character.characterDescription;
        }
        textMeshHP.text = "HP " + character.healthCurrent + "/" + character.healthMax;

        // attack and defense modifiers, movement, and nashbalm
        textMeshAttack.text = "Atk " + character.attack;
        textMeshDefense.text = "Def " + character.defense;
        textMeshMovement.text = "Mov " + character.movementRangeCurrent;
        textMeshStagger.text = "St " + character.stagger + "%";
        textMeshNashbalm.text = "NB " + character.nashbalm + "%";

        // ability text
        /*if (character.movesetData.unlocked > 0)
        {
            abilityUI1.SetActive(character.abilityUIDefinition.strings1.name != "");
            UpdateAbilityUI(abilityUI1, character.abilityUIDefinition.strings1);
        }
        else
        {
            abilityUI1.SetActive(false);
        }

        if (character.movesetData.unlocked > 1)
        {
            abilityUI2.SetActive(character.abilityUIDefinition.strings2.name != "");
            UpdateAbilityUI(abilityUI2, character.abilityUIDefinition.strings2);
        }
        else
        {
            abilityUI2.SetActive(false);
        }

        if (character.movesetData.unlocked > 2)
        {
            abilityUI3.SetActive(character.abilityUIDefinition.strings3.name != "");
            UpdateAbilityUI(abilityUI3, character.abilityUIDefinition.strings3);
        }
        else
        {
            abilityUI3.SetActive(false);
        }

        if (character.movesetData.unlocked > 3)
        {
            abilityUI4.SetActive(character.abilityUIDefinition.strings4.name != "");
            UpdateAbilityUI(abilityUI4, character.abilityUIDefinition.strings4);
        }
        else
        {
            abilityUI4.SetActive(false);
        }*/

        // statuses
        UpdateStatusUI(character);

        // aggro target
        Character enemy = null;
        character.TryGetComponent(out enemy);

        if (enemy != null)
        {
            textMeshAggroTarget.enabled = true;
            GridSpace tmp = enemy.ProcessAggro();
            if (tmp != character.myGridSpace)
            {
                aggroTargetPortrait.sprite = tmp.character.portrait;
                aggroTargetPortrait.enabled = true;
            }
            else
            {
                textMeshAggroTarget.enabled = false;
                aggroTargetPortrait.enabled = false;
            }
        }
        else
        {
            textMeshAggroTarget.enabled = false;
            aggroTargetPortrait.enabled = false;
        }
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
        int aggroNum = 0;
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
            else if (abil.effects[i].id == Effect_ID.aggro)
            {
                aggroNum += abil.effects[i].value;
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
            if (effects != "")
            {
                effects += ", ";
            }

            effects += healingNum.ToString() + " Healing";
        }

        if (aggroNum > 0)
        {
            if (effects != "")
            {
                effects += ", ";
            }

            effects += aggroNum.ToString() + " Aggro";
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

        if (abil.ignoreAttackMod)
        {
            if (effects != "")
            {
                effects += ", ";
            }

            effects += "ignores Atk Mod";
        }

        if (abil.ignoreDefenseMod)
        {
            if (effects != "")
            {
                effects += ", ";
            }

            effects += "ignores Def Mod";
        }

        if (abil.effects.Count > 0 && abil.effects[0].probability < 1.0f)
        {
            if (effects != "")
            {
                effects += ", ";
            }

            effects += "can miss";
        }

        return effects;
    }

    private void UpdateStatusUI(Character character)
    {
        List<StatusUIDefinition> definitions = new List<StatusUIDefinition>();

        // get statuses from passives first
        if (character.passive != null)
        {
            definitions.AddRange(character.passive.GetActiveStatuses());
        }

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
        if (character.statusStunned)
        {
            definitions.Add(statusDefStunned);
        }

        // assign status sprites
        for (int i = 0; i < imageListStatus.Count; ++i)
        {
            if (i < definitions.Count)
            {
                imageListStatus[i].sprite = definitions[i].sprite;
                TooltipDetector tmp;
                if (imageListStatus[i].TryGetComponent(out tmp))
                {
                    tmp.statusDef = definitions[i];
                }
                imageListStatus[i].GetComponent<MaskableGraphic>().enabled = true;
                //imageListStatus[i].gameObject.SetActive(true);
            }
            else
            {
                imageListStatus[i].sprite = null;
                imageListStatus[i].GetComponent<MaskableGraphic>().enabled = false;
                //imageListStatus[i].gameObject.SetActive(false);
            }
        }
    }

    public bool GetMouseIsntOverButton()
    {
        Vector2 mousePos = Input.mousePosition;

        if (CheckMouseOver(mousePos, abilityUI1))
        {
            return false;
        }

        if (CheckMouseOver(mousePos, abilityUI2))
        {
            return false;
        }

        if (CheckMouseOver(mousePos, abilityUI3))
        {
            return false;
        }

        if (CheckMouseOver(mousePos, abilityUI4))
        {
            return false;
        }

        return true;
    }

    private bool CheckMouseOver(Vector2 mousePos, AbilityUI abilityUI)
    {
        if (abilityUI != null && abilityUI.gameObject.activeInHierarchy)
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(abilityUI.imageOverlay.rectTransform, mousePos))
            {
                return true;
            }
        }

        return false;
    }
}
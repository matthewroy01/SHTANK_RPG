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

        // use "CheckMouseOver" function here

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
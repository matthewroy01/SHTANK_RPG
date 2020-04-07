using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ContextSensitiveUI : MonoBehaviour
{
    [Header("UI Elements")]
    public ContextSensitiveUIElements spacebar;
    public ContextSensitiveUIElements leftShift;
    public ContextSensitiveUIElements clickLeft;
    public ContextSensitiveUIElements clickRight;
    public ContextSensitiveUIElements pointer;

    [Header("Behavior")]
    [Range(0.0f, 1.0f)]
    public float alphaDisabled;
    private const float alphaEnabled = 1.0f;
    public float alphaFadeOutDuration;
    public float alphaFadeInDuration;

    public void UpdateContextSensitiveUI(int state, bool rangedAbility)
    {
        switch(state)
        {
            case (int)CharacterSelector.SelectorState.doingNothing:
            {
                spacebar.SetValues("", alphaDisabled, alphaFadeOutDuration);
                leftShift.SetValues("", alphaDisabled, alphaFadeOutDuration);
                clickLeft.SetValues("Select", alphaEnabled, alphaFadeInDuration);
                clickRight.SetValues("", alphaDisabled, alphaFadeOutDuration);
                pointer.SetValues("", alphaDisabled, alphaFadeOutDuration);

                break;
            }
            case (int)CharacterSelector.SelectorState.playerSelected:
            {
                spacebar.SetValues("Confirm", alphaEnabled, alphaFadeInDuration);
                leftShift.SetValues("", alphaDisabled, alphaFadeOutDuration);
                clickLeft.SetValues("Move", alphaEnabled, alphaFadeInDuration);
                clickRight.SetValues("Deselect", alphaEnabled, alphaFadeInDuration);
                pointer.SetValues("", alphaDisabled, alphaFadeOutDuration);

                break;
            }
            case (int)CharacterSelector.SelectorState.enemySelected:
            {
                break;
            }
            case (int)CharacterSelector.SelectorState.playerSelectedWithMovement:
            {
                spacebar.SetValues("Confirm", alphaEnabled, alphaFadeInDuration);
                leftShift.SetValues("", alphaDisabled, alphaFadeOutDuration);
                clickLeft.SetValues("Move", alphaEnabled, alphaFadeInDuration);
                clickRight.SetValues("Deselect", alphaEnabled, alphaFadeInDuration);
                pointer.SetValues("", alphaDisabled, alphaFadeOutDuration);

                break;
            }
            case (int)CharacterSelector.SelectorState.playerSelectedWithAbility:
            {
                spacebar.SetValues("Confirm", alphaEnabled, alphaFadeInDuration);
                leftShift.SetValues("Flip", alphaEnabled, alphaFadeInDuration);
                if (rangedAbility)
                {
                    clickLeft.SetValues("Aim", alphaEnabled, alphaFadeInDuration);
                }
                else
                {
                    clickLeft.SetValues("", alphaDisabled, alphaFadeOutDuration);
                }
                clickRight.SetValues("Deselect", alphaEnabled, alphaFadeInDuration);
                pointer.SetValues("Rotate", alphaEnabled, alphaFadeInDuration);

                break;
            }
        }
    }

    [System.Serializable]
    public class ContextSensitiveUIElements
    {
        public TextMeshProUGUI text;
        public Image image;

        public void SetValues(string newText, float targetAlpha, float fadeDuration)
        {
            text.text = newText;
            image.CrossFadeAlpha(targetAlpha, fadeDuration, false);
        }
    }
}
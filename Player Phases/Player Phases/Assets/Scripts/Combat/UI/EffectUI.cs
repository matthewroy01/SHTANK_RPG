using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EffectUI : MonoBehaviour
{
    [Header("Health bar")]
    public Slider healthBar;
    public TextMeshProUGUI healthNumber;

    [Header("Effect Text")]
    public TextMeshProUGUI effectText;
    public UIEffect uiEffectPop;
    public UIEffect uiEffectShake;

    [Header("Colors and Sounds for Effects")]
    public EffectUIColorAndSound damage;
    public EffectUIColorAndSound noDamage;
    public EffectUIColorAndSound miss;
    public EffectUIColorAndSound healing;
    public EffectUIColorAndSound frosty;
    public EffectUIColorAndSound aggro;
    public EffectUIColorAndSound dispelAggro;
    public EffectUIColorAndSound attackUp;

    [Header("Text for other custom effects")]
    private Color customEffectColor;
    private ManagedAudio customEffectAudio;
    private string customEffectString;

    [Header("Timing")]
    public float timeBetweenEffects;

    private Queue<Effect> toDisplay = new Queue<Effect>();

    private UtilityAudioManager refAudioManager;
    private Character owner;

    private void Start()
    {
        refAudioManager = FindObjectOfType<UtilityAudioManager>();
        owner = GetComponent<Character>();

        StartCoroutine(IterateThroughEffects());
    }

    private void Update()
    {
        healthBar.value = (float)owner.healthCurrent / (float)owner.healthMax;
        healthNumber.text = owner.healthCurrent.ToString();
    }

    public void AddEffect(Effect effect)
    {
        toDisplay.Enqueue(effect);
    }

    public void AddEffectCustom(string customText, ManagedAudio customSound, Color customColor)
    {
        customEffectString = customText;

        customEffectAudio = customSound;
        customEffectColor = customColor;

        toDisplay.Enqueue(new Effect(Effect_ID.custom, 1));
    }

    private IEnumerator IterateThroughEffects()
    {
        while(true)
        {
            // keep checking if there are any effects to display, and then call them to be displayed
            while (toDisplay.Count > 0)
            {
                DisplayEffect(toDisplay.Dequeue());

                // wait in between effects so they don't overlap
                yield return new WaitForSeconds(timeBetweenEffects);
            }

            yield return new WaitForEndOfFrame();
        }
    }

    private void DisplayEffect(Effect effect)
    {
        switch(effect.id)
        {
            case Effect_ID.damage:
            {
                ApplyEffect(effect.value.ToString(), damage.color, damage.audio, uiEffectShake);
                break;
            }
            case Effect_ID.noDamage:
            {
                ApplyEffect(effect.value.ToString(), noDamage.color, noDamage.audio, uiEffectShake);
                break;
            }
            case Effect_ID.miss:
            {
                ApplyEffect("Miss", miss.color, miss.audio, uiEffectShake);
                break;
            }
            case Effect_ID.healing:
            {
                ApplyEffect(effect.value.ToString(), healing.color, healing.audio, uiEffectPop);
                break;
            }
            case Effect_ID.aggro:
            {
                if (effect.value > 0)
                {
                    ApplyEffect("Aggro+", aggro.color, aggro.audio, uiEffectPop);
                }
                else
                {
                    ApplyEffect("Aggro-", aggro.color, aggro.audio, uiEffectPop);
                }
                break;
            }
            case Effect_ID.frosty:
            {
                ApplyEffect("Frosty!", frosty.color, frosty.audio, uiEffectShake);
                break;
            }
            case Effect_ID.aggroDispel:
            {
                ApplyEffect("Aggro Dispelled!", dispelAggro.color, dispelAggro.audio, uiEffectPop);
                break;
            }
            case Effect_ID.attackUp:
            {
                ApplyEffect("Attack Up!", attackUp.color, attackUp.audio, uiEffectPop);
                break;
            }
            default:
            {
                // custom effects
                ApplyEffect(customEffectString, customEffectColor, customEffectAudio, uiEffectPop);
                break;
            }
        }

        Invoke("Reset", timeBetweenEffects);
    }

    private void ApplyEffect(string text, Color color, ManagedAudio audio, UIEffect uiEffect)
    {
        if (text != null)
        {
            effectText.color = color;
            effectText.text = text;
        }

        if (audio != null && refAudioManager != null)
        {
            refAudioManager.QueueSound(audio);
        }

        if (uiEffect != null)
        {
            uiEffect.DoEffect();
        }
    }

    private void Reset()
    {
        effectText.text = "";
    }
}

[System.Serializable]
public class EffectUIParameters
{
    public TextMeshProUGUI ui;
    public Color color;
    public ManagedAudio sound;
    public UIEffect effect;

    public void Apply(string text, UtilityAudioManager source)
    {
        if (ui != null)
        {
            ui.color = color;
            ui.text = text;
        }

        if (sound != null)
        {
            source.QueueSound(sound);
        }

        if (effect != null)
        {
            effect.DoEffect();
        }
    }

    public void Clear()
    {
        if (ui != null)
        {
            ui.text = "";
        }
    }
}

[System.Serializable]
public class EffectUIColorAndSound
{
    public Color color;
    public ManagedAudio audio;
}
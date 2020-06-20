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

    [Header("Text for damage and healing")]
    public EffectUIParameters damage;
    public EffectUIParameters noDamage;
    public EffectUIParameters miss;
    public EffectUIParameters healing;
    public EffectUIParameters frost;
    public EffectUIParameters aggro;
    public EffectUIParameters dispelAggro;
    public EffectUIParameters attackUp;

    [Header("Text for other custom effects")]
    public EffectUIParameters customEffect;
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

        customEffect.sound = customSound;
        customEffect.color = customColor;

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
                damage.Apply(effect.value.ToString(), refAudioManager);
                break;
            }
            case Effect_ID.noDamage:
            {
                noDamage.Apply(effect.value.ToString(), refAudioManager);
                break;
            }
            case Effect_ID.miss:
            {
                miss.Apply("Miss", refAudioManager);
                break;
            }
            case Effect_ID.healing:
            {
                healing.Apply(effect.value.ToString(), refAudioManager);
                break;
            }
            case Effect_ID.aggro:
            {
                if (effect.value > 0)
                {
                    aggro.Apply("Aggro+", refAudioManager);
                }
                else
                {
                    aggro.Apply("Aggro-", refAudioManager);
                }
                break;
            }
            case Effect_ID.frosty:
            {
                frost.Apply("Frosty!", refAudioManager);
                break;
            }
            case Effect_ID.aggroDispel:
            {
                dispelAggro.Apply("Aggro Dispelled!", refAudioManager);
                break;
            }
            case Effect_ID.attackUp:
            {
                attackUp.Apply("Attack Up!", refAudioManager);
                break;
            }
            default:
            {
                // custom effects
                customEffect.Apply(customEffectString, refAudioManager);
                break;
            }
        }

        Invoke("Reset", timeBetweenEffects);
    }

    private void Reset()
    {
        damage.Clear();
        healing.Clear();
        frost.Clear();
        aggro.Clear();
        dispelAggro.Clear();
        attackUp.Clear();

        customEffect.Clear();
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
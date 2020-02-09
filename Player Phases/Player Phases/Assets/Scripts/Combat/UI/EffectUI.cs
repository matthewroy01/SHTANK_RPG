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
    public EffectUIParameters healing;
    public EffectUIParameters frost;
    public EffectUIParameters aggro;
    public EffectUIParameters dispelAggro;
    public EffectUIParameters attackUp;

    [Header("Timing")]
    public float timeBetweenEffects;

    private Queue<Effect> toDisplay = new Queue<Effect>();

    private AudioSource refAudioSource;
    private Character owner;

    private void Start()
    {
        refAudioSource = GetComponent<AudioSource>();
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
                damage.Apply(effect.value.ToString(), refAudioSource);
                break;
            }
            case Effect_ID.healing:
            {
                healing.Apply(effect.value.ToString(), refAudioSource);
                break;
            }
            case Effect_ID.aggro:
            {
                if (effect.value > 0)
                {
                    aggro.Apply("Aggro+", refAudioSource);
                }
                else
                {
                    aggro.Apply("Aggro-", refAudioSource);
                }
                break;
            }
            case Effect_ID.frosty:
            {
                frost.Apply("Frosty!", refAudioSource);
                break;
            }
            case Effect_ID.aggroDispel:
            {
                dispelAggro.Apply("Aggro Dispelled!", refAudioSource);
                break;
            }
            case Effect_ID.attackUp:
            {
                attackUp.Apply("Attack Up!", refAudioSource);
                break;
            }
            default:
            {
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
    }
}

[System.Serializable]
public class EffectUIParameters
{
    public TextMeshProUGUI ui;
    public Color color;
    public AudioClip sound;
    public UIEffect effect;

    public void Apply(string text, AudioSource source)
    {
        if (ui != null)
        {
            ui.color = color;
            ui.text = text;
        }

        if (sound != null)
        {
            source.PlayOneShot(sound);
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
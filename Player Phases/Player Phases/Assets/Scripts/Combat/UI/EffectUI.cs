using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EffectUI : MonoBehaviour
{
    [Header("Health bar")]
    public Slider healthBar;

    [Header("Text for damage and healing")]
    public EffectUIParameters damage;
    public EffectUIParameters healing;
    public EffectUIParameters paralysis;

    [Header("Timing")]
    public float timeBetweenEffects;

    private Stack<Effect> toDisplay = new Stack<Effect>();

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
    }

    public void AddEffect(Effect effect)
    {
        toDisplay.Push(effect);
    }

    private IEnumerator IterateThroughEffects()
    {
        while(true)
        {
            // keep checking if there are any effects to display, and then call them to be displayed
            while (toDisplay.Count > 0)
            {
                DisplayEffect(toDisplay.Pop());

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
            case Effect_ID.paralysis:
            {
                paralysis.Apply("Paralyzed!", refAudioSource);
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
        paralysis.Clear();
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
        ui.color = color;
        ui.text = text;

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
        ui.text = "";
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MovementDialogueProcessor : MonoBehaviour
{
    private int previousIndex = -1;

    [Header("Resources")]
    public TextMeshProUGUI movementDialogueText;
    public MovementDialogue dialogue;

    [Header("Timing")]
    public float secondsBetweenLetters;
    public uint blipEvery = 1;
    public float secondsBeforeClear;

    [Header("Sound")]
    public AudioClip speechBlip;
    [Range(0.0f, 1.0f)]
    public float volume;
    private AudioSource refAudioSource;
    public AudioClip counterAttackSound;

    private void Start()
    {
        TryGetComponent(out refAudioSource);

        if (refAudioSource == null)
        {
            refAudioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void Display()
    {
        if (movementDialogueText != null)
        {
            Clear();

            List<int> possibleIndices = new List<int>();
            int rand = 0;

            if (dialogue.quotes.Length > 1)
            {
                // don't include the previous index so we don't use the same quote twice in a row
                for (int i = 0; i < dialogue.quotes.Length; ++i)
                {
                    if (i != previousIndex)
                    {
                        possibleIndices.Add(i);
                    }
                }

                rand = possibleIndices[Random.Range(0, possibleIndices.Count)];
                StartCoroutine(WriteText(dialogue.quotes[rand]));

                previousIndex = rand;
            }
            else
            {
                if (dialogue.quotes.Length > 0)
                {
                    rand = Random.Range(0, dialogue.quotes.Length);
                    StartCoroutine(WriteText(dialogue.quotes[rand]));
                }
            }
        }
    }

    public void Display(string toDisplay)
    {
        if (movementDialogueText != null)
        {
            Clear();

            // display a specific message
            StartCoroutine(WriteText(toDisplay));
        }
    }

    public void DisplayCounterAttackQuote()
    {
        if (movementDialogueText != null)
        {
            Clear();

            refAudioSource.PlayOneShot(counterAttackSound, volume);

            StartCoroutine(WriteText(dialogue.counterAttackQuotes[Random.Range(0, dialogue.counterAttackQuotes.Length)]));
        }
    }

    public void DisplayDeathQuote()
    {
        if (movementDialogueText != null)
        {
            Clear();

            StartCoroutine(WriteText(dialogue.deathQuote));
        }
    }

    private IEnumerator WriteText(string toWrite)
    {
        for (int i = 0; i < toWrite.Length; ++i)
        {
            movementDialogueText.text += toWrite[i];

            if (blipEvery != 0)
            {
                if (i % blipEvery == 0)
                {
                    refAudioSource.PlayOneShot(speechBlip);
                }
            }

            yield return new WaitForSecondsRealtime(secondsBetweenLetters);
        }

        yield return new WaitForSecondsRealtime(secondsBeforeClear);

        movementDialogueText.text = "";
    }

    public void Clear()
    {
        StopAllCoroutines();

        refAudioSource.Stop();
        movementDialogueText.text = "";
    }
}

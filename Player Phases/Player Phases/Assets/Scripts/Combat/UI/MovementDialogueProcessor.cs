using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MovementDialogueProcessor : MonoBehaviour
{
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

    private void Start()
    {
        refAudioSource = gameObject.AddComponent<AudioSource>();
    }

    public void Display()
    {
        if (movementDialogueText != null)
        {
            Clear();

            StartCoroutine(WriteText(dialogue.quotes[Random.Range(0, dialogue.quotes.Length)]));
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

            yield return new WaitForSeconds(secondsBetweenLetters);
        }

        yield return new WaitForSeconds(secondsBeforeClear);

        movementDialogueText.text = "";
    }

    public void Clear()
    {
        StopAllCoroutines();

        refAudioSource.Stop();
        movementDialogueText.text = "";
    }
}

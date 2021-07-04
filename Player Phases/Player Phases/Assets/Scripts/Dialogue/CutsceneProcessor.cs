using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

using SHTANKCutscenes;

public class CutsceneProcessor : MonoBehaviour
{
    [Header("Dialogue Parent")]
    public CanvasGroup parentCanvasGroup;
    public float fadeDelay;

    [Header("Resources")]
    public TextMeshProUGUI dialogueText;
    public Image dialogueBackground;
    public Image dialogueArrow;

    [Header("Speakers")]
    public List<Speaker> speakers = new List<Speaker>();
    public Speaker defaultSpeaker;

    [Header("Timing")]
    public float secondsBetweenLetters;
    public uint blipEvery = 1;

    private bool moveOn = false;
    private bool running = false;

    private Vector3 targetPosition;
    public Vector3 targetRotation;

    private UtilityAudioManager refAudioManager;

    private void Start()
    {
        refAudioManager = FindObjectOfType<UtilityAudioManager>();

        StartCoroutine(UtilityStaticFunctions.CanvasGroupCrossFadeAlpha(parentCanvasGroup, 0.0f, 0.0f));
    }

    private void LateUpdate()
    {
        if (Input.anyKeyDown && running)
        {
            moveOn = true;
        }
    }

    public void Display(CutsceneDefinition cutsceneDefinition, params GameObject[] participants)
    {
        Debug.Log("Trying to display dialogue!");

        // create a new cutscene and adjust the camera's position
        Cutscene cutscene = new Cutscene(cutsceneDefinition.steps, participants);
        CalculateCameraTransform(cutscene);

        StartCoroutine(WriteText(cutscene));
    }

    private IEnumerator WriteText(Cutscene cutscene)
    {
        yield return new WaitForSeconds(1);

        for (int i = 0; i < cutscene.steps.Count; ++i)
        {
            Clear();

            // assign who is speaking
            Speaker speaker = GetSpeaker(cutscene.steps[i].speaker);

            // enable the background and arrow
            dialogueBackground.gameObject.SetActive(true);
            dialogueArrow.gameObject.SetActive(true);

            // animation for when dialogue starts
            if (i == 0)
            {
                StartCoroutine(UtilityStaticFunctions.CanvasGroupCrossFadeAlpha(parentCanvasGroup, 1.0f, fadeDelay));

                yield return new WaitForSeconds(fadeDelay);
            }

            running = true;

            // loop through the content of the dialogue and display it
            for (int j = 0; j < cutscene.steps[i].text.Length; ++j)
            {
                // allow skipping to the end of a piece of dialogue
                if (moveOn)
                {
                    dialogueText.text = cutscene.steps[i].text;

                    moveOn = false;

                    break;
                }

                dialogueText.text += cutscene.steps[i].text[j];

                // also play a sound
                if (blipEvery != 0)
                {
                    if (j % blipEvery == 0)
                    {
                        refAudioManager.QueueSound(speaker.voiceBlip);
                    }
                }

                yield return new WaitForSeconds(secondsBetweenLetters);
            }

            // wait to move on until the player inputs something
            while(moveOn == false)
            {
                yield return new WaitForEndOfFrame();
            }

            moveOn = false;

            // animation for when dialogue ends
            if (i == cutscene.steps.Count - 1)
            {
                yield return new WaitForSeconds(fadeDelay);

                Clear();
            }
        }

        StartCoroutine(UtilityStaticFunctions.CanvasGroupCrossFadeAlpha(parentCanvasGroup, 0.0f, fadeDelay));
        running = false;
        FindObjectOfType<SHTANKManager>().TryEndDialogue();
    }

    private Speaker GetSpeaker(string speakerName)
    {
        for (int i = 0; i < speakers.Count; ++i)
        {
            if (speakerName == speakers[i].name)
            {
                return speakers[i];
            }
        }

        return defaultSpeaker;
    }

    public void Clear()
    {
        dialogueText.text = "";
    }

    private void CalculateCameraTransform(Cutscene cutscene)
    {
        if (cutscene.participants.Count > 0)
        {
            Vector3 average = Vector3.zero;

            for (int i = 0; i < cutscene.participants.Count; ++i)
            {
                average += cutscene.participants[i].obj.transform.position;
            }

            average /= cutscene.participants.Count;

            average -= Vector3.forward * 1.5f;
            average += Vector3.up * 2.25f;

            targetPosition = average;
        }
    }

    public Vector3 GetCameraTargetPosition()
    {
        return targetPosition;
    }

    public Quaternion GetCameraTargetRotation()
    {
        return Quaternion.Euler(targetRotation);
    }
}

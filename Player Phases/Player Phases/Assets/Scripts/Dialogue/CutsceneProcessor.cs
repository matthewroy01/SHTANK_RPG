using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class CutsceneProcessor : MonoBehaviour
{
    [Header("Dialogue Parent")]
    public RectTransform parentRectTransform;
    public float fadeDelay;

    [Header("Resources")]
    public TextMeshProUGUI dialogueText;
    public Image dialogueBackground;
    public Image dialogueArrow;

    [Header("Speakers")]
    public List<SHTANKCutscenes.Speaker> speakers = new List<SHTANKCutscenes.Speaker>();
    public SHTANKCutscenes.Speaker defaultSpeaker;

    [Header("Timing")]
    public float secondsBetweenLetters;
    public uint blipEvery = 1;

    private bool moveOn = false;
    private bool running = false;

    private UtilityAudioManager refAudioManager;

    private void Start()
    {
        refAudioManager = FindObjectOfType<UtilityAudioManager>();

        //parentRectTransform.localPosition = new Vector2(0.0f, -1200.0f);
    }

    private void LateUpdate()
    {
        if (Input.anyKeyDown && running)
        {
            moveOn = true;
        }
    }

    public void Display(SHTANKCutscenes.CutsceneDefinition cutscene)
    {
        Debug.Log("Trying to display dialogue!");

        StartCoroutine(WriteText(cutscene));
    }

    private IEnumerator WriteText(SHTANKCutscenes.CutsceneDefinition cutscene)
    {
        yield return new WaitForSeconds(1);

        for (int i = 0; i < cutscene.steps.Count; ++i)
        {
            Clear();

            // assign who is speaking
            SHTANKCutscenes.Speaker speaker = GetSpeaker(cutscene.steps[i].speaker);

            // enable the background and arrow
            dialogueBackground.gameObject.SetActive(true);
            dialogueArrow.gameObject.SetActive(true);

            // animation for when dialogue starts
            if (i == 0)
            {
                //yield return new WaitForSeconds(fadeDelay);
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

        running = false;
        FindObjectOfType<SHTANKManager>().TryEndDialogue();
    }

    private SHTANKCutscenes.Speaker GetSpeaker(string speakerName)
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
}

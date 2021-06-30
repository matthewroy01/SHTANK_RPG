using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class DialogueProcessor : MonoBehaviour
{
    //public DialogueDefinition debugDefinition;

    [Header("Dialogue Parent")]
    public RectTransform parentRectTransform;
    public float fadeDelay;

    [Header("Resources")]
    public TextMeshProUGUI dialogueRightBody;
    public TextMeshProUGUI dialogueRightName;
    public GameObject backgroundRight;
    public Image portraitRight;
    public TextMeshProUGUI dialogueLeftBody;
    public TextMeshProUGUI dialogueLeftName;
    public GameObject backgroundLeft;
    public Image portraitLeft;

    private TextMeshProUGUI currentBody;
    private TextMeshProUGUI currentName;
    private Image currentPortrait;
    private GameObject currentBackground;

    [Header("Timing")]
    public float secondsBetweenLetters;
    public uint blipEvery = 1;

    private bool moveOn = false;

    private UtilityAudioManager refAudioManager;

    private void Start()
    {
        refAudioManager = FindObjectOfType<UtilityAudioManager>();

        parentRectTransform.localPosition = new Vector2(0.0f, -1200.0f);
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            moveOn = true;
        }
    }

    public void Display()
    {
        Debug.Log("Trying to display dialogue!");
    }

    private IEnumerator WriteText()
    {
        yield return new WaitForSeconds(1);

        /*for (int i = 0; i < dialogue.sentences.Count; ++i)
        {
            Clear();

            // assign which text boxes we should be using
            if (dialogue.sentences[i].left)
            {
                currentBackground = backgroundLeft;
                currentBody = dialogueLeftBody;
                currentName = dialogueLeftName;
                currentPortrait = portraitLeft;
            }
            else
            {
                currentBackground = backgroundRight;
                currentBody = dialogueRightBody;
                currentName = dialogueRightName;
                currentPortrait = portraitRight;
            }

            // enable the background
            currentBackground.SetActive(true);

            // display the name of the character who is speaking
            currentName.text = dialogue.sentences[i].speaker.speakerName;
            currentName.transform.DOPunchScale(currentName.transform.localScale * 0.1f, 0.15f, 0, 0.0f);

            // display the correct portrait
            currentPortrait.sprite = dialogue.sentences[i].speaker.normal;
            currentPortrait.gameObject.SetActive(true);
            currentPortrait.transform.DOPunchScale(currentPortrait.transform.localScale * 0.1f, 0.15f, 0, 0.0f);

            if (i == 0)
            {
                // fade effect when dialogue starts
                parentRectTransform.transform.DOLocalMoveY(0.0f, fadeDelay);
                yield return new WaitForSeconds(fadeDelay);
            }

            // loop through the content of the dialogue and display it
            for (int j = 0; j < dialogue.sentences[i].content.Length; ++j)
            {
                // allow skipping to the end of a piece of dialogue
                if (moveOn)
                {
                    currentBody.text = dialogue.sentences[i].content;

                    moveOn = false;

                    break;
                }

                currentBody.text += dialogue.sentences[i].content[j];

                // also play a sound
                if (blipEvery != 0)
                {
                    if (j % blipEvery == 0)
                    {
                        refAudioManager.QueueSound(dialogue.sentences[i].speaker.voiceBlip);
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

            if (i == dialogue.sentences.Count - 1)
            {
                parentRectTransform.transform.DOLocalMoveY(-1200.0f, fadeDelay);

                yield return new WaitForSeconds(fadeDelay);

                Clear();
            }
        }

        FindObjectOfType<SHTANKManager>().TryEndDialogue();*/
    }

    public void Clear()
    {
        dialogueRightBody.text = "";
        dialogueRightName.text = "";
        dialogueLeftBody.text = "";
        dialogueLeftName.text = "";

        portraitLeft.gameObject.SetActive(false);
        portraitRight.gameObject.SetActive(false);

        backgroundLeft.SetActive(false);
        backgroundRight.SetActive(false);
    }
}

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

    [Header("Actor Positioning")]
    public float actorDistance;
    private Vector3 actorPosition;

    [Header("Camera")]
    public Vector3 targetRotation;
    private Vector3 targetPosition;
    private const float CAMERA_OFFSET_Y = 1.4f;
    private const float CAMERA_OFFSET_Z = 0.4f;

    private bool moveOn = false;
    private bool running = false; // this boolean keeps track of whether or not the dialogue display is running so that the player interaction input doesn't get detected immediately
    private bool writing = false; // this boolean keeps track of whether or not the dialogue display is still writing text to the screen to see if we should still be animating actors

    private List<Actor> actors = new List<Actor>();
    private Coroutine actorAnimationCoroutine;

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

    public void Display(CutsceneDefinition cutsceneDefinition, params GameObject[] newActors)
    {
        Debug.Log("Trying to display dialogue!");

        Cutscene cutscene = new Cutscene(cutsceneDefinition);

        actors.Clear();
        for (int i = 0; i < newActors.Length; ++i)
        {
            actors.Add(new Actor(newActors[i], i == 0 ? "Shade" : "No Clue"));
        }

        if (actors[0].obj.transform.position.x <= actors[1].obj.transform.position.x)
        {
            actorPosition = actors[1].obj.transform.position + (Vector3.left * actorDistance);
        }
        else
        {
            actorPosition = actors[1].obj.transform.position + (Vector3.right * actorDistance);
        }

        Debug.Log(actorPosition);

        // create a new cutscene and adjust the camera's position
        CalculateCameraTransform();

        StartCoroutine(WriteText(cutsceneDefinition));
    }

    private IEnumerator WriteText(CutsceneDefinition cutscene)
    {
        // move the starting actor before beginning
        StartCoroutine(UtilityStaticFunctions.MoveGameObjectOverTime(actors[0].obj, actorPosition, 1.0f));

        yield return new WaitForSeconds(1.0f);

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
            writing = true;

            // start animating the current actor
            if (actorAnimationCoroutine != null)
            {
                StopCoroutine(actorAnimationCoroutine);
            }
            actorAnimationCoroutine = StartCoroutine(ActorAnimation(GetActor(cutscene.steps[i].speaker)));

            // loop through the content of the dialogue and display it
            for (int j = 0; j < cutscene.steps[i].text.Length; ++j)
            {
                // allow skipping to the end of a piece of dialogue
                if (moveOn)
                {
                    dialogueText.text = cutscene.steps[i].text;

                    moveOn = false;
                    writing = false;

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

            writing = false;

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


    public IEnumerator ActorAnimation(Actor actor)
    {
        // if no actor was provided, don't bother animating
        if (actor == null)
        {
            yield break;
        }

        Vector3 defaultScale = actor.obj.transform.localScale;

        // keep animating as long as the Cutscene Processor is writing text on the screen still
        while (writing == true)
        {
            actor.obj.transform.DOPunchScale(new Vector3(0.0f, 0.5f, 0.5f) * 0.15f, 0.2f, 0, 0);

            yield return new WaitForSecondsRealtime(0.2f);

            actor.obj.transform.localScale = defaultScale;
        }

        // reset to default scale, just in case
        actor.obj.transform.localScale = defaultScale;
    }

    public void Clear()
    {
        dialogueText.text = "";
    }

    private void CalculateCameraTransform()
    {
        if (actors.Count > 1)
        {
            Vector3 average = actorPosition;

            for (int i = 1; i < actors.Count; ++i)
            {
                // accumulate an average position or the midpoint
                average += actors[i].obj.transform.position;
            }

            // create the average
            average /= actors.Count;

            // adjust the camera's position
            average -= Vector3.forward * CAMERA_OFFSET_Z;
            average += Vector3.up * CAMERA_OFFSET_Y;

            // set the target position
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

    private Actor GetActor(string name)
    {
        foreach (Actor actor in actors)
        {
            if (actor.name == name)
            {
                return actor;
            }
        }

        return null;
    }
}

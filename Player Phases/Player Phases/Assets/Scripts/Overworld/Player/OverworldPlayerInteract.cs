using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class OverworldPlayerInteract : MonoBehaviour
{
    public SpriteRenderer icon;

    public Sprite iconTalk;
    public Sprite iconExclamationPoint;

    private SHTANKCutscenes.Interactable currentInteractable;

    private Coroutine iconVisualCoroutine;
    private Tweener iconVisualTweener;
    private bool iconActive = false;
    
    private float iconDefaultY;

    private OverworldPlayerController controller;

    private void Awake()
    {
        if (!TryGetComponent(out controller))
        {
            Debug.LogError("OverworldPlayerInteract could not find component OverworldPlayerController.");
        }

        iconDefaultY = icon.transform.localPosition.y;

        icon.DOFade(0.0f, 0.0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        // show interaction object
        if (other.CompareTag("Overworld_NPC") && other.TryGetComponent(out currentInteractable) == true)
        {
            SetIcon();
            iconActive = true;

            // reset and reactivate icon animation
            if (iconVisualTweener != null)
            {
                iconVisualTweener.Kill();
            }
            if (iconVisualCoroutine != null)
            {
                StopCoroutine(iconVisualCoroutine);
            }
            iconVisualCoroutine = StartCoroutine(IconVisualCoroutine());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Overworld_NPC"))
        {
            iconActive = false;

            icon.DOFade(0.0f, 0.5f);
        }
    }

    public void MyUpdate()
    {
        // input for the player actually interacting with the Interactable
        if (Input.GetKeyDown(KeyCode.E))
        {
            controller.refSHTANKManager.TryBeginDialogue();
        }
    }

    private IEnumerator IconVisualCoroutine()
    {
        icon.DOFade(1.0f, 0.0f);

        float targetYHigh = iconDefaultY + 0.25f;
        float targetYLow = iconDefaultY - 0.25f;

        icon.transform.localPosition = new Vector3(icon.transform.localPosition.x, targetYLow, icon.transform.localPosition.z);

        yield return new WaitForSecondsRealtime(0.25f);

        while (iconActive == true)
        {
            iconVisualTweener = icon.transform.DOLocalMoveY(targetYHigh, 0.5f, false).SetEase(Ease.InOutQuad);

            yield return new WaitForSecondsRealtime(0.5f);

            iconVisualTweener = icon.transform.DOLocalMoveY(targetYLow, 0.5f, false).SetEase(Ease.InOutQuad);

            yield return new WaitForSecondsRealtime(0.5f);
        }
    }

    private void SetIcon()
    {
        if (currentInteractable != null)
        {
            switch (currentInteractable.type)
            {
                case SHTANKCutscenes.Interactable_Type.NPC:
                {
                    icon.sprite = iconTalk;
                    break;
                }
                case SHTANKCutscenes.Interactable_Type.inanimate:
                {
                    icon.sprite = iconExclamationPoint;
                    break;
                }
            }
        }
    }
}

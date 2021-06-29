using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class OverworldPlayerInteract : MonoBehaviour
{
    public SpriteRenderer icon;

    public Sprite iconTalk;
    public Sprite iconExclamationPoint;

    private Coroutine iconVisualCoroutine;
    private Tweener iconVisualTweener;
    private bool iconActive = false;
    
    private float iconDefaultY;

    private void Awake()
    {
        iconDefaultY = icon.transform.localPosition.y;

        icon.DOFade(0.0f, 0.0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        // show interaction object
        if (other.CompareTag("Overworld_NPC"))
        {
            iconActive = true;

            if (iconVisualTweener != null)
            {
                iconVisualTweener.Kill();
            }
            if (iconVisualCoroutine != null)
            {
                StopCoroutine(iconVisualCoroutine);
            }
            iconVisualCoroutine = StartCoroutine(IconVisualCoroutine());

            if (Input.GetKeyDown(KeyCode.E))
            {

            }
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

    private IEnumerator IconVisualCoroutine()
    {
        icon.DOFade(1.0f, 0.15f);

        float targetYHigh = iconDefaultY + 0.5f;
        float targetYLow = iconDefaultY - 0.5f;

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
}

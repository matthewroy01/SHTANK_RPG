using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UISlide : UIEffect
{
    public bool horizontal;
    public float timeToWait;
    [Range(0.0f, 1.0f)]
    public float slideSpeedIn;
    [Range(0.0f, 1.0f)]
    public float slideSpeedOut;
    private RectTransform refRectTransform;

    private void Start()
    {
        refRectTransform = GetComponent<RectTransform>();
    }

    public override void DoEffect()
    {
        StopAllCoroutines();
        Slide();
    }

    private void Slide()
    {
        if (horizontal)
        {
            StartCoroutine(SlideAnimationHorizontal());
        }
        else
        {
            StartCoroutine(SlideAnimationVertical());
        }
    }

    private IEnumerator SlideAnimationHorizontal()
    {
        refRectTransform.localPosition = new Vector2(refRectTransform.rect.width * -1.0f, 0.0f);

        while (Vector2.Distance(refRectTransform.localPosition, Vector2.zero) > 5)
        {
            refRectTransform.localPosition = Vector2.Lerp(refRectTransform.localPosition, Vector2.zero, slideSpeedIn);

            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(timeToWait);

        while (Vector2.Distance(refRectTransform.localPosition, new Vector2(refRectTransform.rect.width, 0.0f)) > 5)
        {
            refRectTransform.localPosition = Vector2.Lerp(refRectTransform.localPosition, new Vector2(refRectTransform.rect.width, 0.0f), slideSpeedOut);

            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator SlideAnimationVertical()
    {
        refRectTransform.localPosition = new Vector2(0.0f, refRectTransform.rect.height * -1.0f);

        while (Vector2.Distance(refRectTransform.localPosition, Vector2.zero) > 5)
        {
            refRectTransform.localPosition = Vector2.Lerp(refRectTransform.localPosition, Vector2.zero, slideSpeedIn);

            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForSeconds(timeToWait);

        while (Vector2.Distance(refRectTransform.localPosition, new Vector2(0.0f, refRectTransform.rect.height)) > 5)
        {
            refRectTransform.localPosition = Vector2.Lerp(refRectTransform.localPosition, new Vector2(0.0f, refRectTransform.rect.height), slideSpeedOut);

            yield return new WaitForEndOfFrame();
        }
    }
}

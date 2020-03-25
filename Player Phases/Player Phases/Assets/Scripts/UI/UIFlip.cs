using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFlip : UIEffect
{
    public UIFlipAxis axis;
    public float accuracyThreshold;
    [Range(0.0f, 1.0f)]
    public float lerpSpeed;
    private Vector3 defaultScale;

    private RectTransform refRectTransform;

    private void Start()
    {
        refRectTransform = GetComponent<RectTransform>();
        defaultScale = transform.localScale;
    }

    public override void DoEffect()
    {
        Flip();
    }

    private void Flip()
    {
        StartCoroutine(DoFlip());
    }

    private IEnumerator DoFlip()
    {
        Vector3 targetScale;

        // put scale to -1
        switch(axis)
        {
            case UIFlipAxis.x:
            {
                targetScale = new Vector3(-1.0f, refRectTransform.localScale.y, refRectTransform.localScale.z);

                // lerp on x
                while (refRectTransform.localScale.x > -1.0f + accuracyThreshold)
                {
                    refRectTransform.localScale = Vector3.Lerp(refRectTransform.localScale, targetScale, lerpSpeed);

                    yield return new WaitForFixedUpdate();
                }
                break;
            }
            case UIFlipAxis.y:
            {
                targetScale = new Vector3(refRectTransform.localScale.x, -1.0f, refRectTransform.localScale.z);

                // lerp on y
                while (refRectTransform.localScale.y > -1.0f + accuracyThreshold)
                {
                    refRectTransform.localScale = Vector3.Lerp(refRectTransform.localScale, targetScale, lerpSpeed);

                    yield return new WaitForFixedUpdate();
                }
                break;
            }
            case UIFlipAxis.z:
            {
                targetScale = new Vector3(refRectTransform.localScale.x, refRectTransform.localScale.y, -1.0f);

                // lerp on z
                while (refRectTransform.localScale.z > -1.0f + accuracyThreshold)
                {
                    refRectTransform.localScale = Vector3.Lerp(refRectTransform.localScale, targetScale, lerpSpeed);

                    yield return new WaitForFixedUpdate();
                }
                break;
            }
        }

        // put scale back to 1
        switch (axis)
        {
            case UIFlipAxis.x:
            {
                targetScale = new Vector3(1.0f, refRectTransform.localScale.y, refRectTransform.localScale.z);

                // lerp on x
                while (refRectTransform.localScale.x < 1.0f - accuracyThreshold)
                {
                    refRectTransform.localScale = Vector3.Lerp(refRectTransform.localScale, targetScale, lerpSpeed);

                    yield return new WaitForFixedUpdate();
                }

                break;
            }
            case UIFlipAxis.y:
            {
                targetScale = new Vector3(refRectTransform.localScale.x, 1.0f, refRectTransform.localScale.z);

                // lerp on y
                while (refRectTransform.localScale.y < 1.0f - accuracyThreshold)
                {
                    refRectTransform.localScale = Vector3.Lerp(refRectTransform.localScale, targetScale, lerpSpeed);

                    yield return new WaitForFixedUpdate();
                }
                break;
            }
            case UIFlipAxis.z:
            {
                targetScale = new Vector3(refRectTransform.localScale.x, refRectTransform.localScale.y, 1.0f);

                // lerp on z
                while (refRectTransform.localScale.z < -1.0f - accuracyThreshold)
                {
                    refRectTransform.localScale = Vector3.Lerp(refRectTransform.localScale, targetScale, lerpSpeed);

                    yield return new WaitForFixedUpdate();
                }
                break;
            }
        }

        // reset scale back to default to be sure
        refRectTransform.localScale = defaultScale;
    }

    public enum UIFlipAxis { x, y, z };
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPop : UIEffect
{
    private RectTransform refRectTransform;
    private Vector3 defaultScale;

    public float scaleMultiplier;
    [Range(0.0f, 1.0f)]
    public float resetScaleSpeed;

    private void Start()
    {
        refRectTransform = GetComponent<RectTransform>();
        defaultScale = refRectTransform.localScale;
    }

    private void Update()
    {
        if (refRectTransform.localScale != defaultScale)
        {
            // reset scale back to default
            refRectTransform.localScale = Vector3.Lerp(refRectTransform.localScale, defaultScale, resetScaleSpeed);
        }
    }

    public override void DoEffect()
    {
        Pop();
    }

    private void Pop()
    {
        // increase scale
        refRectTransform.localScale = defaultScale * scaleMultiplier;
    }
}

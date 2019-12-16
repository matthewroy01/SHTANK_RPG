using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIShake : UIEffect
{
    private RectTransform refRectTransform;
    private float defaultY;

    public float duration;
    public float strength;
    public float speed;
    private bool shaking = false;

    void Start()
    {
        refRectTransform = GetComponent<RectTransform>();
        defaultY = refRectTransform.localPosition.y;
    }

    public override void DoEffect()
    {
        Shake();
    }

    private void Shake()
    {
        StopCoroutine(DoShake());
        CancelInvoke("StopShake");

        StartCoroutine(DoShake());
        Invoke("StopShake", duration);
    }

    public void Update()
    {
        // reset position while not shaking
        if (!shaking && refRectTransform.localPosition.y != defaultY)
        {
            refRectTransform.localPosition = Vector3.Lerp(refRectTransform.transform.localPosition, new Vector3(refRectTransform.transform.localPosition.x, defaultY, refRectTransform.transform.localPosition.z), 0.5f);
        }
    }

    private IEnumerator DoShake()
    {
        shaking = true;

        while (shaking)
        {
            refRectTransform.transform.localPosition = new Vector3(refRectTransform.transform.localPosition.x, defaultY + (Mathf.Cos(Time.time * speed) * strength), refRectTransform.transform.localPosition.z);

            yield return new WaitForEndOfFrame();
        }
    }

    private void StopShake()
    {
        shaking = false;
    }
}

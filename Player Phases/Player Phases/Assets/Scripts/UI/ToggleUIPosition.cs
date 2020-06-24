using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleUIPosition : MonoBehaviour
{
    public bool hiddenByDefault;

    [Header("Movement")]
    public RectTransform toMove;
    public Vector3 moveBy;
    public float lerpSpeed;
    private Vector3 posRevealed;
    private Vector3 posHidden;

    [Header("Sprite Change")]
    public Image image;

    private bool hidden = false;

    void Start()
    {
        posRevealed = toMove.localPosition;
        posHidden = toMove.localPosition + moveBy;

        if (hiddenByDefault)
        {
            hidden = true;
            toMove.localPosition = posHidden;
            image.rectTransform.localScale = new Vector3(image.rectTransform.localScale.x, -1.0f, image.rectTransform.localScale.z);
        }
    }

    public void ToggleUI()
    {
        StopAllCoroutines();
        hidden = !hidden;
        StartCoroutine(Move());

        if (hidden)
        {
            image.rectTransform.localScale = new Vector3(image.rectTransform.localScale.x, -1.0f, image.rectTransform.localScale.z);
        }
        else
        {
            image.rectTransform.localScale = new Vector3(image.rectTransform.localScale.x, 1.0f, image.rectTransform.localScale.z);
        }
    }

    private IEnumerator Move()
    {
        if (hidden)
        {
            while (Vector2.Distance(toMove.localPosition, posHidden) > 1.0f)
            {
                toMove.localPosition = Vector2.Lerp(toMove.localPosition, posHidden, lerpSpeed);
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            while (Vector2.Distance(toMove.localPosition, posRevealed) > 1.0f)
            {
                toMove.localPosition = Vector2.Lerp(toMove.localPosition, posRevealed, lerpSpeed);
                yield return new WaitForEndOfFrame();
            }
        }
    }
}

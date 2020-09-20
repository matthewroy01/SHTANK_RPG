using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;

public class ToggleUIPosition : MonoBehaviour
{
    public bool hiddenByDefault;

    [Header("Movement")]
    public RectTransform toMove;
    public Vector3 moveBy;
    public float animationTime;
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
        hidden = !hidden;

        if (hidden == true)
        {
            toMove.DOLocalMove(posHidden, animationTime, false);

            if (image != null)
            {
                image.rectTransform.localScale = new Vector3(image.rectTransform.localScale.x, -1.0f, image.rectTransform.localScale.z);
            }
        }
        else
        {
            toMove.DOLocalMove(posRevealed, animationTime, false);

            if (image != null)
            {
                image.rectTransform.localScale = new Vector3(image.rectTransform.localScale.x, 1.0f, image.rectTransform.localScale.z);
            }
        }
    }
}

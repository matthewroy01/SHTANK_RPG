using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TooltipController : MonoBehaviour
{
    public RectTransform tooltipTransform;
    public TextMeshProUGUI tooltipText;
    public Image tooltipBackground;

    [Header("Offset Sizes")]
    public float extraPixelsBackground;
    public float extraPixelsMouse;

    [HideInInspector]
    public bool shouldDisplay = false;
    private string toDisplay;

    private List<TooltipDetector> detectors = new List<TooltipDetector>();

    private void Start()
    {
        detectors = new List<TooltipDetector>(FindObjectsOfType<TooltipDetector>());
        Debug.Log("Detectors found: " + detectors.Count);
    }

    private void Update()
    {
        CheckInput();

        // enable or disable tooltip text and set its position
        tooltipTransform.gameObject.SetActive(shouldDisplay);
        tooltipTransform.position = Input.mousePosition + new Vector3(extraPixelsMouse, extraPixelsMouse * -1.0f, 0.0f);
    }

    private void CheckInput()
    {
        bool mousingOver = false;
        string toDisplay = "";

        // loop through all tooltip detectors to check if any are being moused over
        for (int i = 0; i < detectors.Count; ++i)
        {
            // if one is being moused over, save the tooltip text
            if (detectors[i].CheckForMouseOver() == true)
            {
                mousingOver = true;
                if (detectors[i] != null)
                {
                    if (detectors[i].tooltipOverride != "")
                    {
                        toDisplay = detectors[i].tooltipOverride;
                    }
                    else if (detectors[i].statusDef != null)
                    {
                        toDisplay = detectors[i].statusDef.tooltip;
                    }
                }
            }
        }

        // enable or disable the tooltip depending on if one of the detectors detected the mouse
        if (mousingOver)
        {
            EnableTooltip(toDisplay);
        }
        else
        {
            DisableTooltip();
        }
    }

    private void EnableTooltip(string text)
    {
        // set the text
        tooltipText.text = text;

        // resize and reposition the background
        tooltipBackground.rectTransform.sizeDelta = new Vector2(tooltipText.renderedWidth + extraPixelsBackground, tooltipText.renderedHeight + extraPixelsBackground);
        tooltipBackground.rectTransform.position = tooltipText.rectTransform.position - (new Vector3(extraPixelsBackground, extraPixelsBackground * -1.0f, 0.0f) * 0.25f);

        // let the script know we should display in Update
        shouldDisplay = true;
    }

    private void DisableTooltip()
    {
        // let the script know we shouldn't display in Update
        shouldDisplay = false;
    }
}

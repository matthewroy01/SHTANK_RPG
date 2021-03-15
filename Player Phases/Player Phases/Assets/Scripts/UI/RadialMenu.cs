using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RadialMenu : MonoBehaviour
{
    [Header("Parent Transforms")]
    public RectTransform parentMain;
    public RectTransform parentFill;
    public RectTransform parentSeparator;
    public RectTransform parentText;

    [Header("Radii")]
    public float distanceFromCenterSeparator;
    public float distanceFromCenterText;

    [Header("Base Objects")]
    public Image baseFill;
    public Image baseSeparator;
    public TextMeshProUGUI baseText;

    // lists of objects
    private List<Image> listFill = new List<Image>();
    private List<Image> listSeparator = new List<Image>();
    private List<TextMeshProUGUI> listText = new List<TextMeshProUGUI>();

    private int currentAbilityNum;

    private List<RadialButton> radialButtons = new List<RadialButton>();
    private RadialButton currentButton;

    [Space]
    public Moveset debugMoveset;

    private Vector2 FROM = new Vector2(0.5f, 1.0f);
    private Vector2 CENTER = new Vector2(0.5f, 0.5f);

    private float angle = 0.0f;

    private void Start()
    {
        int numOfButtons = 4;

        float angleBetween = (360 / numOfButtons);
        float fillAmount = 1.0f / (float)numOfButtons;

        for (int i = 0; i < numOfButtons; ++i)
        {
            // calculate angles of this button
            float startingAngle = i * angleBetween;
            float endingAngle = startingAngle + angleBetween;

            List<Vector2> angleRanges = new List<Vector2>();
            angleRanges.Add(new Vector2(startingAngle, endingAngle));

            Image fill, separator;
            TextMeshProUGUI text;

            if (i == 0)
            {
                // reuse the base objects
                fill = baseFill;
                separator = baseSeparator;
                text = baseText;
            }
            else
            {
                // duplicate the base objects 
                fill = Instantiate(baseFill, parentFill);
                separator = Instantiate(baseSeparator, parentSeparator);
                text = Instantiate(baseText, parentText);
            }

            // add new buttons
            radialButtons.Add(new RadialButton(angleRanges, fill, separator, text));
            radialButtons[radialButtons.Count - 1].Select(false);

            fill.rectTransform.eulerAngles = new Vector3(0.0f, 0.0f, angleBetween * i * -1.0f);
            fill.fillAmount = fillAmount;
            fill.CrossFadeAlpha(0.25f, 0.0f, true);
        }
    }

    public void Enable()
    {

    }

    private void Update()
    {
        CalculateAngle();

        RadialButton newButton = GetCurrentButton();

        // if the selected button has changed, update all the buttons
        if (newButton != currentButton)
        {
            // deselect the current button
            if (currentButton != null)
            {
                currentButton.Select(false);
            }

            currentButton = newButton;

            // select the new current button
            currentButton.Select(true);
        }
    }

    public void CalculateAngle()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector2 to = new Vector2(mousePos.x / Screen.width, mousePos.y / Screen.height);

        // math here provided by Gamad on YouTube: https://www.youtube.com/watch?v=qBsLezkjJck
        angle = (Mathf.Atan2(FROM.y - CENTER.y, FROM.x - CENTER.x) - Mathf.Atan2(to.y - CENTER.y , to.x - CENTER.x)) * Mathf.Rad2Deg;

        if (angle < 0)
        {
            angle += 360;
        }
    }

    private RadialButton GetCurrentButton()
    {
        // loop through the current buttons and find which one has the current angle within it
        for (int i = 0; i < radialButtons.Count; ++i)
        {
            if (radialButtons[i].CheckIfAngleIsWithinRanges(angle))
            {
                return radialButtons[i];
            }
        }

        // if for some reason, the current angle isn't within any of the radial buttons' specified ranges, just return the current button
        return currentButton;
    }

    private class RadialButton
    {
        public RadialButton(List<Vector2> angles, Image f, Image s, TextMeshProUGUI t)
        {
            angleRanges = angles;

            fill = f;
            separator = s;
            text = t;
        }

        public bool CheckIfAngleIsWithinRanges(float angle)
        {
            // loop through angle ranges to find out if the provided angle is within any of them
            for (int i = 0; i < angleRanges.Count; ++i)
            {
                if (angle > angleRanges[i].x && angle < angleRanges[i].y)
                {
                    return true;
                }
            }

            return false;
        }

        public void Select(bool select)
        {
            // animate the button if it has been selected
            if (select)
            {
                fill.CrossFadeAlpha(0.75f, 0.1f, true);
            }
            // otherwise animate the button if it has been deselected
            else
            {
                fill.CrossFadeAlpha(0.25f, 0.1f, true);
            }
        }

        private List<Vector2> angleRanges; // we keep a list of possible angle ranges in case there angle crosses the 360/0 degree threshold
        public Image fill;
        public Image separator;
        public TextMeshProUGUI text;
    }
}
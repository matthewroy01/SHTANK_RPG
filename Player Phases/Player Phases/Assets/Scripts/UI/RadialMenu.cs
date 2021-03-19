using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RadialMenu : MonoBehaviour
{
    private bool menuActive = false;
    private bool justEnabled = false;

    [Header("Parent Transforms")]
    public CanvasGroup parentMain;
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

    private Coroutine crossFadeAlphaCoroutine;

    private void Start()
    {
        List<Vector2> tmp = new List<Vector2>();
        radialButtons.Add(new RadialButton(tmp, baseFill, baseSeparator, baseText));

        parentMain.alpha = 0.0f;
    }

    private void Update()
    {
        CalculateAngle();

        RadialButton newButton = GetCurrentButton();

        // if the selected button has changed, update all the buttons
        if (newButton != currentButton || justEnabled)
        {
            justEnabled = false;

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

    public void Enable(Moveset moveset)
    {
        int numOfButtons = GetNumOfButtons(moveset);

        if (numOfButtons > 0)
        {
            // start cross fading the canvas group
            if (crossFadeAlphaCoroutine != null)
            {
                StopCoroutine(crossFadeAlphaCoroutine);
            }
            crossFadeAlphaCoroutine = StartCoroutine(CanvasGroupCrossFadeAlpha(parentMain, 1.0f, 0.25f));

            // enable the menu
            menuActive = true;

            // update the contents of the radial menu
            UpdateRadialMenu(numOfButtons);

            justEnabled = true;
        }
    }

    public void Disable()
    {
        // start cross fading the canvas group
        if (crossFadeAlphaCoroutine != null)
        {
            StopCoroutine(crossFadeAlphaCoroutine);
        }
        crossFadeAlphaCoroutine = StartCoroutine(CanvasGroupCrossFadeAlpha(parentMain, 0.0f, 0.25f));

        // disable the menu
        menuActive = false;
    }

    public void UpdateRadialMenu(int numOfButtons)
    {
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

            if (i < radialButtons.Count)
            {
                // reuse the base objects
                fill = radialButtons[i].fill;
                separator = radialButtons[i].separator;
                text = radialButtons[i].text;

                // reset values
                radialButtons[i].SetAngleRanges(angleRanges);
                radialButtons[i].Select(false);

                // fade in any buttons we're reusing
                radialButtons[i].MyCrossFadeAlpha(1.0f);
            }
            else
            {
                // duplicate the base objects 
                fill = Instantiate(baseFill, parentFill);
                separator = Instantiate(baseSeparator, parentSeparator);
                text = Instantiate(baseText, parentText);

                // add new buttons
                radialButtons.Add(new RadialButton(angleRanges, fill, separator, text));
                radialButtons[radialButtons.Count - 1].Select(false);
            }

            fill.rectTransform.eulerAngles = new Vector3(0.0f, 0.0f, angleBetween * i * -1.0f);
            fill.fillAmount = fillAmount;
            fill.CrossFadeAlpha(0.25f, 0.0f, true);
        }

        // fade out buttons that aren't currently being used
        for (int i = numOfButtons; i < radialButtons.Count; ++i)
        {
            radialButtons[i].MyCrossFadeAlpha(0.0f);
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

    private int GetNumOfButtons(Moveset moveset)
    {
        int numOfAbilities = 0;

        if (moveset.ability1 != null)
        {
            numOfAbilities++;
        }
        if (moveset.ability2 != null)
        {
            numOfAbilities++;
        }
        if (moveset.ability3 != null)
        {
            numOfAbilities++;
        }
        if (moveset.ability4 != null)
        {
            numOfAbilities++;
        }

        return numOfAbilities;
    }

    private IEnumerator CanvasGroupCrossFadeAlpha(CanvasGroup group, float alpha, float duration)
    {
        float passedTime = 0.0f;
        float endTime = Time.time + duration;

        while (Time.time < endTime)
        {
            passedTime += Time.deltaTime;

            group.alpha = Mathf.Lerp(group.alpha, alpha, passedTime / duration);

            yield return new WaitForEndOfFrame();
        }

        group.alpha = alpha;
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

        public void SetAngleRanges(List<Vector2> angles)
        {
            angleRanges = angles;
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
                fill.CrossFadeAlpha(0.85f, 0.1f, true);
            }
            // otherwise animate the button if it has been deselected
            else
            {
                fill.CrossFadeAlpha(0.25f, 0.1f, true);
            }
        }

        public void MyCrossFadeAlpha(float alpha)
        {
            fill.CrossFadeAlpha(0.0f, 0.0f, true);
            separator.CrossFadeAlpha(0.0f, 0.0f, true);
            text.CrossFadeAlpha(0.0f, 0.0f, true);
        }

        private List<Vector2> angleRanges; // we keep a list of possible angle ranges in case there angle crosses the 360/0 degree threshold
        public Image fill;
        public Image separator;
        public TextMeshProUGUI text;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RadialMenu : MonoBehaviour
{
    private bool menuActive = false;
    private Character currentTarget;
    private bool justEnabled = false;
    private bool mouseInCenter = false;

    [Header("Parent Transforms")]
    public RectTransform parentMain;
    private CanvasGroup parentMainCanvasGroup;
    public RectTransform parentFill;
    public RectTransform parentSeparator;
    public RectTransform parentText;

    [Header("My Canvas")]
    public Canvas myCanvas;

    [Header("Radii")]
    [Range(0.0f, 1.0f)]
    public float distanceInnerEdge; // this variable is between 0 and 1 because it was easier to convert all of the distance checking vectors to be between 0 and 1 than otherwise
    [Range(0.0f, 1.0f)]
    public float distanceOuterEdge; // this variable is between 0 and 1 because it was easier to convert all of the distance checking vectors to be between 0 and 1 than otherwise
    public float distanceFromCenterSeparator;
    public float distanceFromCenterText;

    [Header("Ability Box UI")]
    public RectTransform abilityBoxRectTransform;
    public CanvasGroup abilityBoxCanvasGroup;
    public TextMeshProUGUI abilityBoxText;
    private AbilityUIDefinition abilityBoxUIDefinition;
    private Coroutine crossFadeAlphaAbilityBoxCoroutine;

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
    public Image debugImage;

    public Vector2 FROM = new Vector2(0.5f, 1.0f);
    public Vector2 CENTER = new Vector2(0.5f, 0.5f);

    private float angle = 0.0f;

    private Coroutine crossFadeAlphaCoroutine;

    private void Start()
    {
        List<Vector2> tmp = new List<Vector2>();
        radialButtons.Add(new RadialButton(tmp, baseFill, baseSeparator, baseText));

        parentMain.TryGetComponent(out parentMainCanvasGroup);
        parentMainCanvasGroup.alpha = 0.0f;

        abilityBoxUIDefinition = new AbilityUIDefinition();
        abilityBoxCanvasGroup.alpha = 0.0f;
    }

    private void Update()
    {
        if (menuActive)
        {
            CalculateAngle();

            RadialButton newButton = GetCurrentButton();

            if (newButton == null)
            {
                if (currentButton != null)
                {
                    // deselect the current button
                    currentButton.Select(false);

                    // disable the ability box
                    if (crossFadeAlphaAbilityBoxCoroutine != null)
                    {
                        StopCoroutine(crossFadeAlphaAbilityBoxCoroutine);
                    }
                    crossFadeAlphaAbilityBoxCoroutine = StartCoroutine(CanvasGroupCrossFadeAlpha(abilityBoxCanvasGroup, 0.0f, 0.25f));
                }

                // update the current button
                currentButton = newButton;
            }
            // if the selected button has changed, update all the buttons
            else if (newButton != currentButton || justEnabled)
            {
                justEnabled = false;

                if (currentButton != null)
                {
                    // deselect the current button
                    currentButton.Select(false);
                }

                // update the current button
                currentButton = newButton;

                // select the new current button
                currentButton.Select(true);

                // update ability box
                if (crossFadeAlphaAbilityBoxCoroutine != null)
                {
                    StopCoroutine(crossFadeAlphaAbilityBoxCoroutine);
                }
                crossFadeAlphaAbilityBoxCoroutine = StartCoroutine(CanvasGroupCrossFadeAlpha(abilityBoxCanvasGroup, 1.0f, 0.25f));
                abilityBoxText.text = GetSelectedAbilityDefinition();
            }

            // also move the menu if there's a current target
            if (currentTarget != null)
            {
                float lerpSpeed = 0.25f;

                // move menu to target position
                Vector3 screenPos = Camera.main.WorldToScreenPoint(currentTarget.transform.position);
                screenPos -= new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0.0f);
                screenPos /= myCanvas.transform.localScale.x;
                parentMain.anchoredPosition = Vector3.Lerp(parentMain.anchoredPosition, screenPos, lerpSpeed);

                // move menu center to target position
                screenPos = RectTransformUtility.PixelAdjustPoint(parentMain.position, parentMain, myCanvas);
                screenPos = new Vector2(screenPos.x / Screen.width, screenPos.y / Screen.height);
                CENTER = Vector3.Lerp(CENTER, screenPos, lerpSpeed);
                FROM = new Vector2(CENTER.x, CENTER.y + 0.5f);
            }

            // move the ability box based on the position of the radial menu
            if (CENTER.y < 0.5f)
            {
                abilityBoxRectTransform.anchoredPosition = new Vector2(abilityBoxRectTransform.anchoredPosition.x, 300);
            }
            else
            {
                abilityBoxRectTransform.anchoredPosition = new Vector2(abilityBoxRectTransform.anchoredPosition.x, -350);
            }
        }
    }

    public int GetSelectedAbility()
    {
        if (Input.GetMouseButtonDown(0) && currentButton != null)
        {
            return radialButtons.IndexOf(currentButton);
        }

        return -1;
    }

    public int GetCurrentAbility()
    {
        return radialButtons.IndexOf(currentButton);
    }

    public bool GetMouseInCenter()
    {
        return mouseInCenter;
    }

    private string GetSelectedAbilityDefinition()
    {
        int tmp = -1;

        if (currentButton != null)
        {
            tmp = radialButtons.IndexOf(currentButton);
        }

        if (tmp != -1)
        {
            tmp++;

            switch(tmp)
            {
                case 1:
                {
                    return abilityBoxUIDefinition.strings1.details + " • " + abilityBoxUIDefinition.strings1.description;
                }
                case 2:
                {
                    return abilityBoxUIDefinition.strings2.details + " • " + abilityBoxUIDefinition.strings2.description;
                }
                case 3:
                {
                    return abilityBoxUIDefinition.strings3.details + " • " + abilityBoxUIDefinition.strings3.description;
                }
                case 4:
                {
                    return abilityBoxUIDefinition.strings4.details + " • " + abilityBoxUIDefinition.strings4.description;
                }
            }
        }

        return "Empty Ability Description, whoops";
    }

    private Vector2 GetScreenPositionOfRectTransform(RectTransform rectTransform)
    {
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 output = (Vector2)Camera.main.WorldToScreenPoint(rectTransform.position) - screenCenter;

        return output;
    }

    public void Enable(Character target)
    {
        int numOfButtons = GetNumOfButtons(target.moveset);

        currentTarget = target;

        if (numOfButtons > 0)
        {
            // start cross fading the canvas group
            if (crossFadeAlphaCoroutine != null)
            {
                StopCoroutine(crossFadeAlphaCoroutine);
            }
            crossFadeAlphaCoroutine = StartCoroutine(CanvasGroupCrossFadeAlpha(parentMainCanvasGroup, 1.0f, 0.25f));

            // enable the menu
            menuActive = true;

            // update the contents of the radial menu
            UpdateRadialMenu(numOfButtons, target.moveset);

            justEnabled = true;

            // set the ability definitions
            abilityBoxUIDefinition.SetDefinitions(target.moveset);
        }
    }

    public void Disable()
    {
        currentTarget = null;
        
        // start cross fading the canvas group
        if (crossFadeAlphaCoroutine != null)
        {
            StopCoroutine(crossFadeAlphaCoroutine);
        }
        crossFadeAlphaCoroutine = StartCoroutine(CanvasGroupCrossFadeAlpha(parentMainCanvasGroup, 0.0f, 0.25f));

        // disable the menu
        menuActive = false;
        mouseInCenter = false;

        // disable the ability box if it was enabled
        if (crossFadeAlphaAbilityBoxCoroutine != null)
        {
            StopCoroutine(crossFadeAlphaAbilityBoxCoroutine);
        }
        crossFadeAlphaAbilityBoxCoroutine = StartCoroutine(CanvasGroupCrossFadeAlpha(abilityBoxCanvasGroup, 0.0f, 0.25f));
    }

    public void UpdateRadialMenu(int numOfButtons, Moveset moveset)
    {
        float angleBetween = (360 / numOfButtons);
        float fillAmount = 1.0f / (float)numOfButtons;

        for (int i = 0; i < numOfButtons; ++i)
        {
            // calculate angles of this button
            float startingAngle = i * angleBetween;
            float endingAngle = startingAngle + angleBetween;
            float midAngle = startingAngle + (angleBetween * 0.5f);

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

            // adjust fill
            fill.rectTransform.eulerAngles = new Vector3(0.0f, 0.0f, angleBetween * i * -1.0f);
            fill.fillAmount = fillAmount;
            fill.CrossFadeAlpha(0.25f, 0.0f, true);

            // adjust separator
            separator.rectTransform.anchoredPosition = parentSeparator.anchoredPosition + (
                new Vector2(Mathf.Sin(Mathf.Deg2Rad * startingAngle), Mathf.Cos(Mathf.Deg2Rad * startingAngle)).normalized * distanceFromCenterSeparator
            );
            separator.rectTransform.eulerAngles = new Vector3(0.0f, 0.0f, angleBetween * i * -1.0f);

            // adjust text
            text.rectTransform.anchoredPosition = parentText.anchoredPosition + (
                new Vector2(Mathf.Sin(Mathf.Deg2Rad * midAngle), Mathf.Cos(Mathf.Deg2Rad * midAngle)).normalized * distanceFromCenterText
                );
            text.text = GetAbilityName(moveset, i);
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
        // check if the mouse is even within the range of the buttons themselves
        Vector2 mousePos = Input.mousePosition;
        mousePos = new Vector2(mousePos.x / Screen.width, mousePos.y / Screen.width); // dividing the mouse position's Y by the width here so that the distance is consistent on the X and Y axes
        Vector2 screenPos = RectTransformUtility.PixelAdjustPoint(parentMain.position, parentMain, myCanvas);
        screenPos = new Vector2(screenPos.x / Screen.width, screenPos.y / Screen.width); // dividing the screen position's Y by the width here so that the distance is consistent on the X and Y axes

        float distance = Vector2.Distance(mousePos, screenPos);
        //Debug.Log(distance);
        if (distance > distanceOuterEdge || distance < distanceInnerEdge)
        {
            // check if the mouse is in the center
            if (distance < distanceInnerEdge)
            {
                mouseInCenter = true;
            }
            else
            {
                mouseInCenter = false;
            }

            // if it's not, return null as no button is currently being moused over
            return null;
        }

        // loop through the current buttons and find which one has the current angle within it
        for (int i = 0; i < radialButtons.Count; ++i)
        {
            if (radialButtons[i].CheckIfAngleIsWithinRanges(angle))
            {
                return radialButtons[i];
            }
        }

        // if for some reason, the current angle isn't within any of the radial buttons' specified ranges, just return null
        return null;
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

    private string GetAbilityName(Moveset moveset, int index)
    {
        Ability tmp = null;

        switch(index)
        {
            case 0:
            {
                tmp = moveset.ability1;
                break;
            }
            case 1:
            {
                tmp = moveset.ability2;
                break;
            }
            case 2:
            {
                tmp = moveset.ability3;
                break;
            }
            case 3:
            {
                tmp = moveset.ability4;
                break;
            }
            default:
            {
                break;
            }
        }

        if (tmp != null)
        {
            return tmp.name;
        }
        else
        {
            return "Ability Name";
        }
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
                fill.CrossFadeAlpha(0.9f, 0.1f, true);
            }
            // otherwise animate the button if it has been deselected
            else
            {
                fill.CrossFadeAlpha(0.0f, 0.1f, true);
            }
        }

        public void MyCrossFadeAlpha(float alpha)
        {
            fill.CrossFadeAlpha(alpha, 0.0f, true);
            separator.CrossFadeAlpha(alpha, 0.0f, true);
            text.CrossFadeAlpha(alpha, 0.0f, true);
        }

        private List<Vector2> angleRanges; // we keep a list of possible angle ranges in case there angle crosses the 360/0 degree threshold
        public Image fill;
        public Image separator;
        public TextMeshProUGUI text;
    }
}
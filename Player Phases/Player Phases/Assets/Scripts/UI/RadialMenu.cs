using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RadialMenu : MonoBehaviour
{
    public RectTransform center;
    public float distanceFromCenterSeparator;
    public float distanceFromCenterText;

    private int currentAbilityNum;

    public Image baseSeparator;
    private List<Image> separators = new List<Image>();

    public TextMeshProUGUI baseTextMesh;
    private List<TextMeshProUGUI> textMeshes = new List<TextMeshProUGUI>();
    private List<string> abilityNames = new List<string>();

    public Image baseFill;
    private List<Image> fills = new List<Image>();

    private List<RadialButton> radialButtons = new List<RadialButton>();
    private RadialButton currentButton;

    public Moveset debugMoveset;

    private void Start()
    {
        separators.Add(baseSeparator);
        textMeshes.Add(baseTextMesh);
        fills.Add(baseFill);

        Enable(debugMoveset);
    }

    private void Update()
    {
        TestSelect();
    }

    public void Enable(Moveset moveset)
    {
        // reset ability number to display correctly
        ResetAbilityNum(moveset);

        MoveSeparators();
    }

    private void ResetAbilityNum(Moveset moveset)
    {
        // reset ability num to 0
        currentAbilityNum = 0;

        // reset list of ability names
        abilityNames.Clear();

        // reset list of buttons
        radialButtons.Clear();

        // count the number of abilities
        if (moveset.ability1 != null)
        {
            currentAbilityNum++;

            abilityNames.Add(moveset.ability1.name);
        }
        if (moveset.ability2 != null)
        {
            currentAbilityNum++;

            abilityNames.Add(moveset.ability2.name);
        }
        if (moveset.ability3 != null)
        {
            currentAbilityNum++;

            abilityNames.Add(moveset.ability3.name);
        }
        if (moveset.ability4 != null)
        {
            currentAbilityNum++;

            abilityNames.Add(moveset.ability4.name);
        }
    }

    private void MoveSeparators()
    {
        float separationAngle = 360.0f / currentAbilityNum;
        float startingAngle = 0.0f;

        // create additional separators and text if needed
        for (int i = 0; i < currentAbilityNum; ++i)
        {
            if (separators.Count <= i)
            {
                separators.Add(Instantiate(baseSeparator, baseSeparator.rectTransform.parent));
            }

            if (textMeshes.Count <= i)
            {
                textMeshes.Add(Instantiate(baseTextMesh, baseTextMesh.rectTransform.parent));
            }

            if (fills.Count <= i)
            {
                fills.Add(Instantiate(baseFill, baseFill.rectTransform.parent));
            }
        }

        // move and rotate separators to fit the different sections
        for (int i = 0; i < currentAbilityNum; ++i)
        {
            float currentAngle = separationAngle * i;

            separators[i].rectTransform.eulerAngles = new Vector3(0.0f, 0.0f, currentAngle);
            separators[i].rectTransform.anchoredPosition = Vector2.zero;
            separators[i].rectTransform.anchoredPosition += (Vector2)separators[i].rectTransform.up * distanceFromCenterSeparator;

            textMeshes[i].rectTransform.anchoredPosition += new Vector2(Mathf.Sin(Mathf.Deg2Rad * (currentAngle + (separationAngle / 2.0f))), Mathf.Cos(Mathf.Deg2Rad * (currentAngle + (separationAngle / 2.0f)))).normalized * distanceFromCenterText;
            if (abilityNames.Count <= currentAbilityNum)
            {
                textMeshes[i].text = abilityNames[i];
            }

            fills[i].rectTransform.eulerAngles = new Vector3(0.0f, 0.0f, currentAngle * -1);
            fills[i].fillAmount = separationAngle / 360.0f;

            radialButtons.Add(new RadialButton(currentAngle, currentAngle + separationAngle, fills[i], separators[i], textMeshes[i]));
        }
    }

    public void TestSelect()
    {
        float separationAngle = 360.0f / currentAbilityNum;
        float startingAngle = separationAngle / 2.0f;

        Vector2 mousePosition = new Vector2((Input.mousePosition.x / Screen.width) - 0.5f, (Input.mousePosition.y / Screen.height) - 0.5f).normalized;

        float angle = Mathf.Atan2(mousePosition.y, mousePosition.x) * Mathf.Rad2Deg;

        // top right quadrant
        if (angle < 90.0f && angle >= 0.0f)
        {
            angle = 90.0f - angle;
        }
        // bottom right quadrant
        else if (angle < 0.0f && angle >= -90.0f)
        {
            angle = (-90.0f - angle) + 180.0f;
        }
        // bottom left quadrant
        else if (angle < -90.0f && angle >= -180.0f)
        {
            angle = (-90 - angle) + 180;
        }
        // top left qudrant
        else if (angle >= 90.0f && angle <= 180.0f)
        {
            angle = (90 - angle) + 360;
        }

        RadialButton newButton = GetSelectedButton(angle, separationAngle, startingAngle);
        if (newButton != currentButton)
        {
            currentButton = newButton;
            UpdateButtons();
        }
    }

    private void UpdateButtons()
    {
        // enable selected button
        currentButton.fill.CrossFadeAlpha(1.0f, 0.1f, true);

        // disable other buttons
        for (int i = 0; i < radialButtons.Count; ++i)
        {
            if (currentButton != radialButtons[i])
            {
                radialButtons[i].fill.CrossFadeAlpha(0.0f, 0.1f, true);
            }
        }
    }

    private RadialButton GetSelectedButton(float angle, float separationAngle, float startingAngle)
    {
        for (int i = 0; i < radialButtons.Count; ++i)
        {
            if (CheckIfAngleIsWithin(angle, radialButtons[i].angleStart, radialButtons[i].angleEnd))
            {
                return radialButtons[i];
            }
        }

        return null;
    }

    private bool CheckIfAngleIsWithin(float angle, float a, float b)
    {
        if (angle > a && angle < b)
        {
            return true;
        }
        return false;
    }

    public void Disable()
    {

    }

    private class RadialButton
    {
        public RadialButton(float start, float end, Image f, Image s, TextMeshProUGUI t)
        {
            angleStart = start;
            angleEnd = end;

            if (angleEnd > 360)
            {
                angleEnd -= 360;
            }

            fill = f;
            separator = s;
            text = t;
        }

        public float angleStart;
        public float angleEnd;
        public Image fill;
        public Image separator;
        public TextMeshProUGUI text;
    }
}
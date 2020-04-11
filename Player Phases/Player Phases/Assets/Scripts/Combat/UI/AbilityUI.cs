using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AbilityUI : MonoBehaviour
{
    public Button buttonSelectAbility;
    public Image imageOverlay;
    public TextMeshProUGUI textMeshAbilityName;
    public TextMeshProUGUI textMeshDetails;
    public TextMeshProUGUI textMeshDescription;

    public RectTransform refRectTransform;

    public void SetActive(bool val)
    {
        buttonSelectAbility.gameObject.SetActive(val);
        textMeshAbilityName.gameObject.SetActive(val);
        textMeshDetails.gameObject.SetActive(val);
        textMeshDescription.gameObject.SetActive(val);
    }
}

[System.Serializable]
public class AbilityUIDefinition
{
    public AbilityUIStrings strings1;
    public AbilityUIStrings strings2;
    public AbilityUIStrings strings3;
    public AbilityUIStrings strings4;

    public AbilityUIDefinition()
    {
        strings1 = new AbilityUIStrings();
        strings2 = new AbilityUIStrings();
        strings3 = new AbilityUIStrings();
        strings4 = new AbilityUIStrings();
    }
}

public class AbilityUIStrings
{
    public string name;
    public string details;
    public string description;

    public AbilityUIStrings()
    {
        name = "";
        details = "";
        description = "";
    }
}
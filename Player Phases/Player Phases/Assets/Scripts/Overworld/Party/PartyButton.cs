using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using DG.Tweening;

public class PartyButton : MonoBehaviour
{
    [HideInInspector]
    public PartyMember myPartyMember;
    public Button myButton;
    public Image mySprite;
    public TextMeshProUGUI text;

    private float defaultY;

    public void SetDelegate(PartyMember partyMember, Party party, float yPos)
    {
        myPartyMember = partyMember;
        text.text = myPartyMember.loader.characterName;
        defaultY = yPos;

        DoTween(party.GetPartyMemberActive(myPartyMember));

        myButton.onClick.AddListener(delegate { party.TogglePartyMemberActivity(myPartyMember); DoTween(party.GetPartyMemberActive(myPartyMember)); });
    }

    private void DoTween(bool active)
    {
        float duration = 0.25f;

        Debug.Log("Active was " + active);

        if (active)
        {
            myButton.transform.DOMoveY(defaultY - 25, duration);
            myButton.transform.DOScale(1.1f, duration);
            mySprite.CrossFadeAlpha(1.0f, duration, true);
        }
        else
        {
            myButton.transform.DOMoveY(defaultY, duration);
            myButton.transform.DOScale(1.0f, duration);
            mySprite.CrossFadeAlpha(0.5f, duration, true);
        }
    }
}

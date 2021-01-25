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
    private float defaultScale;

    public void SetDelegate(PartyMember partyMember, Party party, float yPos, float scale)
    {
        myPartyMember = partyMember;
        text.text = myPartyMember.loader.characterName;
        defaultY = yPos;
        defaultScale = scale;

        DoTween(party.GetPartyMemberActive(myPartyMember));

        myButton.onClick.AddListener(delegate { party.TogglePartyMemberActivity(myPartyMember); DoTween(party.GetPartyMemberActive(myPartyMember)); });
    }

    private void DoTween(bool active)
    {
        float duration = 0.25f;

        if (active)
        {
            mySprite.rectTransform.DOAnchorPosY(defaultY - 25, duration);
            myButton.transform.DOScale(defaultScale * 1.1f, duration);
            mySprite.CrossFadeAlpha(1.0f, duration, true);
        }
        else
        {
            mySprite.rectTransform.DOAnchorPosY(defaultY, duration);
            myButton.transform.DOScale(defaultScale, duration);
            mySprite.CrossFadeAlpha(0.5f, duration, true);
        }
    }

    public RectTransform GetRectTransform()
    {
        return mySprite.rectTransform;
    }
}

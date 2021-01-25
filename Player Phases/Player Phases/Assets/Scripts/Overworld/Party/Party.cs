using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Party : MonoBehaviour
{
    [Header("The current active party members; they will appear in combat")]
    public List<PartyMember> partyActive;
    [Header("Party members in reserve; they will not appear in combat")]
    public List<PartyMember> partyReserve;

    const int MIN_ACTIVEPARTYMEMBERS = 1;
    const int MAX_ACTIVEPARTYMEMBERS = 4;

    [Header("Placeholder UI")]
    public PartyButton button;
    public UnityEngine.UI.Image image;
    private List<UnityEngine.UI.Image> images = new List<UnityEngine.UI.Image>();

    private void Start()
    {
        for (int i = 0; i < partyActive.Count; ++i)
        {
            partyActive[i].currentHealth = partyActive[i].loader.healthMax;
        }

        for (int i = 0; i < partyReserve.Count; ++i)
        {
            partyReserve[i].currentHealth = partyReserve[i].loader.healthMax;
        }

        float xPos = button.GetRectTransform().anchoredPosition.x;
        float yPos = button.GetRectTransform().anchoredPosition.y - 50.0f;
        float buttonScale = button.transform.localScale.x;
        float xPosImage = image.rectTransform.position.x;
        int counter = 0;
        for (int i = 0; i < partyActive.Count; ++i)
        {
            if (i == 0)
            {
                button.SetDelegate(partyActive[i], this, yPos, buttonScale);

                button.gameObject.name = "Party Button " + (counter + 1);

                image.gameObject.name = "Party Portrait " + (counter + 1);

                images.Add(image);
            }
            else
            {
                PartyButton tmp = Instantiate(button, new Vector2(button.GetRectTransform().anchoredPosition.x, 0), Quaternion.identity, button.transform.parent);
                tmp.SetDelegate(partyActive[i], this, yPos, buttonScale);

                tmp.gameObject.name = "Party Button " + (counter + 1);

                UnityEngine.UI.Image tmpImage = Instantiate(image, image.rectTransform.transform.position, Quaternion.identity, image.transform.parent);

                tmpImage.gameObject.name = "Party Portrait " + (counter + 1);

                images.Add(tmpImage);
            }

            xPos += 200;
            counter++;
        }

        for (int i = 0; i < partyReserve.Count; ++i)
        {
            PartyButton tmp = Instantiate(button, new Vector2(button.GetRectTransform().anchoredPosition.x, 0), Quaternion.identity, button.transform.parent);
            tmp.SetDelegate(partyReserve[i], this, yPos, buttonScale);

            tmp.gameObject.name = "Party Button " + (counter + 1);

            UnityEngine.UI.Image tmpImage = Instantiate(image, new Vector2(xPosImage, image.transform.position.y), Quaternion.identity, image.transform.parent);

            tmpImage.gameObject.name = "Party Portrait " + (counter + 1);

            images.Add(tmpImage);

            xPos += 200;
            xPosImage += 100;
            counter++;
        }
    }

    private void Update()
    {
        for (int i = 0; i < images.Count; ++i)
        {
            if (partyActive.Count > i && partyActive[i] != null)
            {
                images[i].sprite = partyActive[i].loader.portrait;
                images[i].CrossFadeAlpha(1.0f, 0.1f, true);
            }
            else
            {
                images[i].CrossFadeAlpha(0.0f, 0.1f, true);
            }
        }
    }

    public void SaveHealth(List<Character> characters)
    {
        for (int i = 0; i < partyActive.Count; ++i)
        {
            bool found = false;

            for (int j = 0; j < characters.Count; ++j)
            {
                if (partyActive[i].loader.characterName == characters[j].name)
                {
                    partyActive[j].currentHealth = characters[j].healthCurrent;
                    Debug.Log(partyActive[j].loader.characterName + "'s health was saved as " + partyActive[j].currentHealth + ".");

                    found = true;
                }
            }

            if (!found)
            {
                partyActive[i].currentHealth = partyActive[i].loader.healthMax / 3;
                //partyReserve.Add(partyActive[i]);
                //partyActive.RemoveAt(i);
                //--i;
            }
        }
    }

    public void TogglePartyMemberActivity(PartyMember partyMember)
    {
        if (partyMember != null)
        {
            if (partyActive.Contains(partyMember) && partyActive.Count > 1)
            {
                partyReserve.Add(partyMember);
                partyActive.Remove(partyMember);

                return;
            }

            if (partyReserve.Contains(partyMember) && partyActive.Count < 100)
            {
                partyActive.Add(partyMember);
                partyReserve.Remove(partyMember);

                return;
            }
        }
    }

    public bool GetPartyMemberActive(PartyMember partyMember)
    {
        if (partyActive.Contains(partyMember))
        {
            return true;
        }
        return false;
    }

    public PartyMember GetPartyMember(string name)
    {
        for (int i = 0; i < partyReserve.Count; ++i)
        {
            if (partyReserve[i].loader.characterName == name)
            {
                return partyReserve[i];
            }
        }

        return null;
    }
}

[System.Serializable]
public class PartyMember
{
    [Header("Character Definition to pass to the PlayerManager")]
    public CharacterDefinition loader;

    [HideInInspector]
    public int currentHealth;

    [HideInInspector]
    public int abilitiesUnlocked;

    // equipment piece "clothing"?
    // equipment piece "accessory"?
}
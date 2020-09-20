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

        float xPos = button.transform.position.x;
        for (int i = 0; i < partyActive.Count; ++i)
        {
            if (i == 0)
            {
                button.SetDelegate(partyActive[i], this, button.transform.position.y);
            }
            else
            {
                PartyButton tmp = Instantiate(button, new Vector2(xPos, button.transform.position.y), Quaternion.identity, button.transform.parent);
                tmp.SetDelegate(partyActive[i], this, button.transform.position.y);
            }

            xPos += 200;
        }

        for (int i = 0; i < partyReserve.Count; ++i)
        {
            PartyButton tmp = Instantiate(button, new Vector2(xPos, button.transform.position.y), Quaternion.identity, button.transform.parent);
            tmp.SetDelegate(partyReserve[i], this, button.transform.position.y);

            xPos += 200;
        }
    }

    public void SaveHealth(List<PlayerBase> characters)
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
                partyReserve.Add(partyActive[i]);
                partyActive.RemoveAt(i);
                --i;
            }
        }
    }

    public void TogglePartyMemberActivity(PartyMember partyMember)
    {
        if (partyActive.Contains(partyMember))
        {
            partyReserve.Add(partyMember);
            partyActive.Remove(partyMember);

            return;
        }

        if (partyReserve.Contains(partyMember))
        {
            partyActive.Add(partyMember);
            partyReserve.Remove(partyMember);

            return;
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
}

[System.Serializable]
public class PartyMember
{
    [Header("Character Definition to pass to the PlayerManager")]
    public CharacterDefinition loader;

    [HideInInspector]
    public int currentHealth;

    // equipment piece "clothing"?
    // equipment piece "accessory"?
}
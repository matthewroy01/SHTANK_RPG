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
}

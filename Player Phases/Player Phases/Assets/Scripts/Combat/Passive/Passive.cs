using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Passive : MonoBehaviour
{
    // main function for listening to events
    public virtual void ReceiveEvent(PassiveEventID id) { }
    public virtual void ReceiveEvent<T>(PassiveEventID id, T param) { }
}

public enum PassiveEventID
{
    combatStart, combatEnd,
    select, deselect,
    turnStart, turnEnd,
    abilityUse1, abilityUse2, abilityUse3, abilityUse4,
    receiveDamage, receiveHealing, receiveStatus,
    dealDamage, dealHealing, dealStatus,
    storeGridSpace
}
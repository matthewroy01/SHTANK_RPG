using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Passive : MonoBehaviour
{
    [HideInInspector]
    public Character myCharacter;

    [Header("Passive Specific Stat Changes")]
    public PassiveStatBoost boostAttack;
    public PassiveStatBoost boostDefense;

    // main function for listening to events
    public virtual void ReceiveEvent(PassiveEventID id) { }
    public virtual void ReceiveEvent<T>(PassiveEventID id, T param) { }

    // function getting any active status effects
    public virtual List<StatusUIDefinition> GetActiveStatuses() { return new List<StatusUIDefinition>(); }

    public int GetAttackBoost(int damage)
    {
        return GetBoost(damage, boostAttack.op, boostAttack.boost);
    }

    public int GetDefenseBoost(int damage)
    {
        return GetBoost(damage, boostDefense.op, boostDefense.boost);
    }

    private int GetBoost(int damage, PassiveStatOperator op, float booster)
    {
        switch (op)
        {
            case PassiveStatOperator.multiplication:
            {
                return (int)(damage * booster);
            }
            case PassiveStatOperator.addition:
            {
                return (int)(damage + booster);
            }
            case PassiveStatOperator.subtraction:
            {
                return (int)(damage - booster);
            }
            case PassiveStatOperator.division:
            {
                return (int)(damage / booster);
            }
        }

        return damage;
    }
}

public enum PassiveEventID
{
    combatStart, combatEnd,
    select, deselect,
    turnStart, turnEnd,
    abilityUse1, abilityUse2, abilityUse3, abilityUse4,
    receiveDamage, receiveHealing, receiveStatus,
    dealDamage, dealHealing, dealStatus,
    dealDamageNotFriendly, dealDamageFriendly,
    storeGridSpace
}

public enum PassiveStatOperator
{
    multiplication,
    addition,
    subtraction,
    division
}

[System.Serializable]
public class PassiveStatBoost
{
    [HideInInspector]
    public float boost = 1.0f;
    public PassiveStatOperator op;
}
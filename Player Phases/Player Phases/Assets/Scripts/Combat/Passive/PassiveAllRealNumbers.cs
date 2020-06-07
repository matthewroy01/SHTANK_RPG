using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveAllRealNumbers : Passive
{
    [Header("Curriculum Passive")]
    public int curriculumMax;
    private int curriculumCurrent;
    public int addAttackForEvery;

    public override void ReceiveEvent(PassiveEventID id)
    {
        switch(id)
        {
            case PassiveEventID.dealDamageNotFriendly:
            {
                // increase curriculum when dealing damage
                CurriculumIncrease();
                break;
            }
            case PassiveEventID.receiveDamage:
            {
                // reset curriculum when receiving damage
                CurriculumDecrease();
                break;
            }
            case PassiveEventID.abilityUse3:
            {
                // reset curriculum when using Divide By Zero
                CurriculumReset();
                break;
            }
        }
    }

    private void CurriculumIncrease()
    {
        // increment curriculum if it's not already the max
        if (curriculumCurrent + 1 < curriculumMax)
        {
            curriculumCurrent++;
        }

        UpdateAttackMod();
    }

    private void CurriculumDecrease()
    {
        // decrement curriculum if it's greater than 0
        if (curriculumCurrent - 1 >= 0)
        {
            curriculumCurrent--;
        }

        UpdateAttackMod();
    }

    private void UpdateAttackMod()
    {
        // decrease the attack modifider based on the amount of curriculum
        myCharacter.attackMod = curriculumCurrent / addAttackForEvery;
    }

    private void CurriculumReset()
    {
        // reset curriculum
        curriculumCurrent = 0;
        myCharacter.attackMod = 0;
    }
}

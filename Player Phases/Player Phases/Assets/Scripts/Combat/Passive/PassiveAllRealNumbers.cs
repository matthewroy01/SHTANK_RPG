using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveAllRealNumbers : Passive
{
    [Header("Curriculum Passive")]
    public int curriculumMax;
    private int curriculumCurrent;
    public int addAttackForEvery;

    [Header("Curriculum Status UI")]
    public StatusUIDefinition curriculumStatus0;
    public StatusUIDefinition curriculumStatus1;
    public StatusUIDefinition curriculumStatus2;

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
    }

    private void CurriculumDecrease()
    {
        // decrement curriculum if it's greater than 0
        if (curriculumCurrent - 1 >= 0)
        {
            curriculumCurrent--;
        }
    }

    private void CurriculumReset()
    {
        // reset curriculum
        curriculumCurrent = 0;
        myCharacter.attackMod = 0;
    }

    public override List<StatusUIDefinition> GetActiveStatuses()
    {
        List<StatusUIDefinition> statuses = new List<StatusUIDefinition>();

        switch(curriculumCurrent / addAttackForEvery)
        {
            case 1:
            {
                statuses.Add(curriculumStatus1);
                break;
            }
            case 2:
            {
                statuses.Add(curriculumStatus2);
                break;
            }
            default:
            {
                // no curriculum active
                statuses.Add(curriculumStatus0);
                break;
            }
        }

        return statuses;
    }
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveAllRealNumbers : Passive
{
    [Header("Curriculum Passive")]
    public int curriculumMax;
    public int curriculumCurrent;
    private int increase = 0;
    public int addAttackForEvery;

    [Header("Curriculum Status UI")]
    public StatusUIDefinition curriculumStatus0;
    public StatusUIDefinition curriculumStatus1;
    public StatusUIDefinition curriculumStatus2;

    [Space]
    public Color curriculumColor;

    [Header("UI Effects For Curriculum Increasing")]
    public ManagedAudio curriculumSoundInc;
    public string curriculumMessageInc;

    [Header("UI Effects For Curriculum Decreasing")]
    public ManagedAudio curriculumSoundDec;
    public string curriculumMessageDec;

    [Header("Particles")]
    public ParticleSystem particles;
    public float rate0;
    public float rate1;
    public float rate2;

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
                // decrease curriculum when receiving damage
                CurriculumDecrease();
                UpdateAttackBoost();
                break;
            }
            case PassiveEventID.abilityUse3:
            {
                // reset curriculum when using Divide By Zero
                CurriculumDecrease();
                CurriculumDecrease();
                break;
            }
            case PassiveEventID.turnEnd:
            {
                // update the attack boost value when the turn ends
                UpdateAttackBoost();
                break;
            }
        }
    }

    private void CurriculumIncrease()
    {
        // increment curriculum if it's not already the max
        if (curriculumCurrent + increase < curriculumMax)
        {
            increase++;
        }
    }

    private void CurriculumDecrease()
    {
        // decrement curriculum if it's greater than 0
        if (curriculumCurrent + increase > 0)
        {
            increase--;
        }
    }

    private void CurriculumReset()
    {
        // reset curriculum
        curriculumCurrent = 0;

        UpdateAttackBoost();
    }

    private void UpdateAttackBoost()
    {
        curriculumCurrent += increase;

        if (increase > 0)
        {
            for (int i = 0; i < increase; ++i)
            {
                myCharacter.refCharacterEffectUI.AddEffectCustom(curriculumMessageInc, curriculumSoundInc, curriculumColor);
            }
        }
        else if (increase < 0)
        {
            for (int i = 0; i < Mathf.Abs(increase); ++i)
            {
                myCharacter.refCharacterEffectUI.AddEffectCustom(curriculumMessageDec, curriculumSoundDec, curriculumColor);
            }
        }

        increase = 0;

        ParticleSystem.EmissionModule tmp = particles.emission;

        switch (curriculumCurrent)
        {
            case 2:
            {
                boostAttack.boost = 2.0f;

                tmp.rateOverTime = rate2;
                break;
            }
            case 1:
            {
                boostAttack.boost = 1.5f;

                tmp.rateOverTime = rate1;
                break;
            }
            default:
            {
                boostAttack.boost = 1.0f;

                tmp.rateOverTime = rate0;
                break;
            }
        }
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

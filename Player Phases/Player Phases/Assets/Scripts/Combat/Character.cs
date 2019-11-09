using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    // stats
    /* ----------------------------------------------------------*/

    public int healthMax;
    public int healthCurrent;

    // defensive modifier, reduces damage taken
    public int defenseMod;
    // offenseive modifier, increases damage dealt
    public int attackMod;

    // nashbalm stat, increases chance of counter attack
    public int nashbalm;

    /* ----------------------------------------------------------*/

    public GridSpace myGridSpace;

    public void ApplyEffect(Effect effect)
    {
        Debug.Log(gameObject.name + " receives effect of type " + effect.id + "!");

        switch(effect.id)
        {
            case Effect_ID.damage:
            {
                if (Random.Range(0.0f, 1.0f) <= effect.probability)
                {
                    healthCurrent -= effect.value;
                }
                break;
            }
            case Effect_ID.healing:
            {
                if (Random.Range(0.0f, 1.0f) <= effect.probability)
                {
                    healthCurrent += effect.value;
                }
                break;
            }
            case Effect_ID.paralysis:
            {
                break;
            }
            case Effect_ID.poison:
            {
                break;
            }
            case Effect_ID.burn:
            {
                break;
            }
        }
    }
}
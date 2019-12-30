using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    // stats
    /* ----------------------------------------------------------*/

    [Header("Stats")]
    public int healthMax;
    [HideInInspector]
    public int healthCurrent;

    // defensive modifier, reduces damage taken
    public int defenseMod;
    // offenseive modifier, increases damage dealt
    public int attackMod;

    // nashbalm stat, increases chance of counter attack
    [Range(0.0f, 100.0f)]
    public int nashbalm;

    // movement range
    public uint movementRange;

    /* ----------------------------------------------------------*/

    [Header("Affiliation")]
    public Character_Affiliation affiliation;

    [Header("Navigable Terrain")]
    public List<GridSpace_TerrainType> terrainTypes = new List<GridSpace_TerrainType>();
    public List<GridSpace> movementSpaces = new List<GridSpace>();

    public GridSpace myGridSpace;

    private EffectUI refCharacterEffectUI;

    public void ApplyEffect(Effect effect)
    {
        if (refCharacterEffectUI == null)
        {
            refCharacterEffectUI = GetComponent<EffectUI>();
        }

        switch (effect.id)
        {
            case Effect_ID.damage:
            {
                if (Random.Range(0.0f, 1.0f) <= effect.probability && effect.source != this)
                {
                    Debug.Log(gameObject.name + " receives effect of type " + effect.id + "!");

                    if (healthCurrent - effect.value < 0)
                    {
                        healthCurrent = 0;
                    }
                    else
                    {
                        healthCurrent -= effect.value;
                    }

                    refCharacterEffectUI.AddEffect(effect);

                    if (GetType().Name == "EnemyBase")
                    {
                        ((EnemyBase)this).ApplyAggro(effect.source, 1);
                    }
                }
                break;
            }
            case Effect_ID.healing:
            {
                if (Random.Range(0.0f, 1.0f) <= effect.probability)
                {
                    Debug.Log(gameObject.name + " receives effect of type " + effect.id + "!");

                    if (healthCurrent + effect.value > 0)
                    {
                        healthCurrent = healthMax;
                    }
                    else
                    {
                        healthCurrent += effect.value;
                    }

                    refCharacterEffectUI.AddEffect(effect);
                }
                break;
            }
            case Effect_ID.aggro:
            {
                if (Random.Range(0.0f, 1.0f) <= effect.probability)
                {
                    Debug.Log(gameObject.name + " receives effect of type " + effect.id + "!");

                    refCharacterEffectUI.AddEffect(effect);

                    if (GetType().Name == "EnemyBase")
                    {
                        ((EnemyBase)this).ApplyAggro(effect.source, effect.value);
                    }
                }
                break;
            }
            case Effect_ID.paralysis:
            {
                if (Random.Range(0.0f, 1.0f) <= effect.probability && effect.source != this)
                {
                    Debug.Log(gameObject.name + " receives effect of type " + effect.id + "!");

                    // inflict paralysis

                    refCharacterEffectUI.AddEffect(effect);
                }
                break;
            }
            case Effect_ID.poison:
            {
                if (Random.Range(0.0f, 1.0f) <= effect.probability && effect.source != this)
                {
                    Debug.Log(gameObject.name + " receives effect of type " + effect.id + "!");

                    // inflict poison
                }
                break;
            }
            case Effect_ID.burn:
            {
                if (Random.Range(0.0f, 1.0f) <= effect.probability && effect.source != this)
                {
                    Debug.Log(gameObject.name + " receives effect of type " + effect.id + "!");

                    // inflict burn
                }
                break;
            }
        }
    }
}
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

    private bool dead = false;

    // defensive modifier, reduces damage taken
    public int defenseMod;
    // offenseive modifier, increases damage dealt
    public int attackMod;

    // nashbalm stat, increases chance of counter attack
    [Range(0.0f, 100.0f)]
    public int nashbalm;

    // movement range
    public uint movementRangeDefault;
    [HideInInspector]
    public uint movementRangeCurrent;

    // statuses
    /* ----------------------------------------------------------*/

    [Header("Statuses")]
    public bool statusFrosty = false;
    public bool statusAttackUp = false;

    /* ----------------------------------------------------------*/

    [Header("Affiliation")]
    public Character_Affiliation affiliation;

    [Header("Navigable Terrain")]
    public List<GridSpace_TerrainType> terrainTypes = new List<GridSpace_TerrainType>();
    public List<GridSpace> movementSpaces = new List<GridSpace>();

    [Header("UI Character Portrait")]
    public Sprite portrait;

    public GridSpace myGridSpace;

    private EffectUI refCharacterEffectUI;

    [HideInInspector]
    public AbilityUIDefinition abilityUIDefinition;

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
                        FindObjectOfType<EnemyManager>().AlertAllEnemies(effect.source, (EnemyBase)this);
                    }

                    if (healthCurrent == 0)
                    {
                        dead = true;
                    }
                }
                break;
            }
            case Effect_ID.healing:
            {
                if (Random.Range(0.0f, 1.0f) <= effect.probability)
                {
                    Debug.Log(gameObject.name + " receives effect of type " + effect.id + "!");

                    if (healthCurrent + effect.value > healthMax)
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
                        FindObjectOfType<EnemyManager>().AlertAllEnemies(effect.source, (EnemyBase)this);
                    }
                }
                break;
            }
            case Effect_ID.frosty:
            {
                if (Random.Range(0.0f, 1.0f) <= effect.probability)
                {
                    Debug.Log(gameObject.name + " receives effect of type " + effect.id + "!");

                    // inflict frosty
                    statusFrosty = true;

                    refCharacterEffectUI.AddEffect(effect);
                }
                break;
            }
            case Effect_ID.aggroDispel:
            {
                if (Random.Range(0.0f, 1.0f) <= effect.probability && effect.source != this)
                {
                    Debug.Log(gameObject.name + " receives effect of type " + effect.id + "!");

                    // inflict aggro dispel
                    EnemyManager tmp = FindObjectOfType<EnemyManager>();
                    if (tmp != null)
                    {
                        tmp.DispelAggroFromTarget(this, effect.value);
                    }

                    refCharacterEffectUI.AddEffect(effect);
                }
                break;
            }
            case Effect_ID.attackUp:
            {
                if (Random.Range(0.0f, 1.0f) <= effect.probability)
                {
                    // apply attack up
                    statusAttackUp = true;

                    refCharacterEffectUI.AddEffect(effect);
                }
                break;
            }
        }
    }

    public void HandleStatuses()
    {
        if (statusFrosty)
        {
            movementRangeCurrent = movementRangeDefault - 1;
        }

        if (statusAttackUp)
        {
            attackMod = 1;
        }
    }

    public bool GetDead()
    {
        return dead;
    }

    private bool CheckValidAffiliation(Effect effect, bool shouldShare)
    {
        if (shouldShare)
        {
            if (effect.source.affiliation == affiliation)
            {
                return true;
            }

            return false;
        }
        else
        {
            if (effect.source.affiliation != affiliation)
            {
                return true;
            }

            return false;
        }
    }
}
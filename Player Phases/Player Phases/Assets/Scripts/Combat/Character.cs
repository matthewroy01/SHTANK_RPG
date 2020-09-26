using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public abstract class Character : MonoBehaviour
{
    public SpriteRenderer placeholderRenderer;

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

    // moveset
    /* ----------------------------------------------------------*/

    [Header("Moveset")]
    public Moveset moveset;
    [HideInInspector]
    public MovesetData movesetData;
    public Passive passive;

    // statuses
    /* ----------------------------------------------------------*/

    [Header("Statuses")]
    public bool statusFrosty = false;
    public bool statusToasty = false;
    public bool statusHoneyed = false;
    public bool statusAttackUp = false;

    /* ----------------------------------------------------------*/

    [Header("Affiliation")]
    public Character_Affiliation affiliation;

    [Header("Navigable Terrain")]
    public List<GridSpace_TerrainType> terrainTypes = new List<GridSpace_TerrainType>();
    public List<GridSpace> movementSpaces = new List<GridSpace>();

    [Header("UI Character Portrait")]
    public Sprite portrait;
    [HideInInspector]
    public string characterDescription;

    public GridSpace myGridSpace;

    [HideInInspector]
    public EffectUI refCharacterEffectUI;

    [HideInInspector]
    public AbilityUIDefinition abilityUIDefinition;
    protected MovementDialogueProcessor refMovementDialogueProcessor;

    protected bool idle = true;

    public void StartApplyEffect(Effect effect, bool counterable)
    {
        StartCoroutine(ApplyEffect(effect, counterable));
    }

    private IEnumerator ApplyEffect(Effect effect, bool counterable)
    {
        switch (effect.id)
        {
            case Effect_ID.damage:
            {
                if (effect.source != this)
                {
                    Debug.Log(gameObject.name + " receives effect of type " + effect.id + "!");

                    if (Random.Range(0.0f, 1.0f) <= effect.probability)
                    {
                        int modValue = effect.value;

                        modValue = GetTrueDamage(effect.value, effect);
                        /*// calculate attack modifiers
                        if (!effect.trueDamage)
                        {
                            // apply attack mod
                            modValue += effect.source.attackMod;

                            // apply passive attack modifiers if any are present
                            if (effect.source.passive != null)
                            {
                                modValue = effect.source.passive.GetAttackBoost(modValue);
                            }
                        }

                        // calculate defense modifiers
                        if (!effect.pierceDefense)
                        {
                            // apply defense mod
                            if (modValue - defenseMod > 0)
                            {
                                modValue -= defenseMod;
                            }
                            else
                            {
                                modValue = 0;
                            }

                            // apply passive defense modifiers if any are present
                            if (passive != null)
                            {
                                modValue = passive.GetDefenseBoost(modValue);
                            }
                        }*/

                        // check nashblam for counterattacks
                        if (counterable && Random.Range(0.0f, 100.0f) <= nashbalm)
                        {
                            // perform a counterattack!
                            Effect counterAttack = new Effect(effect, this);
                            effect.source.StartApplyEffect(counterAttack, false);

                            if (refMovementDialogueProcessor != null)
                            {
                                refMovementDialogueProcessor.DisplayCounterAttackQuote();
                            }

                            yield return new WaitForSeconds(0.5f);

                            // if the counterattack succeeds and it kills the other character, don't bother applying any other effects
                            if (effect.source.healthCurrent <= 0)
                            {
                                yield break;
                            }
                        }

                        // actually deal the damage
                        // if the character's health was going to go below 0, set it to 0 instead
                        if (healthCurrent - modValue < 0)
                        {
                            healthCurrent = 0;
                        }
                        else
                        {
                            healthCurrent -= modValue;

                            SendEvent(PassiveEventID.receiveDamage);
                            if (affiliation != effect.source.affiliation)
                            {
                                effect.source.SendEvent(PassiveEventID.dealDamageNotFriendly);
                            }
                            else
                            {
                                effect.source.SendEvent(PassiveEventID.dealDamageFriendly);
                            }
                            effect.source.SendEvent(PassiveEventID.dealDamage);
                        }

                        // don't tween against a counterattack since we're probably already in the middle of a tween
                        if (counterable)
                        {
                            transform.DOPunchPosition((transform.position - effect.source.transform.position).normalized * 0.5f, 0.25f, 0, 0);
                        }

                        // if 0 damage was dealt, apply a special "no damage" effect, otherwise use the given effect
                        if (modValue > 0)
                        {
                            // we pass in the effect's value instead of the modifed value since the EffectUI script recalculates the true damage (because true damage also has to be calculated for the Ability Forecast)
                            refCharacterEffectUI.AddEffect(new Effect(Effect_ID.damage, effect.value, effect.source));
                        }
                        else
                        {
                            refCharacterEffectUI.AddEffect(new Effect(Effect_ID.noDamage, 0));
                        }

                        // for enemies, also apply some aggro and alert the other enemies
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
                    else
                    {
                        // the attack missed
                        refCharacterEffectUI.AddEffect(new Effect(Effect_ID.miss, 0));
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
                    if (GetType().Name == "EnemyBase")
                    {
                        Debug.Log(gameObject.name + " receives effect of type " + effect.id + "!");

                        refCharacterEffectUI.AddEffect(effect);

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
                    if (GetType().Name == "PlayerBase")
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

    public virtual void StartTurn(CombatGrid grid)
    {
        movesetData.Reset(moveset);

        idle = false;

        HandleStatuses();
        FindMovementSpaces(grid);

        // PASSIVE EVENT: BEGIN TURN
        SendEvent(PassiveEventID.turnStart);
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

    public abstract void Selected(CombatGrid combatGrid);

    public bool GetDead()
    {
        return dead;
    }

    public bool GetIdle()
    {
        return idle;
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

    public void FindMovementSpaces(CombatGrid grid)
    {
        // reset movement spaces
        movementSpaces.Clear();
        movementSpaces = grid.GetBreadthFirst(myGridSpace, movementRangeCurrent, terrainTypes, affiliation);

        // remove movement spaces occupied by other characters
        for (int i = 0; i < movementSpaces.Count; ++i)
        {
            if (movementSpaces[i].character != null && movementSpaces[i].character != this)
            {
                movementSpaces.RemoveAt(i);
                --i;
            }
        }
    }

    public IEnumerator Death()
    {
        if (refMovementDialogueProcessor != null)
        {
            refMovementDialogueProcessor.DisplayDeathQuote();
        }

        myGridSpace.character = null;
        myGridSpace = null;

        yield return new WaitForSeconds(2.0f);

        Destroy(gameObject);
    }

    public void SendEvent(PassiveEventID id)
    {
        if (passive != null)
        {
            passive.ReceiveEvent(id);
        }
    }

    public void SendEvent<T>(PassiveEventID id, T param)
    {
        if (passive != null)
        {
            passive.ReceiveEvent<T>(id, param);
        }
    }

    public int GetTrueDamage(int baseDamage, Effect effect)
    {
        int trueDamage = baseDamage;

        // add damage modifiers from the source of the damage
        if (!effect.trueDamage)
        {
            trueDamage += effect.source.attackMod;

            if (effect.source.passive != null)
            {
                trueDamage = effect.source.passive.GetAttackBoost(trueDamage);
            }
        }

        // add our defense modifiers
        if (!effect.pierceDefense)
        {
            trueDamage -= defenseMod;

            if (passive != null)
            {
                trueDamage = passive.GetDefenseBoost(trueDamage);
            }
        }

        return trueDamage;
    }
}
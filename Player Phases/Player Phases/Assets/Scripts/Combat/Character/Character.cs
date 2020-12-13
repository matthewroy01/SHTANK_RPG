using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public partial class Character : MonoBehaviour
{
    public SpriteRenderer placeholderRenderer;

    private CombatGrid refCombatGrid;

    // stats
    /* ----------------------------------------------------------*/

    [Header("Stats")]
    public int healthMax;
    [HideInInspector]
    public int healthCurrent;

    private bool dead = false;

    // defensive modifier, reduces damage taken
    public int defense;
    // offenseive modifier, increases damage dealt
    public int attack;

    // stagger stat, increases chance of staggering a foe when attacking
    [Range(0.0f, 100.0f)]
    public int stagger;

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
    public bool statusStunned = false;

    [Header("Stagger Status")]
    public int statusStagger = 0;
    private const int STAGGER_MAX = 2;
    public ParticleSystem stunnedParts;

    // aggro information for AI controlled characters
    /* ----------------------------------------------------------*/

    [HideInInspector]
    public List<AggroData> aggroData = new List<AggroData>();
    private GridSpace aggroTarget;

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
    public GridSpace originalGridSpace;

    [HideInInspector]
    public EffectUI refCharacterEffectUI;

    [HideInInspector]
    public AbilityUIDefinition abilityUIDefinition;
    [HideInInspector]
    public MovementDialogueProcessor refMovementDialogueProcessor;

    protected bool idle = true;
    [HideInInspector]
    public bool selected;

    private Tweener damageTween;

    private void Awake()
    {
        refCombatGrid = FindObjectOfType<CombatGrid>();
    }

    private void Update()
    {
        if (idle)
        {
            placeholderRenderer.color = Color.Lerp(placeholderRenderer.color, Color.Lerp(Color.white, Color.black, 0.8f), 0.1f);
        }
        else
        {
            placeholderRenderer.color = Color.Lerp(placeholderRenderer.color, Color.white, 0.1f);
        }
    }

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
                            damageTween.Kill();
                            damageTween = transform.DOPunchPosition((transform.position - effect.source.transform.position).normalized * 0.5f, 0.25f, 0, 0);
                        }

                        // if 0 damage was dealt, apply a special "no damage" effect, otherwise use the given effect
                        if (modValue > 0)
                        {
                            // we pass in the effect's value instead of the modifed value since the EffectUI script recalculates the true damage (because true damage also has to be calculated for the Ability Forecast)
                            refCharacterEffectUI.AddEffect(new Effect(Effect_ID.damage, effect.value, effect.source));

                            // apply stagger if the odds work out
                            if (Random.Range(0.0f, 100.0f) <= effect.source.stagger)
                            {
                                statusStagger++;
                            }

                            // if the amount of stagger is greater than the max, apply the stunned status
                            if (statusStagger >= STAGGER_MAX)
                            {
                                statusStunned = true;
                                statusStagger = 0;

                                refCharacterEffectUI.AddEffect(new Effect(Effect_ID.stunned, 0));
                                stunnedParts.Play();
                            }
                        }
                        else
                        {
                            refCharacterEffectUI.AddEffect(new Effect(Effect_ID.noDamage, 0));
                        }

                        // for enemies, also apply some aggro and alert the other enemies
                        if (affiliation == Character_Affiliation.enemy)
                        {
                            ApplyAggro(effect.source, 1);
                            FindObjectOfType<EnemyManager>().AlertAllEnemies(effect.source, this);
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
                    if (affiliation == Character_Affiliation.enemy)
                    {
                        Debug.Log(gameObject.name + " receives effect of type " + effect.id + "!");

                        refCharacterEffectUI.AddEffect(effect);

                        ApplyAggro(effect.source, effect.value);
                        FindObjectOfType<EnemyManager>().AlertAllEnemies(effect.source, this);
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
        switch(affiliation)
        {
            case Character_Affiliation.player:
            {
                movesetData.Reset(moveset);

                idle = false;
                originalGridSpace = myGridSpace;

                HandleStatuses();
                FindMovementSpaces(grid);

                // PASSIVE EVENT: BEGIN TURN
                SendEvent(PassiveEventID.turnStart);
                break;
            }
            case Character_Affiliation.enemy:
            case Character_Affiliation.ally:
            {
                movesetData.Reset(moveset);

                idle = false;
                originalGridSpace = myGridSpace;

                HandleStatuses();
                FindMovementSpaces(grid);

                // PASSIVE EVENT: BEGIN TURN
                SendEvent(PassiveEventID.turnStart);

                break;
            }
        }
    }

    public void HandleStatuses()
    {
        if (statusStunned)
        {
            movementRangeCurrent = 0;
        }
        else
        {
            movementRangeCurrent = movementRangeDefault;
        }

        if (statusFrosty)
        {
            movementRangeCurrent = movementRangeDefault - 1;
        }

        if (statusAttackUp)
        {
            attack = 1;
        }
    }

    public void Selected(CombatGrid combatGrid)
    {
        switch(affiliation)
        {
            case Character_Affiliation.player:
            {
                selected = true;

                // apply any statuses again in case they have been cured
                // STATUSES LIKE POISON, THAT APPLY DAMAGE WILL NEED A SPECIAL CASE SO THEY DON'T GET APPLIED EVERY TIME THE PLAYER IS SELECTED
                HandleStatuses();

                // refind the potential movement spaces in case another character has moved since the turn began
                FindMovementSpaces(combatGrid);

                if (refMovementDialogueProcessor != null)
                {
                    refMovementDialogueProcessor.Display();
                }
                break;
            }
            case Character_Affiliation.enemy:
            {
                // apply any statuses again in case they have been cured
                // STATUSES LIKE POISON, THAT APPLY DAMAGE WILL NEED A SPECIAL CASE SO THEY DON'T GET APPLIED EVERY TIME THE PLAYER IS SELECTED
                HandleStatuses();

                // refind the potential movement spaces in case another character has moved since the turn began
                FindMovementSpaces(refCombatGrid);

                break;
            }
        }
    }

    public void Deselected(CombatGrid combatGrid)
    {
        switch(affiliation)
        {
            case Character_Affiliation.player:
            {
                selected = false;

                if (refMovementDialogueProcessor != null)
                {
                    refMovementDialogueProcessor.Clear();
                }
                break;
            }
        }
    }

    public void MoveToGridSpace(GridSpace toMoveTo)
    {
        if (toMoveTo != null)
        {
            transform.DOMove(toMoveTo.obj.transform.position, 0.25f);
            myGridSpace = toMoveTo;
        }
    }

    public void MoveToGridSpacePath(List<GridSpace> spaces, GridSpace end)
    {
        if (spaces != null && spaces.Count > 1)
        {
            List<Vector3> positions = new List<Vector3>();

            for (int i = 0; i < spaces.Count; ++i)
            {
                positions.Add(spaces[i].obj.transform.position);

                if (spaces[i] == end)
                {
                    break;
                }
            }

            transform.DOPath(positions.ToArray(), 0.25f);
            myGridSpace = end;
        }
    }

    public void MoveToGridSpaceJump(GridSpace toMoveTo)
    {
        if (toMoveTo != null)
        {
            transform.DOJump(toMoveTo.obj.transform.position, 2.0f, 1, 0.25f);
            myGridSpace = toMoveTo;
        }
    }

    public bool TryMove(CombatDirection dir, CombatGrid grid)
    {
        bool result = false;

        GridSpace tmp = grid.TryMove(dir, myGridSpace, movementSpaces);

        if (tmp != myGridSpace)
        {
            result = true;
        }

        myGridSpace = tmp;
        transform.position = myGridSpace.obj.transform.position;

        return result;
    }

    public bool TryMoveAStar(CombatGrid grid, GridSpace target)
    {
        if (target != null && movementSpaces.Contains(target))
        {
            // try to path to the target GridSpace
            List<GridSpace> spaces;
            spaces = grid.GetAStar(grid, myGridSpace, target, this, true);

            // if a path was found and we are not already at the target, move there
            if (spaces.Count > 0 && myGridSpace != spaces[spaces.Count - 1])
            {
                myGridSpace = spaces[spaces.Count - 1];
                transform.position = myGridSpace.obj.transform.position;
                return true;
            }
        }

        return false;
    }

    public void SaveMyGridSpace()
    {
        // update combat grid with new position
        originalGridSpace.character = null;
        myGridSpace.character = this;
        originalGridSpace = myGridSpace;
    }

    public void EndTurn()
    {
        SaveMyGridSpace();

        selected = false;
        idle = true;

        statusStunned = false;

        if (refMovementDialogueProcessor != null)
        {
            //refMovementDialogueProcessor.Clear();
        }

        // PASSIVE EVENT: END TURN
        SendEvent(PassiveEventID.turnEnd);
    }

    public void ResetToDefaultPosition(GridSpace toReturnTo)
    {
        transform.position = toReturnTo.obj.transform.position;
        myGridSpace = toReturnTo;
    }

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
            trueDamage += effect.source.attack;

            if (effect.source.passive != null)
            {
                trueDamage = effect.source.passive.GetAttackBoost(trueDamage);
            }
        }

        // add our defense modifiers
        if (!effect.pierceDefense)
        {
            trueDamage -= defense;

            if (passive != null)
            {
                trueDamage = passive.GetDefenseBoost(trueDamage);
            }
        }

        if (trueDamage < 0)
        {
            trueDamage = 0;
        }

        return trueDamage;
    }
}

[System.Serializable]
public class AggroData
{
    public Character character;
    public int aggro;

    public AggroData(Character newCharacter, int value)
    {
        character = newCharacter;
        aggro = value;
    }

    public bool Contains(Character c)
    {
        if (character == c)
        {
            return true;
        }
        return false;
    }
}
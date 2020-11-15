using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class Character : MonoBehaviour
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

    private void Awake()
    {
        refCombatGrid = FindObjectOfType<CombatGrid>();
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
            {
                movesetData.Reset(moveset);

                idle = false;

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
        if (statusFrosty)
        {
            movementRangeCurrent = movementRangeDefault - 1;
        }

        if (statusAttackUp)
        {
            attackMod = 1;
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

    IEnumerator MoveAlongPath(List<GridSpace> path)
    {
        if (path.Count > 0)
        {
            GridSpace currentAlongPath = null;

            // move along the path
            for (int i = 0; i < path.Count; ++i)
            {
                // keep a current grid space updated so we can update the grid properly
                currentAlongPath = path[i];
                // update position for visuals
                transform.position = currentAlongPath.obj.transform.position;

                yield return new WaitForSeconds(0.1f);

                // if the current space along the path reached the end and we are in range, attack!
                if (i == path.Count - 1 && refCombatGrid.GetDistance(path[i], aggroTarget) == 1)
                {
                    Attack();
                    break;
                }
            }

            // properly reassign grid spaces
            if (currentAlongPath != null)
            {
                myGridSpace.character = null;
                myGridSpace = currentAlongPath;
                myGridSpace.character = this;
            }
        }

        idle = true;
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

    public void DoAI()
    {
        HandleStatuses();

        FindMovementSpaces(refCombatGrid);

        // select aggro target
        aggroTarget = ProcessAggro();

        GridSpace closestValidSpace = null; // a grid space within the list of valid movement spaces that is closest to the aggro target adjacent space
        List<GridSpace> path = new List<GridSpace>(); // the list to store our final movement path

        if (aggroTarget != myGridSpace)
        {
            Debug.Log(gameObject.name + "'s target was " + aggroTarget.character.name + "!");

            closestValidSpace = CheckClosestValidSpace(aggroTarget);

            // find the path to the found valid movement space
            if (closestValidSpace != null)
            {
                path = refCombatGrid.GetAStar(refCombatGrid, myGridSpace, closestValidSpace, this, true);
            }

            StartCoroutine(MoveAlongPath(path));
        }
        else
        {
            idle = true;
        }
    }

    private void CheckAdjacentSpace(GridSpace center, ref GridSpace currentClosest, params string[] directions)
    {
        int currentDistance = int.MaxValue;

        for (int i = 0; i < directions.Length; ++i)
        {
            GridSpace adjacent = (GridSpace)center.GetType().GetField(directions[i]).GetValue(center);
            int distance = refCombatGrid.GetAStar(refCombatGrid, adjacent, myGridSpace, this, true).Count;

            if (adjacent.character == null && terrainTypes.Contains(adjacent.GetTerrainType()))
            {
                if (distance < currentDistance)
                {
                    currentDistance = distance;

                    currentClosest = adjacent;
                }
                else if (distance == currentDistance)
                {
                    if (Random.Range(0, 2) == 1)
                    {
                        currentDistance = distance;

                        currentClosest = adjacent;
                    }
                }
            }
        }

        if (currentClosest == center)
        {
            currentClosest = center;
        }
    }

    private GridSpace CheckClosestValidSpace(GridSpace toCheckWith)
    {
        int lowest = int.MaxValue;
        GridSpace closest = null;

        if (toCheckWith != myGridSpace)
        {
            for (int i = 0; i < movementSpaces.Count; ++i)
            {
                int pathCount = refCombatGrid.GetAStar(refCombatGrid, toCheckWith, movementSpaces[i], this, true).Count;
                if (pathCount < lowest)
                {
                    lowest = pathCount;
                    closest = movementSpaces[i];
                }
            }
        }

        return closest;
    }

    public GridSpace ProcessAggro()
    {
        GridSpace result = null;

        while (result == null)
        {
            List<AggroData> aggroCandidatesHighest = new List<AggroData>();
            int highestAggro = int.MinValue;

            // first find the characters with the highest aggro values
            for (int i = 0; i < aggroData.Count; ++i)
            {
                if (aggroData[i].aggro > highestAggro)
                {
                    aggroCandidatesHighest.Clear();
                    aggroCandidatesHighest.Add(aggroData[i]);
                    highestAggro = aggroData[i].aggro;
                }
                else if (aggroData[i].aggro == highestAggro)
                {
                    aggroCandidatesHighest.Add(aggroData[i]);
                }
            }

            // if only one character has the highest aggro, this is our target
            if (aggroCandidatesHighest.Count == 1)
            {
                result = aggroCandidatesHighest[0].character.myGridSpace;
                return result;
            }

            // next, look for characters in range
            List<AggroData> aggroCandidatesInRange = new List<AggroData>();
            List<GridSpace> attackSpaces = refCombatGrid.GetBreadthFirst(myGridSpace, movementRangeCurrent, terrainTypes, Character_Affiliation.none);
            // add one extra range regardless of terrain type to cover player characters inside of impassable tiles
            attackSpaces.AddRange(refCombatGrid.GetBorder(attackSpaces));
            for (int i = 0; i < aggroCandidatesHighest.Count; ++i)
            {
                for (int j = 0; j < attackSpaces.Count; ++j)
                {
                    if (aggroCandidatesHighest[i].character == attackSpaces[j].character && (movementSpaces.Contains(attackSpaces[j]) || refCombatGrid.GetBorder(movementSpaces).Contains(attackSpaces[j])))
                    {
                        aggroCandidatesInRange.Add(aggroCandidatesHighest[i]);
                    }
                }
            }

            // if only one character is in range, this is our target
            if (aggroCandidatesInRange.Count == 1)
            {
                result = aggroCandidatesInRange[0].character.myGridSpace;
                return result;
            }
            // if no characters were in range, look for the character with the lowest bulk and we'll move towards them
            else if (aggroCandidatesInRange.Count == 0)
            {
                result = GetTargetWithLowestBulk(aggroData);
                return result;
            }
            // if multiple characters were in range, the one with the lowest bulk is our target
            else
            {
                result = GetTargetWithLowestBulk(aggroCandidatesInRange);
                return result;
            }
        }

        return result;
    }

    private GridSpace GetTargetWithLowestBulk(List<AggroData> list)
    {
        List<AggroData> aggroCandidatesLowestBulk = new List<AggroData>();
        int lowestBulk = int.MaxValue;

        // find the characters with the lowest bulk
        for (int i = 0; i < list.Count; ++i)
        {
            if (list[i].character.healthCurrent + list[i].character.defenseMod < lowestBulk)
            {
                aggroCandidatesLowestBulk.Clear();
                aggroCandidatesLowestBulk.Add(list[i]);
                lowestBulk = list[i].character.healthCurrent + list[i].character.defenseMod;
            }
            else if (list[i].character.healthCurrent + list[i].character.defenseMod == lowestBulk)
            {
                aggroCandidatesLowestBulk.Add(list[i]);
            }
        }

        // if only one character had the lowest bulk, this is our target
        if (aggroCandidatesLowestBulk.Count == 1)
        {
            return aggroCandidatesLowestBulk[0].character.myGridSpace;
        }
        // otherwise randomly select one
        else if (aggroCandidatesLowestBulk.Count > 1)
        {
            return aggroCandidatesLowestBulk[Random.Range(0, aggroCandidatesLowestBulk.Count)].character.myGridSpace;
        }
        else
        {
            return myGridSpace;
        }
    }

    public void ApplyAggro(Character source, int amount)
    {
        for (int i = 0; i < aggroData.Count; ++i)
        {
            if (aggroData[i].character == source)
            {
                if (aggroData[i].aggro + amount > 10)
                {
                    aggroData[i].aggro = 10;
                }
                else if (aggroData[i].aggro + amount < 0)
                {
                    aggroData[i].aggro = 0;
                }
                else
                {
                    aggroData[i].aggro += amount;
                }
            }
        }
    }

    private AggroData GetAggroCandidateFromList(Character c)
    {
        for (int i = 0; i < aggroData.Count; ++i)
        {
            if (aggroData[i].character == c)
            {
                return aggroData[i];
            }
        }

        return null;
    }

    public void DispelAggroFromTarget(Character target, int amount)
    {
        for (int i = 0; i < aggroData.Count; ++i)
        {
            // also check if aggro was already 0, since we shouldn't reset aggro to 1 if it was 0
            if (aggroData[i].character == target && aggroData[i].aggro > 0)
            {
                aggroData[i].aggro -= amount;

                if (aggroData[i].aggro < 1)
                {
                    aggroData[i].aggro = 1;
                }
            }
        }
    }

    private void Attack()
    {
        if (moveset != null)
        {
            // apply source to basic attack for things like aggro and friendly fire
            moveset.ability1.ApplySourceInfo(this);

            // make the grid space dirty
            refCombatGrid.MakeDirty(aggroTarget, moveset.ability1);

            // clean the grid to actually perform the attack
            refCombatGrid.CleanGrid();
        }
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
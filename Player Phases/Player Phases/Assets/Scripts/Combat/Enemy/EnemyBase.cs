using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : Character
{
    private bool idle = true;

    private CombatGrid refCombatGrid;

    public List<AggroData> aggroData = new List<AggroData>();
    private GridSpace aggroTarget;

    [Header("Basic melee attack for testing")]
    public Ability basicAttack;

    private void Start()
    {
        refCombatGrid = FindObjectOfType<CombatGrid>();

        healthCurrent = healthMax;
    }

    public void StartTurn()
    {
        idle = false;
    }

    public void DoAI()
    {
        aggroTarget = ProcessAggro();

        List<GridSpace> path = refCombatGrid.GetAStar(refCombatGrid, myGridSpace, aggroTarget);
        StartCoroutine(MoveAlongPath(path));
    }

    private GridSpace ProcessAggro()
    {
        List<AggroData> aggroCandidates = new List<AggroData>();

        // valid aggro candidates are either in range or have built up some kind of aggro
        for (int i = 0; i < aggroData.Count; ++i)
        {
            if (aggroData[i].aggro > 0 || refCombatGrid.GetDistance(myGridSpace, aggroData[i].character.myGridSpace) <= movementRange)
            {
                aggroCandidates.Add(aggroData[i]);
            }
        }

        // if there are no valid aggro candidates, simply don't move
        if (aggroCandidates.Count == 0)
        {
            return myGridSpace;
        }

        List<AggroData> highestAggro = new List<AggroData>();
        highestAggro.Add(aggroCandidates[0]);

        // find the aggro data with the highest aggro value
        for(int i = 0; i < aggroCandidates.Count; ++i)
        {
            // if a new highest aggro is found, clear the list and add the new character
            if (aggroCandidates[i].aggro > highestAggro[0].aggro)
            {
                highestAggro.Clear();
                highestAggro.Add(aggroCandidates[i]);
            }
            // if the aggros are equal, add it to the list as well
            else if (aggroCandidates[i].aggro == highestAggro[0].aggro && !highestAggro.Contains(aggroCandidates[i]))
            {
                highestAggro.Add(aggroCandidates[i]);
            }
        }

        // if only one was found, return the corresponding grid space
        if (highestAggro.Count == 1)
        {
            return highestAggro[0].character.myGridSpace;
        }

        // if more than one was found, look for the character with the lowest overall bulk
        List<AggroData> weakest = new List<AggroData>();
        int bulkLowest = int.MaxValue;

        for (int i = 0; i < highestAggro.Count; ++i)
        {
            int bulkNew = highestAggro[i].character.healthCurrent + highestAggro[i].character.defenseMod;

            // if a new lowest bulk is found, clear the list and add the new character
            if (bulkNew < bulkLowest)
            {
                weakest.Clear();
                bulkLowest = bulkNew;
                weakest.Add(highestAggro[i]);
            }
            // if the bulks are equal, add it to the list as well
            else if (bulkNew == bulkLowest)
            {
                weakest.Add(highestAggro[i]);
            }
        }

        // at this point if bulks are the same, pick one at random
        return weakest[Random.Range(0, weakest.Count)].character.myGridSpace;
    }

    IEnumerator MoveAlongPath(List<GridSpace> path)
    {
        if (path.Count > 0)
        {
            int movementCounter = 0;
            GridSpace currentAlongPath = null;

            // move along the path
            for (int i = 0; i < path.Count; ++i)
            {
                // only move along the path for the amount that the movement range stat allows
                if (movementCounter >= movementRange)
                {
                    break;
                }

                // keep a current grid space updated so we can update the grid properly
                currentAlongPath = path[i];
                // update position for visuals
                transform.position = currentAlongPath.obj.transform.position;

                ++movementCounter;

                yield return new WaitForSeconds(0.1f);
            }

            // if the current space along the path reached the end, attack!
            if (currentAlongPath == path[path.Count - 1])
            {
                Attack();
            }

            // properly reassign grid spaces
            myGridSpace.character = null;
            myGridSpace = currentAlongPath;
            myGridSpace.character = this;
        }

        idle = true;
    }

    private void Attack()
    {
        if (basicAttack != null)
        {
            refCombatGrid.MakeDirty(aggroTarget, basicAttack);

            refCombatGrid.CleanGrid();
        }
    }

    public bool GetIdle()
    {
        return idle;
    }
}

public class AggroData
{
    public Character character;
    public int aggro;

    public AggroData(Character newCharacter, int value)
    {
        character = newCharacter;
        aggro = value;
    }
}
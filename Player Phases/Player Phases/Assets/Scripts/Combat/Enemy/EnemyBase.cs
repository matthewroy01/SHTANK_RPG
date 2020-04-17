using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : Character
{
    private bool idle = true;

    private CombatGrid refCombatGrid;

    public List<AggroData> aggroData = new List<AggroData>();
    private GridSpace aggroTarget;

    private void Start()
    {
        refCombatGrid = FindObjectOfType<CombatGrid>();

        healthCurrent = healthMax;
        movementRangeCurrent = movementRangeDefault;
    }

    private void Update()
    {
        if (idle == true)
        {
            placeholderRenderer.material.color = Color.Lerp(Color.red, Color.black, 0.75f);
        }
        else
        {
            placeholderRenderer.material.color = Color.red;
        }
    }

    public void StartTurn()
    {
        idle = false;

        FindMovementSpaces(refCombatGrid);
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

            if (adjacent.character == null)
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
                int pathCount = refCombatGrid.GetAStar(refCombatGrid, movementSpaces[i], toCheckWith, this, true).Count;
                if (pathCount < lowest)
                {
                    lowest = pathCount;
                    closest = movementSpaces[i];
                }
            }
        }

        return closest;
    }

    private GridSpace ProcessAggro()
    {
        List<AggroData> aggroCandidates = new List<AggroData>();

        // valid aggro candidates are either in range or have built up some kind of aggro
        for (int i = 0; i < aggroData.Count; ++i)
        {
            // remove any player characters that have died
            if (aggroData[i].character == null || aggroData[i].character.GetDead() == true)
            {
                aggroData.RemoveAt(i);
                --i;
                break;
            }

            if (aggroData[i].aggro > 0 || refCombatGrid.GetDistance(myGridSpace, aggroData[i].character.myGridSpace) <= movementRangeCurrent)
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

    private void Attack()
    {
        if (moveset != null)
        {
            // apply source to basic attack for things like aggro and friendly fire
            moveset.ability1.ApplySource(this);

            // make the grid space dirty
            refCombatGrid.MakeDirty(aggroTarget, moveset.ability1);

            // clean the grid to actually perform the attack
            refCombatGrid.CleanGrid();
        }
    }

    public void Selected()
    {
        FindMovementSpaces(refCombatGrid);

        HandleStatuses();
    }

    public bool GetIdle()
    {
        return idle;
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
}
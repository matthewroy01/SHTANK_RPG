using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Character partial class for AI functionality
public partial class Character : MonoBehaviour
{
    public void DoAI()
    {
        HandleStatuses();

        FindMovementSpaces(refCombatGrid);

        // select aggro target
        aggroTarget = ProcessAggro();

        FindObjectOfType<AIProcessor>().ProcessAbility(this, moveset.ability1, movementSpaces, aggroTarget);
    }

    public void AIFinished(AIResult result)
    {
        List<GridSpace> path = new List<GridSpace>(); // the list to store our final movement path

        if (result.foundAggroTarget)
        {
            path = refCombatGrid.GetAStar(refCombatGrid, myGridSpace, result.toMoveTo, this, true);
            StartCoroutine(MoveAlongPath(path, result));
        }
        else
        {
            GridSpace closestValidSpace = null; // a grid space within the list of valid movement spaces that is closest to the aggro target adjacent space

            if (aggroTarget != myGridSpace)
            {
                Debug.Log(gameObject.name + "'s target was " + aggroTarget.character.name + "!");

                closestValidSpace = CheckClosestValidSpace(aggroTarget);

                // find the path to the found valid movement space
                if (closestValidSpace != null)
                {
                    path = refCombatGrid.GetAStar(refCombatGrid, myGridSpace, closestValidSpace, this, true);
                }

                StartCoroutine(MoveAlongPath(path, result));
            }
            else
            {
                idle = true;
            }
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

    private IEnumerator MoveAlongPath(List<GridSpace> path, AIResult result)
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
            }

            // properly reassign grid spaces
            if (currentAlongPath != null)
            {
                myGridSpace.character = null;
                myGridSpace = currentAlongPath;
                myGridSpace.character = this;
            }

            // attack!
            StartCoroutine(Attack(result));
        }

        idle = true;
    }

    private IEnumerator Attack(AIResult result)
    {
        AbilityProcessor tmp = FindObjectOfType<CombatManager>().GetAbilityProcessor();

        if (moveset != null && result.foundAggroTarget == true)
        {
            tmp.ProcessAbility(this, myGridSpace, 1, result.direction, result.flipped, true, false);

            if (tmp.ApplyAbilityCheck())
            {
                yield return new WaitForSecondsRealtime(0.5f);

                tmp.ApplyAbility();
            }
        }

        tmp.UpdateAbilityForecast();
    }

}
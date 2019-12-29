using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBase : Character
{
    private bool idle = true;

    private CombatGrid refCombatGrid;

    public List<AggroData> aggroData = new List<AggroData>();

    private void Start()
    {
        refCombatGrid = FindObjectOfType<CombatGrid>();
    }

    public void StartTurn()
    {
        idle = false;
    }

    public void DoAI()
    {
        GridSpace destination = ProcessAggro();

        List<GridSpace> path = refCombatGrid.GetAStar(refCombatGrid, myGridSpace, destination);
        StartCoroutine(MoveAlongPath(path));
    }

    private GridSpace ProcessAggro()
    {
        List<AggroData> highestAggro = new List<AggroData>();
        highestAggro.Add(aggroData[0]);

        // find the aggro data with the highest aggro value
        for(int i = 0; i < aggroData.Count; ++i)
        {
            // if a new highest aggro is found, clear the list and add the new character
            if (aggroData[i].aggro > highestAggro[0].aggro)
            {
                highestAggro.Clear();
                highestAggro.Add(aggroData[i]);
            }
            // if the aggros are equal, add it to the list as well
            else if (aggroData[i].aggro == highestAggro[0].aggro)
            {
                highestAggro.Add(aggroData[i]);
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
        for (int i = 0; i < path.Count; ++i)
        {
            transform.position = path[i].obj.transform.position;

            yield return new WaitForSeconds(0.1f);
        }

        idle = true;
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
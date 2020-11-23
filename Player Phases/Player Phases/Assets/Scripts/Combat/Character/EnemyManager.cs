using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : CharacterManager
{
    [Header("List of characters in combat after spawning")]
    public List<Character> enemies = new List<Character>();

    private List<string> names = new List<string>();

    private void Start()
    {
        refPhaseManager = FindObjectOfType<PhaseManager>();
        refCombatGrid = FindObjectOfType<CombatGrid>();

        refPlayerManager = FindObjectOfType<PlayerManager>();
        refAllyManager = FindObjectOfType<AllyManager>();
    }

    public void MyUpdate()
    {
        if (enemies.Count > 0)
        {
            // remove dead players
            for (int i = 0; i < enemies.Count; ++i)
            {
                if (enemies[i].GetDead() == true)
                {
                    StartCoroutine(enemies[i].Death());

                    enemies.RemoveAt(i);
                }
            }
        }
        else
        {
            refPlayerManager.EndPhase();
        }
    }

    public void SpawnEnemies(EnemyGroup group)
    {
        AddRandomNames();

        int targetX = (int)refCombatGrid.gridWidth - 1, targetY = (int)refCombatGrid.gridHeight - 1;

        for (int i = 0; i < group.characterDefinitions.Count; ++i)
        {
            Character tmp;

            // spawn enemies and add them to the list
            tmp = Instantiate(characterPrefab, transform).GetComponent<Character>();
            enemies.Add(tmp);

            // get a valid spawning space for this character
            GridSpace spawnSpace = GetSpawnSpace(targetX, targetY, group.characterDefinitions[i].terrainTypes, Character_Affiliation.enemy);

            // set enemy position and grid space
            tmp.transform.position = spawnSpace.obj.transform.position;
            spawnSpace.character = tmp;
            tmp.myGridSpace = spawnSpace;

            AssignCharacterValues(tmp.gameObject, group.characterDefinitions[i], Character_Affiliation.enemy);
            AssignPassive(tmp, group.characterDefinitions[i]);

            // if the enemy name is blank, set it to something random
            if (tmp.name == "")
            {
                int rand = Random.Range(0, names.Count);
                tmp.name = names[rand];
                names.RemoveAt(rand);
            }

            // initialize aggro data for each enemy
            for (int j = 0; j < refPlayerManager.players.Count; ++j)
            {
                tmp.aggroData.Add(new AggroData(refPlayerManager.players[j], 1));
            }
        }
    }

    public void EnemyActions()
    {
        for (int i = 0; i < enemies.Count; ++i)
        {
            enemies[i].StartTurn(refCombatGrid);
        }

        // enable enemy actions
        StartCoroutine(EnemyActionsCoroutine());
    }

    private IEnumerator EnemyActionsCoroutine()
    {
        // loop through all enemies and have them do their actions
        for (int i = 0; i < enemies.Count; ++i)
        {
            enemies[i].DoAI();

            while (!enemies[i].GetIdle())
            {
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForSeconds(0.5f);
        }

        // this current phase is done, let the Phase Manager know it's time to move to the next phase
        StartCoroutine(refPhaseManager.NextPhase());
    }

    public void AlertAllEnemies(Character target, Character source)
    {
        for (int i = 0; i < enemies.Count; ++i)
        {
            // look for the target of the aggro
            for (int j = 0; j < enemies[i].aggroData.Count; ++j)
            {
                // once we find the target, set it to 1 if it was 0
                if (enemies[i].aggroData[j].character == target && enemies[i].aggroData[j].aggro == 0)
                {
                    enemies[i].aggroData[j].aggro = 1;
                }
            }
        }
    }

    public void DispelAggroFromTarget(Character target, int amount)
    {
        for (int i = 0; i < enemies.Count; ++i)
        {
            enemies[i].DispelAggroFromTarget(target, amount);
        }
    }

    public void DestroyCharacters()
    {
        for (int i = 0; i < enemies.Count; ++i)
        {
            Destroy(enemies[i].gameObject);
        }

        enemies.Clear();
    }

    public void AddToAggroTargets(Character character)
    {
        for (int i = 0; i < enemies.Count; ++i)
        {
            enemies[i].aggroData.Add(new AggroData(character, 1));
        }
    }

    public void RemoveFromAggroTargets(Character character)
    {
        for (int i = 0; i < enemies.Count; ++i)
        {
            for (int j = 0; j < enemies[i].aggroData.Count; ++j)
            {
                if (enemies[i].aggroData[j].character == character)
                {
                    enemies[i].aggroData.RemoveAt(j);
                    --j;
                }
            }
        }
    }

    private void AddRandomNames()
    {
        string[] tmp = {
            "S-Fear",
            "P-Trol",
            "Some Robot",
            "M4L-Function",
            "Barely a Threat",
            "Crazy Eyes",
            "A-Nihilation",
            "D-Struction",
            "Peter Plum",
            "Gimbal Lock",
            "Nicholas Picholas",
            "Scarred Jellybean",
            "The Food Machine",
            "No Clue's Grandma",
            "A Unity Primitive",
            "John Quoiby",
            "Michael Aberdeen"
        };
        names = new List<string>(tmp);
    }

    public override void AddCharacter(CharacterDefinition def, GridSpace gridSpace)
    {

    }
}

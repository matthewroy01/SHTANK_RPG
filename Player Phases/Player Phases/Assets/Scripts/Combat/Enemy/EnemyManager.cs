using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private PhaseManager refPhaseManager;
    private PlayerManager refPlayerManager;
    private CombatManager refCombatInitiator;

    [Header("Base Enemy Prefab")]
    public GameObject enemyPrefab;

    [Header("List of characters in combat after spawning")]
    public List<EnemyBase> enemies = new List<EnemyBase>();

    private CombatGrid refCombatGrid;

    private List<string> names = new List<string>();

    private void Start()
    {
        refPhaseManager = FindObjectOfType<PhaseManager>();
        refPlayerManager = FindObjectOfType<PlayerManager>();
        refCombatInitiator = FindObjectOfType<CombatManager>();

        refCombatGrid = FindObjectOfType<CombatGrid>();
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
            EnemyBase tmp;

            // spawn enemies and add them to the list
            tmp = Instantiate(enemyPrefab, transform).GetComponent<EnemyBase>();
            enemies.Add(tmp);

            // get a valid spawning space for this character
            GridSpace spawnSpace = GetSpawnSpace(targetX, targetY, group.characterDefinitions[i].terrainTypes);

            // set enemy position and grid space
            tmp.transform.position = spawnSpace.obj.transform.position;
            spawnSpace.character = tmp;
            tmp.myGridSpace = spawnSpace;

            AssignEnemyValues(tmp.gameObject, group.characterDefinitions[i]);
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

    private GridSpace GetSpawnSpace(int targetX, int targetY, List<GridSpace_TerrainType> validTerrainTypes)
    {
        List<GridSpace> search = refCombatGrid.GetBreadthFirst(refCombatGrid.grid[targetX, targetY], refCombatGrid.gridWidth * refCombatGrid.gridHeight, TerrainTypePresets.all, Character_Affiliation.enemy);

        for (int i = 0; i < search.Count; ++i)
        {
            if (validTerrainTypes.Contains(search[i].GetTerrainType()) && search[i].character == null && IsOnCheckerboard(search[i].coordinate))
            {
                return search[i];
            }
        }

        return refCombatGrid.grid[targetX, targetY];
    }

    private bool IsOnCheckerboard(Vector2Int coordinate)
    {
        if (coordinate.x % 2 == 0)
        {
            if (coordinate.y % 2 == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else if (coordinate.x % 2 == 1)
        {
            if (coordinate.y % 2 == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        return false;
    }

    private void AssignEnemyValues(GameObject obj, CharacterDefinition def)
    {
        EnemyBase refEnemyBase = obj.GetComponent<EnemyBase>();
        MovementDialogueProcessor refMovementDialogueProcessor = obj.GetComponent<MovementDialogueProcessor>();
        SpriteRenderer refSpriteRenderer = obj.GetComponentInChildren<SpriteRenderer>();

        obj.name = def.characterName;

        if (refEnemyBase != null)
        {
            // max health and current health
            refEnemyBase.healthMax = def.healthMax;
            refEnemyBase.healthCurrent = refEnemyBase.healthMax;

            // attack and defense modifiers
            refEnemyBase.attackMod = def.attackMod;
            refEnemyBase.defenseMod = def.defenseMod;

            // nashbalm
            refEnemyBase.nashbalm = def.nashbalm;

            // movement range
            refEnemyBase.movementRangeDefault = def.movementRange;

            // moveset
            refEnemyBase.moveset = def.moveset;

            // affiliation (always enemy, since this is the Enemy Manager)
            refEnemyBase.affiliation = Character_Affiliation.enemy;

            // navigable terrain types
            refEnemyBase.terrainTypes = def.terrainTypes;

            // character portrait for UI
            refEnemyBase.portrait = def.portrait;

            // ability definitions for UI
            refEnemyBase.abilityUIDefinition = FindObjectOfType<CharacterUI>().InitializeAbilityUI(refEnemyBase);
        }

        if (refMovementDialogueProcessor != null)
        {
            // movement dialogue text
            refMovementDialogueProcessor.dialogue = def.movementDialogue;

            // movement dialogue sound
            refMovementDialogueProcessor.speechBlip = def.movementDialogueSound;
        }

        if (refSpriteRenderer != null)
        {
            // temporary sprite
            refSpriteRenderer.sprite = def.sprite;
        }
    }

    private void AssignPassive(Character parent, CharacterDefinition def)
    {
        if (def.passiveFunctionality != null)
        {
            parent.passive = Instantiate(def.passiveFunctionality, parent.transform);
            parent.passive.myCharacter = parent;
        }
    }

    public void EnemyActions()
    {
        for (int i = 0; i < enemies.Count; ++i)
        {
            enemies[i].StartTurn();
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
        refPhaseManager.NextPhase();
    }

    public void AlertAllEnemies(Character target, EnemyBase source)
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
            "Reluctant Game Tester",
            "John Quoiby",
            "Michael Aberdeen"
        };
        names = new List<string>(tmp);
    }
}

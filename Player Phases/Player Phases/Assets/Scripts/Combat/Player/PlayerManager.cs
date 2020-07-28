using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private PhaseManager refPhaseManager;
    private CombatManager refCombatInitiator;

    [Header("Base Player Prefab")]
    public GameObject playerPrefab;

    [Header("List of specific players to spawn")]
    public List<CharacterDefinition> characterDefinitions;

    [Header("List of players in combat after spawning")]
    public List<PlayerBase> players = new List<PlayerBase>();

    private CombatGrid refCombatGrid;

    private List<string> names = new List<string>();

    private void Start()
    {
        refPhaseManager = FindObjectOfType<PhaseManager>();

        refCombatGrid = FindObjectOfType<CombatGrid>();

        /*names.Add("Karate Person");
        names.Add("Melon Man");
        names.Add("All Real Numbers");
        names.Add("Cheddar Jack");
        names.Add("Tuna Boy");
        names.Add("Tuna Boy's Sister");
        names.Add("Fire and Ice Ball Man");
        names.Add("No Clue");
        names.Add("Danny Doppelganger");
        names.Add("Sparrow");
        names.Add("The Entomologist");
        names.Add("Magnet Hands");
        names.Add("Knight and Day");
        names.Add("Ava");
        names.Add("unnamed sixteenth character");*/
    }

    public void MyUpdate()
    {
        // remove dead players
        for (int i = 0; i < players.Count; ++i)
        {
            if (players[i].GetDead() == true)
            {
                StartCoroutine(players[i].Death());

                players.RemoveAt(i);
            }
        }
    }

    public void SpawnPlayers()
    {
        int targetX = 0, targetY = 0;

        for (int i = 0; i < characterDefinitions.Count; ++i)
        {
            // spawn players and add them to the list
            PlayerBase tmp = Instantiate(playerPrefab, transform).GetComponent<PlayerBase>();
            players.Add(tmp);

            // get a valid spawning space for this character
            GridSpace spawnSpace = GetSpawnSpace(targetX, targetY, characterDefinitions[i].terrainTypes);

            tmp.transform.position = spawnSpace.obj.transform.position;
            spawnSpace.character = tmp;
            tmp.myGridSpace = spawnSpace;

            AssignPlayerValues(tmp.gameObject, characterDefinitions[i]);
            AssignPassive(tmp, characterDefinitions[i]);
        }
    }

    private GridSpace GetSpawnSpace(int targetX, int targetY, List<GridSpace_TerrainType> validTerrainTypes)
    {
        List<GridSpace> search = refCombatGrid.GetBreadthFirst(refCombatGrid.grid[targetX, targetY], refCombatGrid.gridWidth * refCombatGrid.gridHeight, TerrainTypePresets.all, Character_Affiliation.player);

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

    private void AssignPlayerValues(GameObject obj, CharacterDefinition def)
    {
        PlayerBase refPlayerBase = obj.GetComponent<PlayerBase>();
        MovementDialogueProcessor refMovementDialogueProcessor = obj.GetComponent<MovementDialogueProcessor>();
        SpriteRenderer refSpriteRenderer = obj.GetComponentInChildren<SpriteRenderer>();

        obj.name = def.characterName;

        if (refPlayerBase != null)
        {
            // max health and current health
            refPlayerBase.healthMax = def.healthMax;
            refPlayerBase.healthCurrent = refPlayerBase.healthMax;

            // attack and defense modifiers
            refPlayerBase.attackMod = def.attackMod;
            refPlayerBase.defenseMod = def.defenseMod;

            // nashbalm
            refPlayerBase.nashbalm = def.nashbalm;

            // movement range
            refPlayerBase.movementRangeDefault = def.movementRange;

            // moveset
            refPlayerBase.moveset = def.moveset;

            // affiliation (always player, since this is the Player Manager)
            refPlayerBase.affiliation = Character_Affiliation.player;

            // navigable terrain types
            refPlayerBase.terrainTypes = def.terrainTypes;

            // character portrait for UI
            refPlayerBase.portrait = def.portrait;

            // ability definitions for UI
            refPlayerBase.abilityUIDefinition = FindObjectOfType<CharacterUI>().InitializeAbilityUI(refPlayerBase);

            // character description for tooltips
            refPlayerBase.characterDescription = def.characterDescription;
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

    public void PlayerActions()
    {
        // enable player actions
        StartCoroutine(PlayerActionsCoroutine());
    }

    private IEnumerator PlayerActionsCoroutine()
    {
        // loop through all players and enable their actions
        for (int i = 0; i < players.Count; ++i)
        {
            players[i].StartTurn(refCombatGrid);

            yield return new WaitForEndOfFrame();
        }

        // check if all players are idle again
        int counter = 0;
        while (counter < players.Count)
        {
            counter = 0;

            for (int i = 0; i < players.Count; ++i)
            {
                if (players[i].GetIdle() == true)
                {
                    counter++;
                }
            }

            yield return new WaitForEndOfFrame();
        }

        // this current phase is done, let the Phase Manager know it's time to move to the next phase
        refPhaseManager.NextPhase();
    }

    public void EndPhase()
    {
        for (int i = 0; i < players.Count; ++i)
        {
            players[i].EndTurn();
        }
    }

    public void DestroyCharacters()
    {
        for (int i = 0; i < players.Count; ++i)
        {
            Destroy(players[i].gameObject);
        }

        players.Clear();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : CharacterManager
{
    private Party refParty;

    [Header("List of specific players to spawn")]
    public List<CharacterDefinition> characterDefinitions;

    [Header("List of players in combat after spawning")]
    public List<Character> players = new List<Character>();

    private List<string> names = new List<string>();

    private void Start()
    {
        refPhaseManager = FindObjectOfType<PhaseManager>();
        refCombatGrid = FindObjectOfType<CombatGrid>();

        refAllyManager = FindObjectOfType<AllyManager>();
        refEnemyManager = FindObjectOfType<EnemyManager>();
        refCombatGrid = FindObjectOfType<CombatGrid>();

        refParty = FindObjectOfType<Party>();

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
                refEnemyManager.RemoveFromAggroTargets(players[i]);

                players.RemoveAt(i);
            }
        }
    }

    public void SpawnPlayers()
    {
        int targetX = 0, targetY = 0;

        for (int i = 0; i < refParty.partyActive.Count; ++i)
        {
            // spawn players and add them to the list
            Character tmp = Instantiate(characterPrefab, transform).GetComponent<Character>();
            players.Add(tmp);

            // get a valid spawning space for this character
            GridSpace spawnSpace = GetSpawnSpace(targetX, targetY, refParty.partyActive[i].loader.terrainTypes, Character_Affiliation.player);

            tmp.transform.position = spawnSpace.obj.transform.position;
            spawnSpace.character = tmp;
            tmp.myGridSpace = spawnSpace;

            AssignCharacterValues(tmp.gameObject, refParty.partyActive[i].loader, Character_Affiliation.player, refParty.partyActive[i]);
            AssignPassive(tmp, refParty.partyActive[i].loader);
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
        StartCoroutine(refPhaseManager.NextPhase());
    }

    public void EndPhase()
    {
        for (int i = 0; i < players.Count; ++i)
        {
            players[i].EndTurn();
        }
    }

    public override void AddCharacter(CharacterDefinition def, GridSpace gridSpace)
    {
        
    }
}

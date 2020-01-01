﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    private PhaseManager refPhaseManager;

    public GameObject playerPrefab;
    public List<PlayerBase> players = new List<PlayerBase>();

    private CombatGrid refCombatGrid;

    private List<string> names = new List<string>();

    private void Start()
    {
        refPhaseManager = FindObjectOfType<PhaseManager>();

        refCombatGrid = FindObjectOfType<CombatGrid>();

        names.Add("Karate Person");
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
        names.Add("unnamed sixteenth character");
    }

    private void Update()
    {
        for (int i = 0; i < players.Count; ++i)
        {
            // remove dead players
            if (players[i].GetDead() == true)
            {
                StartCoroutine(players[i].Death());

                players.RemoveAt(i);
            }
        }
    }

    public void SpawnPlayers()
    {
        for (int i = 0; i < 2; ++i)
        {
            // spawn players and add them to the list
            PlayerBase tmp = Instantiate(playerPrefab, transform).GetComponent<PlayerBase>();
            players.Add(tmp);
            tmp.transform.position = refCombatGrid.grid[i, 0].obj.transform.position;
            refCombatGrid.grid[i, 0].character = tmp;
            tmp.myGridSpace = refCombatGrid.grid[i, 0];

            int rand = Random.Range(0, names.Count);
            tmp.name = names[rand];
            names.RemoveAt(rand);
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
}

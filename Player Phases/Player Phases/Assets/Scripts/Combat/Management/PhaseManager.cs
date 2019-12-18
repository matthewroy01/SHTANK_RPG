using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PhaseManager : MonoBehaviour
{
    [Header("Combat phase info")]
    public CombatPhase currentPhase; // the current combat phase, to keep track of which phase is active
    private int totalPhases = 2; // the total number of phases (not including the Null phase) in the CombatPhase enum

    private PlayerManager refPlayerManager;
    private EnemyManager refEnemyManager;

    void Start()
    {
        refPlayerManager = FindObjectOfType<PlayerManager>();
        refEnemyManager = FindObjectOfType<EnemyManager>();

        // set current phase to default while outside of combat
        currentPhase = CombatPhase.Null;
    }

    // start combat by changing the current state to something other than "Null"
    public void InitiateCombat(CombatPhase startingPhase)
    {
        if (currentPhase == CombatPhase.Null)
        {
            // change the current phase
            // by allowing any starting phase, we can create scenarios where the player or the enemy can initate combat
            currentPhase = startingPhase;
        }

        refPlayerManager.SpawnPlayers();
        refEnemyManager.SpawnEnemies();

        FireEvents();
    }

    // change the current phase to the next phase, in order that they appear in the enum
    public void NextPhase()
    {
        // increment the current phase
        if ((int)currentPhase + 1 < totalPhases)
        {
            currentPhase++;
        }
        // if we're at the end of the phase list, return to the start
        else
        {
            currentPhase = 0;
        }

        FireEvents();
    }

    private void FireEvents()
    {
        Debug.Log("It is now " + currentPhase + " phase!");

        // based on the current phase, fire events
        switch (currentPhase)
        {
            case CombatPhase.Player:
            {
                refPlayerManager.PlayerActions();
                break;
            }
            case CombatPhase.Enemy:
            {
                refEnemyManager.EnemyActions();
                break;
            }
            default:
            {
                break;
            }
        }
    }
}

/*
 * Combat Phases include:
 * - a "null" phase for when there is no combat
 * - a Player phase for the player to act
 * - an enemy phase for AI to act
 * - any more that we may want to add if there is need for an additional AI controlled party on the field
 */
public enum CombatPhase { Null = -1, Player, Enemy };
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.SceneManagement;

public class PhaseManager : MonoBehaviour
{
    [Header("Combat phase info")]
    public CombatPhase currentPhase; // the current combat phase, to keep track of which phase is active
    private int totalPhases = 2; // the total number of phases (not including the Null phase) in the CombatPhase enum

    [Header("Phase UI")]
    public EffectUIParameters uiEffectPhasePlayer;
    public EffectUIParameters uiEffectPhaseEnemy;
    public EffectUIParameters uiEffectVictoryAndDefeat;
    public float timeBetweenPhases;

    private PlayerManager refPlayerManager;
    private EnemyManager refEnemyManager;
    private CombatGrid refCombatGrid;

    private AudioSource refAudioSource;

    void Start()
    {
        refPlayerManager = FindObjectOfType<PlayerManager>();
        refEnemyManager = FindObjectOfType<EnemyManager>();
        refCombatGrid = FindObjectOfType<CombatGrid>();

        refAudioSource = GetComponent<AudioSource>();

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

        StartCoroutine(FireEvents());
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

        StartCoroutine(FireEvents());
    }

    public void Victory()
    {
        uiEffectVictoryAndDefeat.Apply("Victory!", refAudioSource);

        Invoke("RestartScene", 3.0f);
    }

    public void Defeat()
    {
        uiEffectVictoryAndDefeat.Apply("Defeat...", refAudioSource);

        Invoke("RestartScene", 3.0f);
    }

    private IEnumerator FireEvents()
    {
        Debug.Log("It is now " + currentPhase + " phase!");

        // if either the player manager or enemy manager have no characters remaining, end combat
        if (refPlayerManager.players.Count == 0 && refEnemyManager.enemies.Count == 0)
        {
            Debug.LogError("PhaseManager, FireEvents, refPlayerManager and refEnemyManager have no characters stored. Is this a draw?");
            yield break;
        }
        else if (refPlayerManager.players.Count == 0)
        {
            Defeat();
            yield break;
        }
        else if (refEnemyManager.enemies.Count == 0)
        {
            Victory();
            yield break;
        }

        // based on the current phase, fire events
        switch (currentPhase)
        {
            case CombatPhase.Player:
            {
                // alert the combat grid that the next turn has begun
                refCombatGrid.NextTurn();

                uiEffectPhasePlayer.Apply("Player Phase", refAudioSource);

                yield return new WaitForSeconds(timeBetweenPhases);

                refPlayerManager.PlayerActions();
                break;
            }
            case CombatPhase.Enemy:
            {
                uiEffectPhaseEnemy.Apply("Enemy Phase", refAudioSource);

                yield return new WaitForSeconds(timeBetweenPhases);

                refEnemyManager.EnemyActions();
                break;
            }
            default:
            {
                break;
            }
        }
    }

    private void DisplayText(string text, EffectUIParameters effect)
    {
        ClearText();

        if (effect.ui != null)
        {
            effect.ui.color = effect.color;
            effect.ui.text = text;
        }

        if (effect.sound != null)
        {
            refAudioSource.PlayOneShot(effect.sound);
        }

        if (effect.effect != null)
        {
            effect.effect.DoEffect();
        }

        CancelInvoke("ClearText");
        Invoke("ClearText", timeBetweenPhases);
    }

    private void ClearText()
    {
        uiEffectPhasePlayer.Clear();
        uiEffectPhaseEnemy.Clear();
        uiEffectVictoryAndDefeat.Clear();
    }

    private void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
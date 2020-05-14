using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.SceneManagement;

public class PhaseManager : MonoBehaviour
{
    public bool inCombat;

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
    private GridColorProcessor refGridColorProcessor;
    private CombatGrid refCombatGrid;

    private UtilityAudioManager refAudioManager;

    void Start()
    {
        refPlayerManager = FindObjectOfType<PlayerManager>();
        refEnemyManager = FindObjectOfType<EnemyManager>();
        refGridColorProcessor = FindObjectOfType<GridColorProcessor>();
        refCombatGrid = FindObjectOfType<CombatGrid>();
        refAudioManager = FindObjectOfType<UtilityAudioManager>();

        // set current phase to default while outside of combat
        currentPhase = CombatPhase.Null;
    }

    public void MyUpdate()
    {
        refPlayerManager.MyUpdate();
        refEnemyManager.MyUpdate();
        refGridColorProcessor.MyUpdate();
    }

    // start combat by changing the current state to something other than "Null"
    public void InitiateCombat(CombatPhase startingPhase, Vector3 collisionPoint)
    {
        if (currentPhase == CombatPhase.Null)
        {
            // change the current phase
            // by allowing any starting phase, we can create scenarios where the player or the enemy can initate combat
            currentPhase = startingPhase;
        }

        refPlayerManager.SpawnPlayers();
        refEnemyManager.SpawnEnemies();

        StartCoroutine(PreCombatAnimation(collisionPoint));
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
        uiEffectVictoryAndDefeat.Apply("Victory!", refAudioManager);

        Invoke("RestartScene", 3.0f);
    }

    public void Defeat()
    {
        uiEffectVictoryAndDefeat.Apply("Defeat...", refAudioManager);

        Invoke("RestartScene", 3.0f);
    }

    public EnemyManager GetEnemyManager()
    {
        return refEnemyManager;
    }

    private IEnumerator PreCombatAnimation(Vector3 collisionPoint)
    {
        // snap players into place to start
        for (int i = 0; i < refPlayerManager.players.Count; ++i)
        {
            refPlayerManager.players[i].transform.position = new Vector3(collisionPoint.x, refPlayerManager.players[i].transform.position.y, collisionPoint.z);
        }

        // snap enemies into place to start
        for (int i = 0; i < refEnemyManager.enemies.Count; ++i)
        {
            refEnemyManager.enemies[i].transform.position = new Vector3(collisionPoint.x, refEnemyManager.enemies[i].transform.position.y, collisionPoint.z);
        }

        bool playersComplete, enemiesComplete;

        do
        {
            // keep moving players until they are all in the correct positions
            playersComplete = true;

            for (int i = 0; i < refPlayerManager.players.Count; ++i)
            {
                if (!PreCombatAnimationLerp(refPlayerManager.players[i], 0.1f))
                {
                    playersComplete = false;
                }
            }

            // keep moving enemies until they are all in the correct positions
            enemiesComplete = true;

            for (int i = 0; i < refEnemyManager.enemies.Count; ++i)
            {
                if (!PreCombatAnimationLerp(refEnemyManager.enemies[i], 0.1f))
                {
                    enemiesComplete = false;
                }
            }

            yield return new WaitForFixedUpdate();
        }
        while (!playersComplete || !enemiesComplete);

        StartCoroutine(FireEvents());
    }

    private bool PreCombatAnimationLerp(Character character, float lerp)
    {
        if (Vector3.Distance(character.transform.position, character.myGridSpace.obj.transform.position) > 0.1f)
        {
            // move a character from combat's starting position to its proper position
            Vector3 gridSpacePos = character.myGridSpace.obj.transform.position;
            character.transform.position = Vector3.Lerp(character.transform.position, new Vector3(gridSpacePos.x, character.transform.position.y, gridSpacePos.z), lerp);

            return false;
        }

        PreCombatAnimationSnap(character);

        return true;
    }

    private void PreCombatAnimationSnap(Character character)
    {
        // if a character is done moving, snap it into position just in case
        Vector3 gridSpacePos = character.myGridSpace.obj.transform.position;
        character.transform.position = new Vector3(gridSpacePos.x, character.transform.position.y, gridSpacePos.z);
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

                uiEffectPhasePlayer.Apply("Player Phase", refAudioManager);

                yield return new WaitForSeconds(timeBetweenPhases);

                refPlayerManager.PlayerActions();
                break;
            }
            case CombatPhase.Enemy:
            {
                uiEffectPhaseEnemy.Apply("Enemy Phase", refAudioManager);

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
            refAudioManager.QueueSound(effect.sound);
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
        //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
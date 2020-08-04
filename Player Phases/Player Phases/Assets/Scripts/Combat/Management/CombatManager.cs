using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    private CombatGrid refCombatGrid;
    private SHTANKCamera refCombatCamera;
    private PhaseManager refPhaseManager;

    public OverworldEnemyController currentEnemy;

    void Start()
    {
        refCombatGrid = GetComponent<CombatGrid>();
        refCombatCamera = FindObjectOfType<SHTANKCamera>();
        refPhaseManager = FindObjectOfType<PhaseManager>();
    }

    public void MyUpdate()
    {
        refPhaseManager.MyUpdate();
    }

    public void InitiatePhase(CombatPhase initialPhase, Vector3 collisionPosition, OverworldEnemyController targetEnemy)
    {
        if (initialPhase == CombatPhase.Null)
        {
            Debug.LogWarning("Combat Initiator: Initial Phase probably shouldn't be Null.");
        }

        refCombatGrid.SpawnGrid(collisionPosition);
        refCombatCamera.InitiateCombatCamera();
        refPhaseManager.InitiateCombat(initialPhase, collisionPosition);

        currentEnemy = targetEnemy;
    }

    public void DestroyCombatObjects(bool won)
    {
        refCombatGrid.DestroyGrid();
        refPhaseManager.DestroyCharacters();

        if (won)
        {
            Destroy(currentEnemy.gameObject);
        }
    }
}

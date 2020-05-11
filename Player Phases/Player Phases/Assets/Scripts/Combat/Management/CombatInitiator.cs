using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatInitiator : MonoBehaviour
{
    private CombatGrid refCombatGrid;
    private CombatCamera refCombatCamera;
    private PhaseManager refPhaseManager;

    void Start()
    {
        refCombatGrid = GetComponent<CombatGrid>();
        refCombatCamera = FindObjectOfType<CombatCamera>();
        refPhaseManager = FindObjectOfType<PhaseManager>();

        //InitiatePhase(CombatPhase.Player);
    }

    public void InitiatePhase(CombatPhase initialPhase, Vector3 collisionPosition)
    {
        if (initialPhase == CombatPhase.Null)
        {
            Debug.LogWarning("Combat Initiator: Initial Phase probably shouldn't be Null.");
        }

        refCombatGrid.SpawnGrid(collisionPosition);
        refCombatCamera.InitiateCombatCamera();
        refPhaseManager.InitiateCombat(initialPhase);
    }
}

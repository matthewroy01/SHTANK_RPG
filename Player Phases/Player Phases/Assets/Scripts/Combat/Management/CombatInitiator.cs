using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatInitiator : MonoBehaviour
{
    private CombatGrid refCombatGrid;
    private PhaseManager refPhaseManager;

    void Start()
    {
        refCombatGrid = GetComponent<CombatGrid>();
        refPhaseManager = FindObjectOfType<PhaseManager>();

        //InitiatePhase(CombatPhase.Player);
    }

    public void InitiatePhase(CombatPhase initialPhase)
    {
        if (initialPhase == CombatPhase.Null)
        {
            Debug.LogWarning("Combat Initiator: Initial Phase probably shouldn't be Null.");
        }

        refCombatGrid.SpawnGrid();
        refPhaseManager.InitiateCombat(initialPhase);
    }
}

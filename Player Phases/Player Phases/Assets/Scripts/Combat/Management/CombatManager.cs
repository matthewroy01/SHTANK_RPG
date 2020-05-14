using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    private CombatGrid refCombatGrid;
    private SHTANKCamera refCombatCamera;
    private PhaseManager refPhaseManager;

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

    public void InitiatePhase(CombatPhase initialPhase, Vector3 collisionPosition)
    {
        if (initialPhase == CombatPhase.Null)
        {
            Debug.LogWarning("Combat Initiator: Initial Phase probably shouldn't be Null.");
        }

        refCombatGrid.SpawnGrid(collisionPosition);
        refCombatCamera.InitiateCombatCamera();
        refPhaseManager.InitiateCombat(initialPhase, collisionPosition);
    }

    public void DestroyCombatObjects()
    {
        refCombatGrid.DestroyGrid();
        refPhaseManager.DestroyCharacters();
    }
}

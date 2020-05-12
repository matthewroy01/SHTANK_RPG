using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHTANKManager : MonoBehaviour
{
    private StateMachine stateMachine;

    private OverworldManager refOverworldManager;
    private CombatManager refCombatManager;

    private void Awake()
    {
        InitializeStateMachine();
    }

    private void InitializeStateMachine()
    {
        // set up state machine connections here
        stateMachine = new StateMachine((int)GameState.overworld,

            // overworld and combat
            new StateMachineConnection((int)GameState.overworld, (int)GameState.combat),
            new StateMachineConnection((int)GameState.combat, (int)GameState.overworld)
        );
    }

    void Start()
    {
        refOverworldManager = FindObjectOfType<OverworldManager>();
        refCombatManager = FindObjectOfType<CombatManager>();
    }

    void Update()
    {
        ProcessState();
    }

    private void ProcessState()
    {
        switch (stateMachine.currentState)
        {
            case (int)GameState.overworld:
            {
                // run overworld behaviour
                refOverworldManager.MyUpdate();

                break;
            }
            case (int)GameState.combat:
            {
                // run combat behaviour
                refCombatManager.MyUpdate();

                break;
            }
        }
    }

    public void TryBeginCombat(Vector3 position)
    {
        if (stateMachine.TryUpdateConnection((int)GameState.combat))
        {
            refCombatManager.InitiatePhase(CombatPhase.Player, position);
        }
    }

    public void TryEndCombat()
    {
        if (stateMachine.TryUpdateConnection((int)GameState.overworld))
        {
            
        }
    }

    public enum GameState
    {
        overworld = 0,
        combat = 1
    }
}

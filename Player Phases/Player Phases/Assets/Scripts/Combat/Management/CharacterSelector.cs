using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelector : MonoBehaviour
{
    [Header("Selected Characters (for display only)")]
    public PlayerBase currentPlayer;
    private GridSpace defaultGridSpace;
    public Character overlayCharacter;

    [Header("Layer Masks for raycast collision from mouse")]
    public LayerMask layerMaskGrid;

    private StateMachine stateMachine;

    private CombatGrid refCombatGrid;
    private AbilityProcessor refAbilityProcessor;
    private CharacterUI refCharacterUI;

    System.Tuple<KeyCode, CombatDirection>[] keysAndDirections = {
            new System.Tuple<KeyCode, CombatDirection>(KeyCode.W, CombatDirection.up),
            new System.Tuple<KeyCode, CombatDirection>(KeyCode.S, CombatDirection.down),
            new System.Tuple<KeyCode, CombatDirection>(KeyCode.D, CombatDirection.left),
            new System.Tuple<KeyCode, CombatDirection>(KeyCode.A, CombatDirection.right),
        };

    private void Awake()
    {
        InitializeStateMachine();
    }

    private void InitializeStateMachine()
    {
        // set up state machine connections here
        stateMachine = new StateMachine((int)SelectorState.doingNothing,

            // from doing nothing
            new StateMachineConnection((int)SelectorState.doingNothing, (int)SelectorState.playerSelected),
            new StateMachineConnection((int)SelectorState.doingNothing, (int)SelectorState.enemySelected),

            // base level selecting a player or enemy
            new StateMachineConnection((int)SelectorState.playerSelected, (int)SelectorState.playerSelected),
            new StateMachineConnection((int)SelectorState.enemySelected, (int)SelectorState.enemySelected),
            new StateMachineConnection((int)SelectorState.playerSelected, (int)SelectorState.enemySelected),
            new StateMachineConnection((int)SelectorState.enemySelected, (int)SelectorState.playerSelected),
            new StateMachineConnection((int)SelectorState.enemySelected, (int)SelectorState.doingNothing),
            new StateMachineConnection((int)SelectorState.playerSelected, (int)SelectorState.doingNothing),

            // higher levels of selecting players
            new StateMachineConnection((int)SelectorState.playerSelected, (int)SelectorState.playerSelectedWithMovement),
            new StateMachineConnection((int)SelectorState.playerSelected, (int)SelectorState.playerSelectedWithAbility),
            new StateMachineConnection((int)SelectorState.playerSelectedWithMovement, (int)SelectorState.playerSelected),
            new StateMachineConnection((int)SelectorState.playerSelectedWithAbility, (int)SelectorState.playerSelectedWithMovement),
            new StateMachineConnection((int)SelectorState.playerSelectedWithAbility, (int)SelectorState.playerSelected),

            // ending a player's turn
            new StateMachineConnection((int)SelectorState.playerSelected, (int)SelectorState.doingNothing),
            new StateMachineConnection((int)SelectorState.playerSelectedWithAbility, (int)SelectorState.doingNothing),
            new StateMachineConnection((int)SelectorState.playerSelectedWithMovement, (int)SelectorState.doingNothing)
        );
    }

    private void Start()
    {
        refCombatGrid = FindObjectOfType<CombatGrid>();
        refAbilityProcessor = FindObjectOfType<AbilityProcessor>();
        refCharacterUI = FindObjectOfType<CharacterUI>();
    }

    private void Update()
    {
        ProcessState();
    }

    private void ProcessState()
    {
        switch(stateMachine.currentState)
        {
            case (int)SelectorState.doingNothing:
            {
                // select player characters
                Select();

                break;
            }
            case (int)SelectorState.playerSelected:
            {
                // input abilities

                // input movement
                MovementMouse();

                // select a different character
                Select();

                // confirm/end movement
                EndTurn();

                // deselect the current character
                Deselect();

                break;
            }
            case (int)SelectorState.enemySelected:
            {
                break;
            }
            case (int)SelectorState.playerSelectedWithMovement:
            {
                // input abilities

                // input movement
                MovementMouse();

                // select a different character

                // confirm/end movement
                EndTurn();

                // cancel movement
                CancelMovement();

                break;
            }
            case (int)SelectorState.playerSelectedWithAbility:
            {
                // ranged aim

                // change direction

                // flip ability

                // use an ability

                // cancel ability

                break;
            }
        }
    }

    private void Select()
    {
        // clicking on objects in scene using raycasts from: https://www.youtube.com/watch?v=EANtTI6BCxk
        // because I'm dumb and I couldn't remember how to do it myself
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out hit))
        {
            if (hit.transform)
            {
                PlayerBase tmp = null;

                if (hit.transform.TryGetComponent(out tmp) && tmp != null && tmp != currentPlayer && !tmp.GetIdle())
                {
                    // deselect the previously selected player
                    if (currentPlayer != null)
                    {
                        currentPlayer.Deselected();
                    }

                    // assign the new current player
                    currentPlayer = tmp;

                    // keep track of their starting grid space
                    defaultGridSpace = currentPlayer.myGridSpace;

                    // let the newly selected player know they have been selected
                    currentPlayer.Selected(refCombatGrid);

                    // update UI
                    refCharacterUI.ToggleUI(true);
                    refCharacterUI.UpdateCharacterUI(currentPlayer);

                    // update the state machine
                    stateMachine.TryUpdateConnection((int)SelectorState.playerSelected);
                }
            }
        }
    }

    private void Deselect()
    {
        if (Input.GetMouseButtonDown(1))
        {
            currentPlayer.Deselected();
            currentPlayer = null;

            refCharacterUI.ToggleUI(false);

            stateMachine.TryUpdateConnection((int)SelectorState.doingNothing);
        }
    }

    private void MovementMouse()
    {
        // clicking on objects in scene using raycasts from: https://www.youtube.com/watch?v=EANtTI6BCxk
        // because I'm dumb and I couldn't remember how to do it myself
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButton(0) && Physics.Raycast(ray, out hit, layerMaskGrid))
        {
            if (hit.transform != null)
            {
                // if the A* movement was succesful (the character actually moved)
                if (currentPlayer.TryMoveAStar(refCombatGrid, refCombatGrid.GetGridSpace(hit.transform.gameObject)))
                {
                    // and the player didn't just move back to its original starting space
                    if (currentPlayer.myGridSpace != defaultGridSpace)
                    {
                        // update the state machine
                        stateMachine.TryUpdateConnection((int)SelectorState.playerSelectedWithMovement);
                    }
                    // but the player moved back to its original starting space
                    else
                    {
                        // update the state machine
                        stateMachine.TryUpdateConnection((int)SelectorState.playerSelected);
                    }
                }
            }
        }
    }

    private void Movement()
    {
        for (int i = 0; i < keysAndDirections.Length; ++i)
        {
            // if input is provided and the movement occurs successfully, update the state machine since we have moved
            if (Input.GetKeyDown(keysAndDirections[i].Item1) && currentPlayer.TryMove(keysAndDirections[i].Item2, refCombatGrid))
            {
                stateMachine.TryUpdateConnection((int)SelectorState.playerSelectedWithMovement);
            }
        }
    }

    private void CancelMovement()
    {
        if (Input.GetMouseButtonDown(1))
        {
            currentPlayer.ResetToDefaultPosition(defaultGridSpace);

            stateMachine.TryUpdateConnection((int)SelectorState.playerSelected);
        }
    }

    private void EndTurn()
    {
        // end turn by pressing spacebar
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EndTurnFunctionality();

            return;
        }
    }

    private void EndTurnFunctionality()
    {
        currentPlayer.EndTurn();
        currentPlayer = null;

        refCharacterUI.ToggleUI(false);

        refAbilityProcessor.CancelAbility();

        stateMachine.TryUpdateConnection((int)SelectorState.doingNothing);
    }

    public void SelectAbility(int abilNum)
    {
        Debug.Log("CharacterSelector, SelectAbility with parameter " + abilNum + ".");
    }

    public enum SelectorState
    {
        doingNothing = 0,
        playerSelected = 1,
        enemySelected = 2,
        playerSelectedWithMovement = 3,
        playerSelectedWithAbility = 4
    }
}

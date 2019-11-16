using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSelector : MonoBehaviour
{
    public PlayerBase currentPlayer = null;
    private bool atDefPos = true;
    private bool inputtedAbility = false;

    private CombatGrid refCombatGrid;

    private int savedAbilityNum;
    public CombatDirection facing = CombatDirection.up;
    public bool flipped = false;

    void Start()
    {
        refCombatGrid = FindObjectOfType<CombatGrid>();
    }

    void Update()
    {
        if (currentPlayer == null)
        {
            Select();
        }

        if (currentPlayer != null)
        {
            Movement();
            Flip();
            CancelOrSave();
            DoMoves();
        }
    }

    private void Select()
    {
        // only try to select a new current player if we're not already selecting something
        if (currentPlayer == null)
        {
            // clicking on objects in scene using raycasts from: https://www.youtube.com/watch?v=EANtTI6BCxk
            // because I'm dumb and I couldn't remember how to do it myself
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out hit))
            {
                if (hit.transform)
                {
                    // try to set currentPlayer if the hit object has a player component
                    currentPlayer = hit.transform.GetComponent<PlayerBase>();
                    if (currentPlayer.GetIdle() == true)
                    {
                        currentPlayer = null;
                    }
                }
            }
        }
    }

    private void Movement()
    {
        bool moved = false;

        if (Input.GetKeyDown(KeyCode.W))
        {
            ApplyNewDirection(CombatDirection.up, ref moved);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            ApplyNewDirection(CombatDirection.down, ref moved);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            ApplyNewDirection(CombatDirection.left, ref moved);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            ApplyNewDirection(CombatDirection.right, ref moved);
        }

        if (moved == true)
        {
            atDefPos = false;
        }
    }

    private void ApplyNewDirection(CombatDirection direction, ref bool moved)
    {
        // change the direction we're facing
        facing = direction;

        if (inputtedAbility)
        {
            // if an ability has been input, simply change direction and clean the grid without applying any effects
            refCombatGrid.CleanGridWithoutApplying();

            // then reattempt the ability
            TryPrepareAbility();
        }
        else
        {
            // try to move
            moved = currentPlayer.TryMove(direction);
        }
    }

    private void Flip()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            // toggle the flip
            flipped = !flipped;

            // clean the grid to update the rendering of the preview
            refCombatGrid.CleanGridWithoutApplying();

            // then try to use the ability
            TryPrepareAbility();
        }
    }

    private void DoMoves()
    {
        // before we commit to the ability, we prepare the one we want to use so that a direction can be selected
        if (!inputtedAbility)
        {
            // have the selected player prepare its ability given the input, the direction we are "facing", and whether or not we are "flipped"
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                savedAbilityNum = 1;

                TryPrepareAbility();

                inputtedAbility = true;
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                savedAbilityNum = 2;

                TryPrepareAbility();

                inputtedAbility = true;
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                savedAbilityNum = 3;

                TryPrepareAbility();

                inputtedAbility = true;
            }

            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                savedAbilityNum = 4;

                TryPrepareAbility();

                inputtedAbility = true;
            }
        }
        else
        {
            // if we have inputted an ability previously and a button is pressed again, commit to the ability
            if (Input.GetKeyDown(KeyCode.Alpha1) ||
                Input.GetKeyDown(KeyCode.Alpha2) ||
                Input.GetKeyDown(KeyCode.Alpha3) ||
                Input.GetKeyDown(KeyCode.Alpha4))
            {
                // end the selected player's turn
                EndTurn();

                // make the combat grid apply any effects applied by the player's ability
                refCombatGrid.CleanGrid();

                inputtedAbility = false;
            }
        }
    }

    private void TryPrepareAbility()
    {
        if (currentPlayer != null)
        {
            currentPlayer.PrepareAbility(savedAbilityNum, facing, flipped);
        }
    }

    private void CancelOrSave()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            EndTurn();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cancel();
        }
    }

    private void EndTurn()
    {
        currentPlayer.EndTurn();
        currentPlayer = null;
        atDefPos = true;
    }

    private void Cancel()
    {
        if (atDefPos)
        {
            currentPlayer = null;
        }
        else
        {
            currentPlayer.ResetToDefaultPosition();
            atDefPos = true;
        }
    }
}

public enum CombatDirection { up = 0, down, left, right };
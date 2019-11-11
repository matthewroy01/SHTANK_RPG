using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSelector : MonoBehaviour
{
    public PlayerBase currentPlayer = null;
    private bool atDefPos = true;

    private CombatGrid refCombatGrid;

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
            moved = currentPlayer.TryMove(CombatMovDir.up);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            moved = currentPlayer.TryMove(CombatMovDir.down);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            moved = currentPlayer.TryMove(CombatMovDir.left);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            moved = currentPlayer.TryMove(CombatMovDir.right);
        }

        if (moved == true)
        {
            atDefPos = false;
        }
    }

    private void DoMoves()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // have the selected player use its ability
            currentPlayer.UseAbility(1);

            // end the selected player's turn
            EndTurn();

            // make the combat grid apply any effects applied by the player's ability
            refCombatGrid.CleanGrid();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            currentPlayer.UseAbility(2);
            EndTurn();
            refCombatGrid.CleanGrid();
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            currentPlayer.UseAbility(3);
            EndTurn();
            refCombatGrid.CleanGrid();
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            currentPlayer.UseAbility(4);
            EndTurn();
            refCombatGrid.CleanGrid();
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

public enum CombatMovDir { up = 0, down, left, right }
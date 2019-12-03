﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSelector : MonoBehaviour
{
    public PlayerBase currentPlayer = null;
    private GridSpace defaultGridSpace;
    private bool atDefPos = true;
    private bool inputtedAbility = false;

    private CombatGrid refCombatGrid;
    private AbilityProcessor refAbilityProcessor;

    private int savedAbilityNum;
    public CombatDirection facing = CombatDirection.up;
    public bool flipped = false;

    void Start()
    {
        refCombatGrid = FindObjectOfType<CombatGrid>();
        refAbilityProcessor = FindObjectOfType<AbilityProcessor>();
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
                    else
                    {
                        defaultGridSpace = currentPlayer.myGridSpace;
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
            // try to process an ability based on the new direction
            TryProcessAbility();
        }
        else
        {
            // try to move
            moved = currentPlayer.TryMove(direction, refCombatGrid);
        }
    }

    private void Flip()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            // toggle the flip
            flipped = !flipped;

            // try to process an ability based on the new orienatation
            TryProcessAbility();
        }
    }

    private void DoMoves()
    {
        KeyCode[] keys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };

        // if we have inputted an ability previously and a button is pressed again, commit to the ability
        for (int i = 0; i < keys.Length; ++i)
        {
            if (Input.GetKeyDown(keys[i]) && savedAbilityNum == i + 1)
            {
                // end the selected player's turn
                EndTurn();

                // apply the currently saved ability
                refAbilityProcessor.ApplyAbility();

                // reset our previously inputted ability
                inputtedAbility = false;
                savedAbilityNum = 0;

                // don't bother checking for additional input this frame
                return;
            }
        }

        // if an ability hasn't already been input, or a new one is being input, save that ability's input
        for (int i = 0; i < keys.Length; ++i)
        {
            if (Input.GetKeyDown(keys[i]))
            {
                SaveAbilityInput(i + 1);
            }
        }
    }

    private void SaveAbilityInput(int num)
    {
        // have the selected player prepare its ability given the input, the direction we are "facing", and whether or not we are "flipped"
        savedAbilityNum = num;

        TryProcessAbility();

        inputtedAbility = true;
    }

    private void TryProcessAbility()
    {
        if (currentPlayer != null)
        {
            refAbilityProcessor.ProcessAbility(currentPlayer, savedAbilityNum, facing, flipped);
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
        // if an ability has been input, cancel it
        if (inputtedAbility)
        {
            inputtedAbility = false;
            savedAbilityNum = 0;
        }
        // otherwise, reset movement
        else
        {
            // if the character is already at their default position, deselect them
            if (atDefPos)
            {
                currentPlayer = null;
            }
            // otherwise move the character back to their default position
            else
            {
                currentPlayer.ResetToDefaultPosition(defaultGridSpace);
                atDefPos = true;
            }
        }
    }
}

public enum CombatDirection { up = 0, down, left, right };
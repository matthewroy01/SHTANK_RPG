﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class PlayerBase : Character
{
    private bool idle = true;
    private GridSpace originalGridSpace;

    private enum PathDirections { up, down, left, right };

    private bool selected = false;

    private void Start()
    {
        refMovementDialogueProcessor = GetComponent<MovementDialogueProcessor>();

        // this may be redundant, as the player manager initializes each player's stats
        healthCurrent = healthMax;
        movementRangeCurrent = movementRangeDefault;
    }

    private void Update()
    {
        if (selected)
        {
            placeholderRenderer.material.color = Color.Lerp(Color.Lerp(Color.blue, Color.white, 0.5f), Color.blue, Mathf.Sin(Time.time * 10.0f));
        }
        else
        {
            placeholderRenderer.material.color = Color.blue;
        }

        if (idle == true)
        {
            placeholderRenderer.material.color = Color.Lerp(Color.blue, Color.black, 0.75f);
        }
    }

    public void StartTurn(CombatGrid grid)
    {
        idle = false;
        originalGridSpace = myGridSpace;

        HandleStatuses();
        FindMovementSpaces(grid);

        // PASSIVE EVENT: BEGIN TURN
        passive.BeginTurn();
    }

    public bool TryMove(CombatDirection dir, CombatGrid grid)
    {
        bool result = false;

        GridSpace tmp = grid.TryMove(dir, myGridSpace, movementSpaces);

        if (tmp != myGridSpace)
        {
            result = true;
        }

        myGridSpace = tmp;
        transform.position = myGridSpace.obj.transform.position;

        return result;
    }

    public bool TryMoveAStar(CombatGrid grid, GridSpace target)
    {
        if (target != null && movementSpaces.Contains(target))
        {
            // try to path to the target GridSpace
            List<GridSpace> spaces;
            spaces = grid.GetAStar(grid, myGridSpace, target, this, true);

            // if a path was found and we are not already at the target, move there
            if (spaces.Count > 0 && myGridSpace != spaces[spaces.Count - 1])
            {
                myGridSpace = spaces[spaces.Count - 1];
                transform.position = myGridSpace.obj.transform.position;
                return true;
            }
        }

        return false;
    }

    public void Selected(CombatGrid grid)
    {
        selected = true;

        // apply any statuses again in case they have been cured
        // STATUSES LIKE POISON, THAT APPLY DAMAGE WILL NEED A SPECIAL CASE SO THEY DON'T GET APPLIED EVERY TIME THE PLAYER IS SELECTED
        HandleStatuses();

        // refind the potential movement spaces in case another character has moved since the turn began
        FindMovementSpaces(grid);

        if (refMovementDialogueProcessor != null)
        {
            refMovementDialogueProcessor.Display();
        }
    }

    public void Deselected()
    {
        selected = false;

        if (refMovementDialogueProcessor != null)
        {
            refMovementDialogueProcessor.Clear();
        }
    }

    public void MoveToGridSpace(GridSpace toMoveTo)
    {
        if (toMoveTo != null)
        {
            transform.position = toMoveTo.obj.transform.position;
            myGridSpace = toMoveTo;
        }
    }

    public void SaveMyGridSpace()
    {
        // update combat grid with new position
        originalGridSpace.character = null;
        myGridSpace.character = this;
        originalGridSpace = myGridSpace;
    }

    public void EndTurn()
    {
        SaveMyGridSpace();

        selected = false;
        idle = true;

        if (refMovementDialogueProcessor != null)
        {
            //refMovementDialogueProcessor.Clear();
        }
    }

    public bool GetIdle()
    {
        return idle;
    }

    public void ResetToDefaultPosition(GridSpace toReturnTo)
    {
        transform.position = originalGridSpace.obj.transform.position;
        myGridSpace = toReturnTo;
    }
}
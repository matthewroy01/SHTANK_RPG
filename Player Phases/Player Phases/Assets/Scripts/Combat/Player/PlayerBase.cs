using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

using DG.Tweening;

/*public class PlayerBase : Character
{
    private GridSpace originalGridSpace;

    private enum PathDirections { up, down, left, right };

    private bool selected = false;

    private void Start()
    {
        refMovementDialogueProcessor = GetComponent<MovementDialogueProcessor>();
        refCharacterEffectUI = GetComponent<EffectUI>();

        // this may be redundant, as the player manager initializes each player's stats
        movementRangeCurrent = movementRangeDefault;
    }

    private void Update()
    {
        if (selected)
        {
            placeholderRenderer.material.color = Color.Lerp(Color.Lerp(Color.white, Color.white * 0.5f, 0.5f), Color.white, Mathf.Sin(Time.time * 10.0f));
        }
        else
        {
            placeholderRenderer.material.color = Color.white;
        }

        if (idle == true)
        {
            placeholderRenderer.material.color = Color.Lerp(Color.white, Color.white * 0.5f, 0.75f);
        }
    }

    public override void StartTurn(CombatGrid grid)
    {
        movesetData.Reset(moveset);

        idle = false;
        originalGridSpace = myGridSpace;

        HandleStatuses();
        FindMovementSpaces(grid);

        // PASSIVE EVENT: BEGIN TURN
        SendEvent(PassiveEventID.turnStart);
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

    public override void Selected(CombatGrid grid)
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
            transform.DOMove(toMoveTo.obj.transform.position, 0.25f);
            myGridSpace = toMoveTo;
        }
    }

    public void MoveToGridSpacePath(List<GridSpace> spaces, GridSpace end)
    {
        if (spaces != null && spaces.Count > 1)
        {
            List<Vector3> positions = new List<Vector3>();

            for (int i = 0; i < spaces.Count; ++i)
            {
                positions.Add(spaces[i].obj.transform.position);

                if (spaces[i] == end)
                {
                    break;
                }
            }

            transform.DOPath(positions.ToArray(), 0.25f);
            myGridSpace = end;
        }
    }

    public void MoveToGridSpaceJump(GridSpace toMoveTo)
    {
        if (toMoveTo != null)
        {
            transform.DOJump(toMoveTo.obj.transform.position, 2.0f, 1, 0.25f);
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

        // PASSIVE EVENT: END TURN
        SendEvent(PassiveEventID.turnEnd);
    }

    public void ResetToDefaultPosition(GridSpace toReturnTo)
    {
        transform.position = toReturnTo.obj.transform.position;
        myGridSpace = toReturnTo;
    }
}*/
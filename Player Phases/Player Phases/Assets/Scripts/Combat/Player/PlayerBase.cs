using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class PlayerBase : Character
{
    private bool idle = true;
    private Vector3 defaultPosition;

    private CombatGrid refCombatGrid;

    public Moveset moveset;

    private enum PathDirections { up, down, left, right };

    private void Start()
    {
        refCombatGrid = FindObjectOfType<CombatGrid>();
    }

    private void Update()
    {
        if (transform.position != defaultPosition)
        {
            GetComponent<Renderer>().material.color = Color.Lerp(Color.Lerp(Color.blue, Color.white, 0.5f), Color.blue, Mathf.Sin(Time.time * 10.0f));
        }
        else
        {
            GetComponent<Renderer>().material.color = Color.blue;
        }

        if (idle == true)
        {
            GetComponent<Renderer>().material.color = Color.Lerp(Color.blue, Color.black, 0.75f);
        }
    }

    public void StartTurn()
    {
        idle = false;
        defaultPosition = transform.position;
    }

    public bool TryMove(CombatDirection dir)
    {
        bool result = false;

        GridSpace tmp = refCombatGrid.TryMove(dir, myGridSpace);

        if (tmp != myGridSpace)
        {
            result = true;
        }

        myGridSpace = tmp;
        transform.position = myGridSpace.obj.transform.position;

        return result;
    }

    public void PrepareAbility(int num, CombatDirection facing, bool flipped)
    {
        Ability abil = null;

        switch(num)
        {
            case 1:
            {
                abil = moveset.ability1;
                break;
            }
            case 2:
            {
                abil = moveset.ability2;
                break;
            }
            case 3:
            {
                abil = moveset.ability3;
                break;
            }
            case 4:
            {
                abil = moveset.ability4;
                break;
            }
        }

        if (abil != null)
        {
            string abilType = abil.GetType().Name;

            if (abilType == "PathAbility")
            {
                Debug.Log(abilType + " used.");
                ProcessPathAbility((PathAbility)abil, facing, flipped);
            }
            else if (abilType == "CircleAbility")
            {
                Debug.Log(abilType + " used.");
                ProcessCircleAbility((CircleAbility)abil);
            }
            else if (abilType == "ConeAbility")
            {
                Debug.Log(abilType + " used.");
                ProcessConeAbility((ConeAbility)abil, facing);
            }
            else if (abilType == "RectangleAbility")
            {
                Debug.Log(abilType + " used.");
            }
            else
            {
                Debug.LogError(abilType + " is not a valid Ability type.");
            }
        }
    }

    private void ProcessPathAbility(PathAbility abil, CombatDirection direction, bool flipped)
    {
        GridSpace currentGridSpace = myGridSpace;

        // depending on the direction the character is facing, the meaning of "forward", etc changes, so do something different for each case
        // PLACEHOLDER SWITCH STATEMENT, REPLACE WITH DIRECTION THE CHARACTER IS FACING

        if (flipped)
        {
            switch (direction)
            {
                // facing upwards
                case CombatDirection.up:
                {
                    MakePathDirty(abil, currentGridSpace, "up", "right", "left", "down");
                    break;
                }
                // facing downwards
                case CombatDirection.down:
                {
                    MakePathDirty(abil, currentGridSpace, "down", "left", "right", "up");
                    break;
                }
                // facing left
                case CombatDirection.left:
                {
                    MakePathDirty(abil, currentGridSpace, "left", "up", "down", "right");
                    break;
                }
                // facing right
                case CombatDirection.right:
                {
                    MakePathDirty(abil, currentGridSpace, "right", "down", "up", "left");
                    break;
                }
            }
        }
        else
        {
            switch (direction)
            {
                // facing upwards
                case CombatDirection.up:
                {
                    MakePathDirty(abil, currentGridSpace, "up", "left", "right", "down");
                    break;
                }
                // facing downwards
                case CombatDirection.down:
                {
                    MakePathDirty(abil, currentGridSpace, "down", "right", "left", "up");
                    break;
                }
                // facing left
                case CombatDirection.left:
                {
                    MakePathDirty(abil, currentGridSpace, "left", "down", "up", "right");
                    break;
                }
                // facing right
                case CombatDirection.right:
                {
                    MakePathDirty(abil, currentGridSpace, "right", "up", "down", "left");
                    break;
                }
            }
        }
    }

    private void MakePathDirty(PathAbility abil, GridSpace currentGridSpace, string forwards, string sideways, string sidewaysOpposite, string backwards)
    {
        // loop through all the segments of the path
        for (int i = 0; i < abil.path.Count; ++i)
        {
            // check the direction, and set that part of the segment dirty based on the given direction
            switch (abil.path[i].directions)
            {
                case AbilityDirection.forwards:
                {
                    MakePathSegmentDirty(abil, ref currentGridSpace, forwards, i);

                    break;
                }
                case AbilityDirection.sideways:
                {
                    MakePathSegmentDirty(abil, ref currentGridSpace, sideways, i);

                    break;
                }
                case AbilityDirection.sidewaysOpposite:
                {
                    MakePathSegmentDirty(abil, ref currentGridSpace, sidewaysOpposite, i);

                    break;
                }
                case AbilityDirection.backwards:
                {
                    MakePathSegmentDirty(abil, ref currentGridSpace, backwards, i);

                    break;
                }
                default:
                {
                    break;
                }
            }
        }   
    }

    private void MakePathSegmentDirty(PathAbility abil, ref GridSpace currentGridSpace, string direction, int i)
    {
        // for the amount of spaces specified for this part of the path, set each grid space dirty
        for (int j = 0; j < abil.path[i].amount; ++j)
        {
            // use reflection to get the field using a string
            // in this case, one of four GridSpaces (up, down, left, and right) representing connections
            if (!refCombatGrid.MakeDirty((GridSpace)currentGridSpace.GetType().GetField(direction).GetValue(currentGridSpace), abil))
            {
                // if the MakeDirty function returns false, we've reached the end of the grid and should stop
                return;
            }

            // update the current grid space to continue along the path
            currentGridSpace = (GridSpace)currentGridSpace.GetType().GetField(direction).GetValue(currentGridSpace);
        }
    }

    private void ProcessCircleAbility(CircleAbility abil)
    {
        List<GridSpace> toMakeDirty = new List<GridSpace>();
        Queue<GridSpace> toCheckNext = new Queue<GridSpace>();

        toCheckNext.Enqueue(myGridSpace);

        if (abil.radius > 0)
        {
            // FOR NOW WE ARE USING myGridSpace BUT IN THE FUTURE THIS SHOULD BE ABLE TO CHANGE FOR RANGED ATTACKS
            int radiusCounter = 0;
            int numToCheck = 0;

            // keep checking until the specified radius has been reached
            while (radiusCounter < abil.radius)
            {
                // keep track of how many GridSpaces there were in ToCheckNext so that we don't loop too much for GridSpaces we end up adding
                numToCheck = toCheckNext.Count;

                for (int j = 0; j < numToCheck; ++j)
                {
                    // check and potentially add all the connections currently in the toCheckNext queue
                    CheckListAndAdd(ref toMakeDirty, ref toCheckNext, toCheckNext.Peek().up);
                    CheckListAndAdd(ref toMakeDirty, ref toCheckNext, toCheckNext.Peek().down);
                    CheckListAndAdd(ref toMakeDirty, ref toCheckNext, toCheckNext.Peek().left);
                    CheckListAndAdd(ref toMakeDirty, ref toCheckNext, toCheckNext.Peek().right);

                    // remove the GridSpace we just checked
                    toCheckNext.Dequeue();
                }

                radiusCounter++;
            }
        }

        // include the center GridSpace (either the space the user is on or the center of a ranged attack)
        toMakeDirty.Add(myGridSpace);

        // mark all found GridSpaces as dirty
        for (int i = 0; i < toMakeDirty.Count; ++i)
        {
            refCombatGrid.MakeDirty(toMakeDirty[i], abil);
        }
    }

    private void CheckListAndAdd(ref List<GridSpace> toMakeDirty, ref Queue<GridSpace> toCheckNext, GridSpace toCheckAndMaybeAdd)
    {
        if (toCheckAndMaybeAdd != null)
        {
            // add the GridSpace we are checking to be made dirty if it's not already in the list
            if (!toMakeDirty.Contains(toCheckAndMaybeAdd))
            {
                // add the GridSpace to be made dirty
                toMakeDirty.Add(toCheckAndMaybeAdd);

                // and add the GridSpace to have its connections checked next too
                toCheckNext.Enqueue(toCheckAndMaybeAdd);
            }
        }
    }

    private void ProcessConeAbility(ConeAbility abil, CombatDirection direction)
    {
        GridSpace currentGridSpace = myGridSpace;

        // depending on the direction the character is facing, the meaning of "forward", etc changes, so do something different for each case
        // PLACEHOLDER SWITCH STATEMENT, REPLACE WITH DIRECTION THE CHARACTER IS FACING
        switch (direction)
        {
            // facing upwards
            case CombatDirection.up:
            {
                MakeConeDirty(abil, currentGridSpace, "up", "left", "right", "down");
                break;
            }
            // facing downwards
            case CombatDirection.down:
            {
                MakeConeDirty(abil, currentGridSpace, "down", "right", "left", "up");
                break;
            }
            // facing left
            case CombatDirection.left:
            {
                MakeConeDirty(abil, currentGridSpace, "left", "down", "up", "right");
                break;
            }
            // facing right
            case CombatDirection.right:
            {
                MakeConeDirty(abil, currentGridSpace, "right", "up", "down", "left");
                break;
            }
        }
    }

    private void MakeConeDirty(ConeAbility abil, GridSpace startingGridSpace, string forwards, string sideways, string sidewaysOpposite, string backwards)
    {
        GridSpace currentGridSpace = startingGridSpace;

        // loop through the length of this ability
        for (int i = 0; i < abil.length; ++i)
        {
            // for this loop, we move along a straight line and spread out in both directions
            // here, make the center grid space dirty (the one along the straight line)
            refCombatGrid.MakeDirty(currentGridSpace, abil);

            // row trackers to help spread out from the center line
            GridSpace rowTrackerSideways = currentGridSpace, rowTrackerSidewaysOpposite = currentGridSpace;

            // be careful to not divide by zero
            if (abil.angle != 0)
            {
                // slowly expand as we move outwards
                for (int j = 0; j < i / abil.angle; ++j)
                {
                    // make grid spaces dirty in both directions from the center line
                    refCombatGrid.MakeDirty(rowTrackerSideways = (GridSpace)rowTrackerSideways.GetType().GetField(sideways).GetValue(rowTrackerSideways), abil);
                    refCombatGrid.MakeDirty(rowTrackerSidewaysOpposite = (GridSpace)rowTrackerSidewaysOpposite.GetType().GetField(sidewaysOpposite).GetValue(rowTrackerSidewaysOpposite), abil);
                }
            }
            else
            {
                Debug.LogError("PlayerBase, MakeConeDirty, abil.angle cannot be 0. All Real Numbers can divide by zero, but you shouldn't.");
            }

            // update the current grid space
            currentGridSpace = (GridSpace)currentGridSpace.GetType().GetField(forwards).GetValue(currentGridSpace);
        }
    }

    public void EndTurn()
    {
        idle = true;
    }

    public bool GetIdle()
    {
        return idle;
    }

    public void ResetToDefaultPosition()
    {
        transform.position = defaultPosition;
    }
}
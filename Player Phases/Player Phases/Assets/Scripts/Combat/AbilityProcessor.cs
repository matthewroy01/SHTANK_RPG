using System.Collections.Generic;
using UnityEngine;

public class AbilityProcessor : MonoBehaviour
{
    private CombatGrid refCombatGrid;

    private List<GridSpace> gridSpaces = new List<GridSpace>();
    private List<GridSpace> startingSpaces = new List<GridSpace>();
    private Ability savedAbility;

    void Start()
    {
        refCombatGrid = FindObjectOfType<CombatGrid>();
    }

    private void Update()
    {
        // TEMPORARY CODE FOR SETTING THE COLOR OF THE GRID SPACES, THIS SHOULD PROBABLY BE HANDLED BY A SHADER
        // red for ranged view
        for (int i = 0; i < startingSpaces.Count; ++i)
        {
            startingSpaces[i].obj.GetComponent<Renderer>().material.color = Color.Lerp(Color.Lerp(Color.red, Color.white, 0.25f), Color.red, Mathf.Sin(Time.time * 10.0f + (0.5f * i)));
        }

        // TEMPORARY CODE FOR SETTING THE COLOR OF THE GRID SPACES, THIS SHOULD PROBABLY BE HANDLED BY A SHADER
        // green for attack preview
        for (int i = 0; i < gridSpaces.Count; ++i)
        {
            gridSpaces[i].obj.GetComponent<Renderer>().material.color = Color.Lerp(Color.Lerp(Color.green, Color.white, 0.5f), Color.green, Mathf.Sin(Time.time * 10.0f + (0.5f * i)));
        }
    }

    public void ProcessAbility(PlayerBase player, GridSpace startingSpace, int abilNum, CombatDirection facing, bool flipped)
    {
        CancelAbility();

        if (startingSpace == null)
        {
            startingSpace = player.myGridSpace;
        }

        switch (abilNum)
        {
            case 1:
            {
                savedAbility = player.moveset.ability1;
                break;
            }
            case 2:
            {
                savedAbility = player.moveset.ability2;
                break;
            }
            case 3:
            {
                savedAbility = player.moveset.ability3;
                break;
            }
            case 4:
            {
                savedAbility = player.moveset.ability4;
                break;
            }
        }

        if (savedAbility != null)
        {
            string abilType = savedAbility.GetType().Name;

            // get possible starting grid spaces
            startingSpaces.AddRange(refCombatGrid.GetBreadthFirst(player.myGridSpace, savedAbility.range, TerrainTypePresets.onlyStandard));

            // make sure the specified starting grid space is a valid starting space contained within startingSpaces
            if (startingSpaces.Contains(startingSpace))
            {
                // process ability
                if (abilType == "PathAbility")
                {
                    Debug.Log(abilType + " processed.");
                    ProcessPathAbility((PathAbility)savedAbility, startingSpace, facing, flipped);
                }
                else if (abilType == "CircleAbility")
                {
                    Debug.Log(abilType + " processed.");
                    ProcessCircleAbility((CircleAbility)savedAbility, startingSpace);
                }
                else if (abilType == "ConeAbility")
                {
                    Debug.Log(abilType + " processed.");
                    ProcessConeAbility((ConeAbility)savedAbility, startingSpace, facing);
                }
                else if (abilType == "RectangleAbility")
                {
                    Debug.Log(abilType + " processed.");
                }
                else
                {
                    Debug.LogError(abilType + " is not a valid Ability type.");
                }
            }
        }
    }

    private void CancelAbility()
    {
        // TEMPORARY CODE FOR RESETTING THE COLOR OF THE GRID SPACES, THIS SHOULD PROBABLY BE HANDLED BY A SHADER
        for (int i = 0; i < gridSpaces.Count; ++i)
        {
            gridSpaces[i].obj.GetComponent<Renderer>().material.color = Color.white;
        }

        // TEMPORARY CODE FOR RESETTING THE COLOR OF THE GRID SPACES, THIS SHOULD PROBABLY BE HANDLED BY A SHADER
        for (int i = 0; i < startingSpaces.Count; ++i)
        {
            startingSpaces[i].obj.GetComponent<Renderer>().material.color = Color.white;
        }

        gridSpaces.Clear();
        startingSpaces.Clear();
        savedAbility = null;
    }

    public bool ApplyAbility()
    {
        if (gridSpaces.Count > 0 && savedAbility != null)
        {
            // set saved grid spaces as dirty
            for (int i = 0; i < gridSpaces.Count; ++i)
            {
                refCombatGrid.MakeDirty(gridSpaces[i], savedAbility);
            }

            string abilType = savedAbility.GetType().Name;
            Debug.Log(abilType + " applied.");

            // and apply their saved effects
            refCombatGrid.CleanGrid();

            // then erase whatever ability information is currently saved
            CancelAbility();

            return true;
        }

        return false;
    }

    private void ProcessPathAbility(PathAbility abil, GridSpace startingGridSpace, CombatDirection direction, bool flipped)
    {
        GridSpace currentGridSpace = startingGridSpace;

        // depending on the direction the character is facing, the meaning of "forward", etc changes, so do something different for each case
        // PLACEHOLDER SWITCH STATEMENT, REPLACE WITH DIRECTION THE CHARACTER IS FACING

        if (flipped)
        {
            switch (direction)
            {
                // facing upwards
                case CombatDirection.up:
                {
                    SavePath(abil, currentGridSpace, "up", "right", "left", "down");
                    break;
                }
                // facing downwards
                case CombatDirection.down:
                {
                    SavePath(abil, currentGridSpace, "down", "left", "right", "up");
                    break;
                }
                // facing left
                case CombatDirection.left:
                {
                    SavePath(abil, currentGridSpace, "left", "up", "down", "right");
                    break;
                }
                // facing right
                case CombatDirection.right:
                {
                    SavePath(abil, currentGridSpace, "right", "down", "up", "left");
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
                    SavePath(abil, currentGridSpace, "up", "left", "right", "down");
                    break;
                }
                // facing downwards
                case CombatDirection.down:
                {
                    SavePath(abil, currentGridSpace, "down", "right", "left", "up");
                    break;
                }
                // facing left
                case CombatDirection.left:
                {
                    SavePath(abil, currentGridSpace, "left", "down", "up", "right");
                    break;
                }
                // facing right
                case CombatDirection.right:
                {
                    SavePath(abil, currentGridSpace, "right", "up", "down", "left");
                    break;
                }
            }
        }
    }

    private void SavePath(PathAbility abil, GridSpace currentGridSpace, string forwards, string sideways, string sidewaysOpposite, string backwards)
    {
        // loop through all the segments of the path
        for (int i = 0; i < abil.path.Count; ++i)
        {
            // check the direction, and save that part of the segment based on the given direction
            switch (abil.path[i].directions)
            {
                case AbilityDirection.forwards:
                {
                    SavePathSegment(abil, ref currentGridSpace, forwards, i);

                    break;
                }
                case AbilityDirection.sideways:
                {
                    SavePathSegment(abil, ref currentGridSpace, sideways, i);

                    break;
                }
                case AbilityDirection.sidewaysOpposite:
                {
                    SavePathSegment(abil, ref currentGridSpace, sidewaysOpposite, i);

                    break;
                }
                case AbilityDirection.backwards:
                {
                    SavePathSegment(abil, ref currentGridSpace, backwards, i);

                    break;
                }
                default:
                {
                    break;
                }
            }
        }
    }

    private void SavePathSegment(PathAbility abil, ref GridSpace currentGridSpace, string direction, int i)
    {
        GridSpace toMoveTo = currentGridSpace;

        // for the amount of spaces specified for this part of the path, save each grid space
        for (int j = 0; j < abil.path[i].amount; ++j)
        {
            // use reflection to get the field using a string
            // in this case, one of four GridSpaces (up, down, left, and right) representing connections
            if (!TryAddGridSpace((GridSpace)currentGridSpace.GetType().GetField(direction).GetValue(currentGridSpace)))
            {
                return;
            }

            // update the current grid space to continue along the path
            currentGridSpace = (GridSpace)currentGridSpace.GetType().GetField(direction).GetValue(currentGridSpace);
            toMoveTo = currentGridSpace;
        }
    }

    private void ProcessCircleAbility(CircleAbility abil, GridSpace startingGridSpace)
    {
        List<GridSpace> toSave = new List<GridSpace>();
        Queue<GridSpace> toCheckNext = new Queue<GridSpace>();

        toCheckNext.Enqueue(startingGridSpace);

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
                    CheckListAndAdd(ref toSave, ref toCheckNext, toCheckNext.Peek().up);
                    CheckListAndAdd(ref toSave, ref toCheckNext, toCheckNext.Peek().down);
                    CheckListAndAdd(ref toSave, ref toCheckNext, toCheckNext.Peek().left);
                    CheckListAndAdd(ref toSave, ref toCheckNext, toCheckNext.Peek().right);

                    // remove the GridSpace we just checked
                    toCheckNext.Dequeue();
                }

                radiusCounter++;
            }
        }

        // include the center GridSpace (either the space the user is on or the center of a ranged attack)
        toSave.Add(startingGridSpace);

        // add grid spaces to save to our list
        for (int i = 0; i < toSave.Count; ++i)
        {
            TryAddGridSpace(toSave[i]);
        }
    }

    private void CheckListAndAdd(ref List<GridSpace> toSave, ref Queue<GridSpace> toCheckNext, GridSpace toCheckAndMaybeAdd)
    {
        if (toCheckAndMaybeAdd != null)
        {
            // add the GridSpace we are checking to save if it's not already in the list
            if (!toSave.Contains(toCheckAndMaybeAdd))
            {
                // add the GridSpace to save
                toSave.Add(toCheckAndMaybeAdd);

                // and add the GridSpace to have its connections checked next too
                toCheckNext.Enqueue(toCheckAndMaybeAdd);
            }
        }
    }

    private void ProcessConeAbility(ConeAbility abil, GridSpace startingGridSpace, CombatDirection direction)
    {
        GridSpace currentGridSpace = startingGridSpace;

        // depending on the direction the character is facing, the meaning of "forward", etc changes, so do something different for each case
        // PLACEHOLDER SWITCH STATEMENT, REPLACE WITH DIRECTION THE CHARACTER IS FACING
        switch (direction)
        {
            // facing upwards
            case CombatDirection.up:
            {
                SaveCone(abil, currentGridSpace, "up", "left", "right", "down");
                break;
            }
            // facing downwards
            case CombatDirection.down:
            {
                SaveCone(abil, currentGridSpace, "down", "right", "left", "up");
                break;
            }
            // facing left
            case CombatDirection.left:
            {
                SaveCone(abil, currentGridSpace, "left", "down", "up", "right");
                break;
            }
            // facing right
            case CombatDirection.right:
            {
                SaveCone(abil, currentGridSpace, "right", "up", "down", "left");
                break;
            }
        }
    }

    private void SaveCone(ConeAbility abil, GridSpace startingGridSpace, string forwards, string sideways, string sidewaysOpposite, string backwards)
    {
        GridSpace currentGridSpace = startingGridSpace;

        // loop through the length of this ability
        for (int i = 0; i < abil.length; ++i)
        {
            // for this loop, we move along a straight line and spread out in both directions
            // here, save the center grid space (the one along the straight line)
            TryAddGridSpace(currentGridSpace);

            if (currentGridSpace != null)
            {
                // row trackers to help spread out from the center line
                GridSpace rowTrackerSideways = currentGridSpace, rowTrackerSidewaysOpposite = currentGridSpace;

                // be careful to not divide by zero
                if (abil.angle != 0)
                {
                    // slowly expand as we move outwards
                    for (int j = 0; j < i / abil.angle; ++j)
                    {
                        if (rowTrackerSideways != null)
                        {
                            // each step here broken into separate lines for ease of debugging
                            // full line would be: rowTrackerSideways = (GridSpace)rowTrackerSideways.GetType().GetField(sideways).GetValue(rowTrackerSideways);
                            System.Type type = rowTrackerSideways.GetType();
                            System.Reflection.FieldInfo fieldInfo = type.GetField(sideways);
                            object value = fieldInfo.GetValue(rowTrackerSideways);
                            rowTrackerSideways = (GridSpace)value;
                        }

                        if (rowTrackerSidewaysOpposite != null)
                        {
                            // each step here broken into separate lines for ease of debugging
                            // full line would be: rowTrackerSidewaysOpposite = (GridSpace)rowTrackerSidewaysOpposite.GetType().GetField(sidewaysOpposite).GetValue(rowTrackerSidewaysOpposite);
                            System.Type typeO = rowTrackerSidewaysOpposite.GetType();
                            System.Reflection.FieldInfo fieldInfoO = typeO.GetField(sidewaysOpposite);
                            object valueO = fieldInfoO.GetValue(rowTrackerSidewaysOpposite);
                            rowTrackerSidewaysOpposite = (GridSpace)valueO;
                        }

                        // save grid spaces in both directions from the center line
                        TryAddGridSpace(rowTrackerSideways);
                        TryAddGridSpace(rowTrackerSidewaysOpposite);
                    }
                }
                else
                {
                    Debug.LogError("PlayerBase, SaveCone, abil.angle cannot be 0. All Real Numbers can divide by zero, but you shouldn't.");
                }

                // update the current grid space
                currentGridSpace = (GridSpace)currentGridSpace.GetType().GetField(forwards).GetValue(currentGridSpace);
            }
        }
    }

    private bool TryAddGridSpace(GridSpace target)
    {
        // if the grid space exists...
        if (target != null)
        {
            // save it to our list
            gridSpaces.Add(target);

            return true;
        }

        return false;
    }

    public Ability GetAbility()
    {
        return savedAbility;
    }
}
using System.Collections.Generic;
using UnityEngine;

public class AbilityProcessor : MonoBehaviour
{
    private CombatGrid refCombatGrid;

    private List<GridSpace> gridSpaces = new List<GridSpace>();
    private List<GridSpace> startingSpaces = new List<GridSpace>();
    private GridSpace endingSpace;

    private Ability savedAbility;
    private PlayerBase savedPlayer;

    void Start()
    {
        refCombatGrid = FindObjectOfType<CombatGrid>();
    }

    public void ProcessAbility(PlayerBase player, GridSpace startingSpace, int abilNum, CombatDirection facing, bool flipped)
    {
        CancelAbility();

        if (startingSpace == null)
        {
            startingSpace = player.myGridSpace;
        }

        // save the supplied ability
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

        // save the supplied player
        savedPlayer = player;

        if (savedAbility != null)
        {
            string abilType = savedAbility.GetType().Name;

            // get possible starting grid spaces
            startingSpaces.AddRange(refCombatGrid.GetBreadthFirst(player.myGridSpace, savedAbility.range, GetValidTerrainTypes(), Character_Affiliation.none));

            // if the ability causes the character to move, remove spaces with characters so the character can't occupy the same space as another character
            if (savedAbility.moveCharacter)
            {
                RemoveCharactersFromStartingSpaces(player);
            }

            // make sure the specified starting grid space is a valid starting space contained within startingSpaces
            if (startingSpaces.Contains(startingSpace))
            {
                if (!savedAbility.moveCharacter)
                {
                    endingSpace = player.myGridSpace;
                }

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

    private void RemoveCharactersFromStartingSpaces(Character player)
    {
        // loop through starting spaces and remove characters
        for (int i = 0; i < startingSpaces.Count; ++i)
        {
            if (player != null)
            {
                // don't remove the current player's grid space if we provide one
                if (startingSpaces[i].character != null && startingSpaces[i].character != player)
                {
                    startingSpaces.RemoveAt(i);
                    i--;
                }
            }
            else
            {
                // otherwise, remove all grid spaces with characters
                if (startingSpaces[i].character != null)
                {
                    startingSpaces.RemoveAt(i);
                    i--;
                }
            }
        }
    }

    public void CancelAbility()
    {
        gridSpaces.Clear();
        startingSpaces.Clear();
        savedAbility = null;
        savedPlayer = null;
    }

    public bool ApplyAbilityCheck()
    {
        return gridSpaces.Count > 0 && savedAbility != null;
    }

    public void ApplyAbility()
    {
        // add source to ability for use with things like aggro and friendly fire
        savedAbility.ApplySource(savedPlayer);

        // set saved grid spaces as dirty
        for (int i = 0; i < gridSpaces.Count; ++i)
        {
            refCombatGrid.MakeDirty(gridSpaces[i], savedAbility);
        }

        string abilType = savedAbility.GetType().Name;
        Debug.Log(abilType + " applied.");

        // move the player to the saved ending space
        savedPlayer.MoveToGridSpace(endingSpace);

        // and apply their saved effects
        refCombatGrid.CleanGrid();

        // then erase whatever ability information is currently saved
        CancelAbility();
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
                    if (!SavePathSegment(abil, ref currentGridSpace, forwards, i))
                    {
                        return;
                    }

                    break;
                }
                case AbilityDirection.sideways:
                {
                    if (!SavePathSegment(abil, ref currentGridSpace, sideways, i))
                    {
                        return;
                    }

                    break;
                }
                case AbilityDirection.sidewaysOpposite:
                {
                    if (!SavePathSegment(abil, ref currentGridSpace, sidewaysOpposite, i))
                    {
                        return;
                    }

                    break;
                }
                case AbilityDirection.backwards:
                {
                    if (!SavePathSegment(abil, ref currentGridSpace, backwards, i))
                    {
                        return;
                    }

                    break;
                }
                default:
                {
                    break;
                }
            }
        }
    }

    private bool SavePathSegment(PathAbility abil, ref GridSpace currentGridSpace, string direction, int i)
    {
        // for the amount of spaces specified for this part of the path, save each grid space
        for (int j = 0; j < abil.path[i].amount; ++j)
        {
            // use reflection to get the field using a string
            // in this case, one of four GridSpaces (up, down, left, and right) representing connections
            if (!TryAddGridSpace((GridSpace)currentGridSpace.GetType().GetField(direction).GetValue(currentGridSpace), abil.ignoreWalls))
            {
                return false;
            }

            // update the current grid space to continue along the path
            currentGridSpace = (GridSpace)currentGridSpace.GetType().GetField(direction).GetValue(currentGridSpace);

            // update the ending space if the character should move
            if (savedAbility.moveCharacter)
            {
                endingSpace = GetValidEndingSpace(gridSpaces);
            }
        }

        return true;
    }

    private void ProcessCircleAbility(CircleAbility abil, GridSpace startingGridSpace)
    {
        gridSpaces.AddRange(refCombatGrid.GetBreadthFirst(startingGridSpace, abil.radius, GetValidTerrainTypes(), Character_Affiliation.none));

        if (abil.moveCharacter)
        {
            endingSpace = startingGridSpace;
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
        List<GridSpace> centerLine = new List<GridSpace>();

        // loop through the length of this ability
        for (int i = 0; i < abil.length; ++i)
        {
            // for this loop, we move along a straight line and spread out in both directions
            // here, save the center grid space (the one along the straight line)
            if (TryAddGridSpace(currentGridSpace, abil.ignoreWalls) && savedAbility.moveCharacter)
            {
                // keep track of the center line to create a consistent result for needing to move out of the way of another character following a cone ability
                centerLine.Add(currentGridSpace);

                // if the space was added properly and the ability should move the character, update the ending space
                endingSpace = GetValidEndingSpace(centerLine);
            }

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
                        TryAddGridSpace(rowTrackerSideways, abil.ignoreWalls);
                        TryAddGridSpace(rowTrackerSidewaysOpposite, abil.ignoreWalls);
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

    private bool TryAddGridSpace(GridSpace target, bool ignoreWalls)
    {
        // if the grid space exists...
        if (target != null)
        {
            // if the terrain is a type of wall, check if we should igrore it
            if (target.GetTerrainType() == GridSpace_TerrainType.wall || target.GetTerrainType() == GridSpace_TerrainType.wall_artificial)
            {
                if (ignoreWalls)
                {
                    // save it to our list
                    gridSpaces.Add(target);

                    return true;
                }
            }
            // otherwise, add it to the list
            else
            {
                // save it to our list
                gridSpaces.Add(target);

                return true;
            }
        }

        return false;
    }

    public Ability GetAbility()
    {
        return savedAbility;
    }

    private List<GridSpace_TerrainType> GetValidTerrainTypes()
    {
        if (savedAbility != null && savedPlayer != null)
        {
            List<GridSpace_TerrainType> validTerrainTypes = new List<GridSpace_TerrainType>(savedPlayer.terrainTypes);

            if (savedAbility.moveCharacter)
            {
                validTerrainTypes = new List<GridSpace_TerrainType>(TerrainTypePresets.onlyStandard);
            }
            else if (savedAbility.ignoreWalls)
            {
                validTerrainTypes.Add(GridSpace_TerrainType.wall);
                validTerrainTypes.Add(GridSpace_TerrainType.wall_artificial);
            }

            return validTerrainTypes;
        }

        return null;
    }

    private GridSpace GetValidEndingSpace(List<GridSpace> spaces)
    {
        for (int i = spaces.Count - 1; i >= 0; --i)
        {
            if (spaces[i].character == null)
            {
                return spaces[i];
            }
        }

        return spaces[spaces.Count - 1];
    }

    public List<GridSpace> GetGridSpaces()
    {
        return gridSpaces;
    }

    public List<GridSpace> GetStartingSpaces()
    {
        return startingSpaces;
    }
}
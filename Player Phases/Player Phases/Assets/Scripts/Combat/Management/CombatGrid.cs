using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatGrid : MonoBehaviour
{
    public uint gridWidth;
    public uint gridHeight;

    public LayerMask scannable;

    // the grid used in combat
    [HideInInspector]
    public GridSpace[,] grid;

    // list of grid spaces marked as "dirty"
    [HideInInspector]
    public Stack<GridSpace> dirty = new Stack<GridSpace>();

    public GameObject gridSpacePrefab;
    public GameObject shadowWallPrefab;

    public bool yUp;

    const float antiZFightBuffer = 0.05f;

    public void SpawnGrid(Vector3 center)
    {
        grid = new GridSpace[gridWidth, gridHeight];

        // find the bottom left of our grid based on the point of contact in the overworld
        Vector3 bottomLeft = Vector3.zero;
        if (yUp)
        {
            bottomLeft = new Vector3(center.x - (gridWidth / 2.0f), 0, center.z - (gridHeight / 2.0f));
            bottomLeft.x = Mathf.RoundToInt(bottomLeft.x);
            bottomLeft.z = Mathf.RoundToInt(bottomLeft.z);
        }
        else
        {
            bottomLeft = new Vector3(center.x - (gridWidth / 2.0f), center.y - (gridHeight / 2.0f), center.z);
        }

        // calculate offset and adjust the bottom left corner based on the grid overlapping with any empty space
        Vector2 offset = GetCenterOffset(center);
        bottomLeft += new Vector3(offset.x, 0.0f, offset.y);
        Debug.Log("Offset was " + offset);

        // loop through to create all grid spaces
        for (int x = 0; x < gridWidth; ++x)
        {
            for (int y = 0; y < gridHeight; ++y)
            {
                // fire a raycast to scan for special terrain types
                RaycastHit hit;
                if (yUp)
                {
                    Physics.Raycast(new Vector3(x, 100.0f, y) + bottomLeft, Vector3.down, out hit, Mathf.Infinity, scannable);
                }
                else
                {
                    Physics.Raycast(new Vector3(x, y, -100.0f) + bottomLeft, Vector3.forward, out hit, Mathf.Infinity, scannable);
                }

                // set the terrain type to standard by default
                GridSpace_TerrainType terrainType = GridSpace_TerrainType.standard;

                // get the terrain type from the transform's tag if we found something with the raycast
                if (hit.transform != null)
                {
                    terrainType = GetTerrainTypeFromTag(hit.transform.tag);
                }

                float gridSpaceHeight = hit.transform != null ? hit.point.y : 0;

                // create new Grid Space with associated Game Object and terrain type from scan
                if (yUp)
                {
                    grid[x, y] = new GridSpace(Instantiate(gridSpacePrefab, new Vector3(x, gridSpaceHeight, y) + bottomLeft, Quaternion.identity, transform), terrainType, new Vector2Int(x, y));
                }
                else
                {
                    grid[x, y] = new GridSpace(Instantiate(gridSpacePrefab, new Vector3(x, y, 0.0f) + bottomLeft, Quaternion.identity, transform), terrainType, new Vector2Int(x, y));
                }
                grid[x, y].shadowWall = Instantiate(shadowWallPrefab, grid[x, y].obj.transform.position, grid[x, y].obj.transform.rotation, grid[x, y].obj.transform);
                grid[x, y].shadowWall.SetActive(false);
                grid[x, y].obj.name = "Grid Space (" + x + ", " + y + ")";
            }
        }

        // loop through again to set up connections
        for (int x = 0; x < gridWidth; ++x)
        {
            for (int y = 0; y < gridHeight; ++y)
            {
                // set connections where applicable
                // left
                if (x > 0)
                {
                    grid[x, y].left = grid[x - 1, y];
                }

                // down
                if (y > 0)
                {
                    grid[x, y].down = grid[x, y - 1];
                }

                // right
                if (x < gridWidth - 1)
                {
                    grid[x, y].right = grid[x + 1, y];
                }

                // up
                if (y < gridHeight - 1)
                {
                    grid[x, y].up = grid[x, y + 1];
                }
            }
        }
    }

    private Vector2 GetCenterOffset(Vector3 center)
    {
        Vector2 offset = Vector2.zero;

        // check right
        offset.x -= CheckForVoid(center, gridWidth / 2, true, false);
        // check left
        offset.x += CheckForVoid(center, gridWidth / 2, true, true);

        // check up
        offset.y -= CheckForVoid(center, gridHeight / 2, false, false);
        // check down
        offset.y += CheckForVoid(center, gridHeight / 2, false, true);

        if (offset.x < 0)
        {
            offset.x++;
        }
        if (offset.y < 0)
        {
            offset.y++;
        }
        return offset;
    }

    private int CheckForVoid(Vector3 center, float distance, bool horizontal, bool negative)
    {
        int counter = 0;

        int hor = horizontal ? 1 : 0;
        int ver = horizontal ? 0 : 1;

        if (negative)
        {
            hor *= -1;
            ver *= -1;
        }

        RaycastHit hit;

        for (int i = 1; i < distance + 1; ++i)
        {
            Physics.Raycast(new Vector3(center.x + (i * hor), 100.0f, center.z + (i * ver)), Vector3.down, out hit, Mathf.Infinity, scannable);
            if (hit.transform == null)
            {
                counter++;
            }
        }

        return counter;
    }

    public void CleanGrid()
    {
        Debug.Log("Cleaning grid... " + dirty.Count + " grid spaces were set as dirty.");

        // loop through all dirty grid spaces and apply their effects
        while (dirty.Count > 0)
        {
            dirty.Pop().Apply();
        }
    }

    public void CleanGridWithoutApplying()
    {
        Debug.Log("Cleaning grid without applying... " + dirty.Count + " grid spaces were set as dirty.");

        // loop through all dirty grid spaces and apply their effects
        while (dirty.Count > 0)
        {
            dirty.Pop().RemoveAllEffects();
        }
    }

    public bool MakeDirty(GridSpace target, Ability ability)
    {
        if (target != null)
        {
            // add effects to target grid space
            for (int i = 0; i < ability.effects.Count; ++i)
            {
                target.AddEffect(ability.effects[i]);
            }

            // add the target to the list of dirty grid spaces
            dirty.Push(target);

            return true;
        }

        Debug.Log("Target was null...");

        // return false if the target grid space was null
        // this can be used to tell scripts trying to apply abiliy effects that they've gone off the grid
        return false;
    }

    public GridSpace TryMove(CombatDirection dir, GridSpace current, List<GridSpace> movementSpaces)
    {
        System.Tuple<int, int> indices = GetIndicesOfGridSpace(current);
        GridSpace result = null;

        switch (dir)
        {
            case CombatDirection.up:
            {
                if (CheckSpace(indices.Item1, indices.Item2 + 1))
                {
                    result = grid[indices.Item1, indices.Item2 + 1];
                }
                break;
            }
            case CombatDirection.down:
            {
                if (CheckSpace(indices.Item1, indices.Item2 - 1))
                {
                    result = grid[indices.Item1, indices.Item2 - 1];
                }
                break;
            }
            case CombatDirection.left:
            {
                if (CheckSpace(indices.Item1 - 1, indices.Item2))
                {
                    result = grid[indices.Item1 - 1, indices.Item2];
                }
                break;
            }
            case CombatDirection.right:
            {
                if (CheckSpace(indices.Item1 + 1, indices.Item2))
                {
                    result = grid[indices.Item1 + 1, indices.Item2];
                }
                break;
            }
        }

        // if we found a result that's not null, check if it's one of the potential movement spaces
        if (result != null && movementSpaces.Contains(result))
        {
            return result;
        }

        return current;
    }

    private System.Tuple<int, int> GetIndicesOfGridSpace(GridSpace check)
    {
        GridSpace tmp;
        System.Tuple<int, int> result = new System.Tuple<int, int>(-1, -1);

        for (int i = 0; i < grid.GetLength(0); ++i)
        {
            for (int j = 0; j < grid.GetLength(1); ++j)
            {
                tmp = grid[i, j];

                if (tmp == check)
                {
                    result = new System.Tuple<int, int>(i, j);
                }
            }
        }

        return result;
    }

    private bool CheckSpace(int x, int y)
    {
        // temporary space check, later we will need to check if a space is a wall or otherwise
        // we also need to check if the space is occupied by an ally, but for now, it's convenient in testing abilities
        if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
        {
            return true;
        }

        return false;
    }

    public GridSpace GetGridSpace(GameObject obj)
    {
        // loop through grid and try to find the grid space given the provided GameObject
        for (int i = 0; i < grid.GetLength(0); ++i)
        {
            for (int j = 0; j < grid.GetLength(1); ++j)
            {
                // check if the GameObject is the grid space's GameObject
                if (grid[i, j].obj == obj)
                {
                    return grid[i, j];
                }
            }
        }

        return null;
    }

    public List<GridSpace> GetBreadthFirst(GridSpace center, uint radius, List<GridSpace_TerrainType> terrainTypes, Character_Affiliation affiliation)
    {
        List<GridSpace> result = new List<GridSpace>();
        result.Add(center);

        List<GridSpace> currentSweep = new List<GridSpace>();
        List<GridSpace> nextSweep = new List<GridSpace>();

        // add first Grid Space to serve as the center
        currentSweep.Add(center);

        // loop through the radius of our search
        for (int i = 0; i < radius; ++i)
        {
            // loop through the currently saved "sweep" of Grid Spaces
            for (int j = 0; j < currentSweep.Count; ++j)
            {
                BreadthFirstAddToLists(currentSweep[j].up, result, nextSweep, terrainTypes, affiliation);
                BreadthFirstAddToLists(currentSweep[j].down, result, nextSweep, terrainTypes, affiliation);
                BreadthFirstAddToLists(currentSweep[j].left, result, nextSweep, terrainTypes, affiliation);
                BreadthFirstAddToLists(currentSweep[j].right, result, nextSweep, terrainTypes, affiliation);
            }

            // reset the current sweep and reassign its conets to that of the next sweep
            currentSweep.Clear();
            currentSweep.AddRange(nextSweep);
            nextSweep.Clear();
        }

        return result;
    }

    private void BreadthFirstAddToLists(GridSpace space, List<GridSpace> result, List<GridSpace> nextSweep, List<GridSpace_TerrainType> terrainTypes, Character_Affiliation affiliation)
    {
        if (space != null && !result.Contains(space))
        {
            if (terrainTypes.Contains(space.GetTerrainType()))
            {
                if (space.character != null && (affiliation != Character_Affiliation.none && space.character.affiliation != affiliation))
                {
                    //Debug.Log("Terrain had character " + space.character.name + " and was ignored.");
                    return;
                }

                result.Add(space);
                nextSweep.Add(space);
            }
            else
            {
                //Debug.Log("Terrain of type " + space.GetTerrainType() + " was ignored.");
            }
        }
    }

    public List<GridSpace> GetAStar(CombatGrid refCombatGrid, GridSpace start, GridSpace target, Character subject, bool includeTarget)
    {
        AStarInitializeCosts(refCombatGrid.grid, start, target);

        // store all nodes where F cost has been calculated
        List<GridSpace> open = new List<GridSpace>();
        // store all nodes that have been evaluated
        List<GridSpace> closed = new List<GridSpace>();
        // nodes to return once the path is found
        List<GridSpace> result = new List<GridSpace>();

        GridSpace current = null;

        open.Add(start);

        while (open.Count > 0)
        {
            // set current to the node with the lowest f cost
            current = AStarGetLowestFCost(open);

            // remove current from the open list
            open.Remove(current);

            // add current to the closed list
            closed.Add(current);

            // if current is equal to the target, we have reached our destination
            if (current == target)
            {
                // only include the target if specified
                // not including the target helps enemies get within one space of the enemy to attack
                if (includeTarget)
                {
                    result.Add(current);
                }

                // loop back through the pathing connections until we reach the start again to find our path
                while (current != start)
                {
                    result.Add(current.pathingConnection);
                    current = current.pathingConnection;
                }

                result.Reverse();
                return result;
            }

            // check all neighbors
            AStarCheckNeighborNode(open, closed, current, start, target, subject, current.up);
            AStarCheckNeighborNode(open, closed, current, start, target, subject, current.down);
            AStarCheckNeighborNode(open, closed, current, start, target, subject, current.left);
            AStarCheckNeighborNode(open, closed, current, start, target, subject, current.right);
        }

        Debug.LogError("CombatGrid, GetAStar, current node never reached the target, returning empty path.");
        return result;
    }

    private void AStarCheckNeighborNode(List<GridSpace> open, List<GridSpace> closed, GridSpace current, GridSpace start, GridSpace target, Character subject, GridSpace neighbor)
    {
        // check if the neighbor is null for safety (we don't want to check a space that is off of the grid)
        if (neighbor != null)
        {
            if (neighbor != start)
            {
                // skip the node if it is closed or it is not traversable
                if (!subject.terrainTypes.Contains(neighbor.GetTerrainType()) || closed.Contains(neighbor) /*|| (neighbor.character != null && subject.affiliation != neighbor.character.affiliation)*/)
                {
                    return;
                }
            }

            float currentCostGWithWeight = current.costG + current.GetWeight();

            // check if the path to the neighbor is shorter or that the neighbor is not in the open list
            if (currentCostGWithWeight < neighbor.costG || !open.Contains(neighbor))
            {
                // update neighbor costs
                neighbor.costG = currentCostGWithWeight;
                neighbor.costH = Vector3.Distance(neighbor.obj.transform.position, target.obj.transform.position);
                neighbor.costF = neighbor.costG + neighbor.costH;

                // set connection for finding the completed path later
                neighbor.pathingConnection = current;

                // if the neighbor isn't already in the open list, add it
                if (!open.Contains(neighbor))
                {
                    open.Add(neighbor);
                }
            }
        }
    }

    private void AStarInitializeCosts(GridSpace[,] grid, GridSpace start, GridSpace target)
    {
        for (int i = 0; i < grid.GetLength(0); ++i)
        {
            for (int j = 0; j < grid.GetLength(1); ++j)
            {
                // calculate each grid space's costs given the start and target nodes
                grid[i, j].costG = Vector3.Distance(start.obj.transform.position, grid[i, j].obj.transform.position);//Mathf.Abs(start.coordinate.x - grid[i, j].coordinate.x) + Mathf.Abs(start.coordinate.y - grid[i, j].coordinate.y);
                grid[i, j].costH = Vector3.Distance(target.obj.transform.position, grid[i, j].obj.transform.position);//Mathf.Abs(target.coordinate.x - grid[i, j].coordinate.x) + Mathf.Abs(target.coordinate.y - grid[i, j].coordinate.y);
                grid[i, j].costF = grid[i, j].costG + grid[i, j].costH;
            }
        }

        return;
    }

    private GridSpace AStarGetLowestFCost(List<GridSpace> list)
    {
        // find the grid space with the lowest f cost in the given list and return it
        if (list.Count > 0)
        {
            GridSpace lowest = list[0];

            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i].costF < lowest.costF)
                {
                    lowest = list[i];
                }
            }

            return lowest;
        }

        Debug.LogError("CombatGrid, AStarGetLowestFCost, given list had Count of 0, returning null.");
        return null;
    }

    private bool AStarCheckGridSpaceForCharacters(GridSpace neighbor, GridSpace start, GridSpace target, Character subject)
    {
        if (neighbor != start && neighbor != target)
        {
            // check if the neighbor space contains a character, but only skip if the character isn't the subject character
            if (neighbor.character != null && neighbor.character.affiliation != subject.affiliation)
            {
                return true;
            }
        }

        return false;
    }

    public int GetDistance(GridSpace a, GridSpace b)
    {
        int distance = Mathf.Abs(a.coordinate.x - b.coordinate.x) + Mathf.Abs(a.coordinate.y - b.coordinate.y);
        return distance;
    }

    public void NextTurn()
    {
        for (int i = 0; i < grid.GetLength(0); ++i)
        {
            for (int j = 0; j < grid.GetLength(1); ++j)
            {
                grid[i, j].NextTurn();
            }
        }
    }

    private GridSpace_TerrainType GetTerrainTypeFromTag(string tag)
    {
        switch (tag)
        {
            case "TerrainType_Wall":
            {
                return GridSpace_TerrainType.wall;
            }
            case "TerrainType_Water":
            {
                return GridSpace_TerrainType.water;
            }
            default:
            {
                return GridSpace_TerrainType.standard;
            }
        }
    }

    public void DestroyGrid()
    {
        // destroy grid gameobjects
        for (int i = 0; i < grid.GetLength(0); ++i)
        {
            for (int j = 0; j < grid.GetLength(1); ++j)
            {
                Destroy(grid[i, j].obj);
            }
        }

        // clear the list
        grid = new GridSpace[0, 0];
    }
}

public class GridSpace
{
    // the actual GameObject of this grid space in the scene
    public GameObject obj;

    // the character: player, enemy, or otherwise currently on this GridSpace
    public Character character;

    // coordinate identifier, set on grid creation
    public Vector2Int coordinate;

    // queue of effects that should be applied to currChar, if it is not null
    public Queue<Effect> effects = new Queue<Effect>();

    // object to enable when a shadow wall is enabled on this grid space
    public GameObject shadowWall;

    // grid space terrain type, used for movement and processing abilities
    private GridSpace_TerrainType currentTerrainType;
    private GridSpace_TerrainType originalTerrainType;

    // constructor with in-world associated GameObject and original terrain type
    public GridSpace(GameObject gameobject, GridSpace_TerrainType terrainType, Vector2Int coord)
    {
        obj = gameobject;

        // set up terrain types
        originalTerrainType = terrainType;
        currentTerrainType = terrainType;

        // save coordinate position as a unique identifier for this grid space
        coordinate = coord;
    }

    // connections for ease of access
    public GridSpace up;
    public GridSpace down;
    public GridSpace left;
    public GridSpace right;

    // weight between grid spaces
    private const int weight = 1;

    // A* costs
    public float costG; // distance between this node and the starting node
    public float costH; // distance between this node and the ending node
    public float costF; // g cost + h cost

    // A* connection
    public GridSpace pathingConnection;

    public int Apply()
    {
        // first check if there any effects that should be applied to the grid space and not the character
        Effect[] effectArray = effects.ToArray();
        for (int i = 0; i < effectArray.Length; ++i)
        {
            // check for shadow wall effect
            if (effectArray[i].id == Effect_ID.shadowWall && character == null)
            {
                effectArray[i].source.SendEvent(PassiveEventID.storeGridSpace, this);
            }
        }

        while (effects.Count > 0)
        {
            // only apply effects if the grid space is currently storing a character
            if (character != null)
            {
                character.StartApplyEffect(effects.Dequeue(), true);
            }
            else
            {
                // when the grid space is not storing a character, do nothing and clear the effects
                effects.Clear();
            }
        }

        return effects.Count;
    }

    public void RemoveAllEffects()
    {
        effects.Clear();
    }

    public void AddEffect(Effect effect)
    {
        effects.Enqueue(effect);
    }

    public void NextTurn()
    {
        
    }

    public void SetTerrainType(GridSpace_TerrainType terrainType)
    {
        currentTerrainType = terrainType;
    }

    public GridSpace_TerrainType GetTerrainType()
    {
        return currentTerrainType;
    }

    public GridSpace_TerrainType GetTerrainTypeOriginal()
    {
        return originalTerrainType;
    }

    public int GetWeight()
    {
        return weight;
    }
}

[System.Serializable]
public class Effect
{
    // the enum identified for the type of effect that should be applied
    public Effect_ID id;

    // the value for the effect if applicable (for example, how much damage to deal)
    public int value;

    // the probability that this ability will succeed from 0 to 1 with 1 being guaranteed
    [Range(0.0f, 1.0f)]
    public float probability = 1.0f;

    [HideInInspector]
    public Character source;
    [HideInInspector]
    public bool trueDamage;
    [HideInInspector]
    public bool pierceDefense;

    // constructor for other classes to construct their own attack effects
    public Effect(Effect_ID effect_id, int val, float prob)
    {
        id = effect_id;
        value = val;
        probability = prob;

        Mathf.Clamp(probability, 0.0f, 1.0f);
    }

    // alternate constructor with probability set to 1 by default
    public Effect(Effect_ID effect_id, int val)
    {
        id = effect_id;
        value = val;
        probability = 1.0f;
    }

    // alternate constructor for counterattacks where the source is changed
    public Effect(Effect toCopyFrom, Character newSource)
    {
        id = toCopyFrom.id;
        value = toCopyFrom.value;
        probability = 1.0f;

        source = newSource;
    }
}

public enum GridSpace_TerrainType { standard, wall, wall_artificial, water };

public enum Effect_ID { damage, healing, aggro, frosty, aggroDispel, shadowWall, attackUp, noDamage, miss, custom };

public enum Character_Affiliation { player, enemy, ally, none };

static public class TerrainTypePresets
{
    public static List<GridSpace_TerrainType> onlyStandard = new List<GridSpace_TerrainType>() { GridSpace_TerrainType.standard };
    public static List<GridSpace_TerrainType> standardAndWater = new List<GridSpace_TerrainType>() { GridSpace_TerrainType.standard, GridSpace_TerrainType.water };
    public static List<GridSpace_TerrainType> none = new List<GridSpace_TerrainType>() { };
    public static List<GridSpace_TerrainType> all = new List<GridSpace_TerrainType>() { GridSpace_TerrainType.standard, GridSpace_TerrainType.water,
        GridSpace_TerrainType.wall, GridSpace_TerrainType.wall_artificial };
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatGrid : MonoBehaviour
{
    public uint gridWidth;
    public uint gridHeight;

    // the grid used in combat
    [HideInInspector]
    public GridSpace[,] grid;

    // list of grid spaces marked as "dirty"
    [HideInInspector]
    public Stack<GridSpace> dirty = new Stack<GridSpace>();

    public GameObject gridSpacePrefab;

    public void SpawnGrid()
    {
        grid = new GridSpace[gridWidth, gridHeight];

        // loop through to create all grid spaces
        for (int x = 0; x < gridWidth; ++x)
        {
            for (int y = 0; y < gridHeight; ++y)
            {
                grid[x, y] = new GridSpace(Instantiate(gridSpacePrefab, new Vector3(x, y, 0.0f), Quaternion.identity, transform));
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

    public void CleanGrid()
    {
        Debug.Log("Cleaning grid... " + dirty.Count + " grid spaces were set as dirty.");

        // loop through all dirty grid spaces and apply their effects
        while (dirty.Count > 0)
        {
            dirty.Peek().obj.GetComponent<Renderer>().material.color = Color.white;
            dirty.Pop().Apply();
        }
    }

    public void CleanGridWithoutApplying()
    {
        Debug.Log("Cleaning grid without applying... " + dirty.Count + " grid spaces were set as dirty.");

        // loop through all dirty grid spaces and apply their effects
        while (dirty.Count > 0)
        {
            dirty.Peek().obj.GetComponent<Renderer>().material.color = Color.white;
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

    public GridSpace TryMove(CombatDirection dir, GridSpace current)
    {
        System.Tuple<int, int> indices = GetIndicesOfGridSpace(current);

        switch (dir)
        {
            case CombatDirection.up:
            {
                if (CheckSpace(indices.Item1, indices.Item2 + 1))
                {
                    return grid[indices.Item1, indices.Item2 + 1];
                }
                break;
            }
            case CombatDirection.down:
            {
                if (CheckSpace(indices.Item1, indices.Item2 - 1))
                {
                    return grid[indices.Item1, indices.Item2 - 1];
                }
                break;
            }
            case CombatDirection.left:
            {
                if (CheckSpace(indices.Item1 - 1, indices.Item2))
                {
                    return grid[indices.Item1 - 1, indices.Item2];
                }
                break;
            }
            case CombatDirection.right:
            {
                if (CheckSpace(indices.Item1 + 1, indices.Item2))
                {
                    return grid[indices.Item1 + 1, indices.Item2];
                }
                break;
            }
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
}

public class GridSpace
{
    // the actual GameObject of this grid space in the scene
    public GameObject obj;

    // the character: player, enemy, or otherwise currently on this GridSpace
    public Character character;

    // queue of effects that should be applied to currChar, if it is not null
    public Stack<Effect> effects = new Stack<Effect>();

    public GridSpace(GameObject gameobject)
    {
        obj = gameobject;
    }

    // connections for ease of access
    public GridSpace up;
    public GridSpace down;
    public GridSpace left;
    public GridSpace right;

    public int Apply()
    {
        // only apply effects if the grid space is currently storing a character
        if (character != null)
        {
            while (effects.Count > 0)
            {
                character.ApplyEffect(effects.Pop());
            }
        }
        else
        {
            // when the grid space is not storing a character, do nothing and clear the effects
            effects.Clear();
        }

        return effects.Count;
    }

    public void RemoveAllEffects()
    {
        effects.Clear();
    }

    public void AddEffect(Effect effect)
    {
        effects.Push(effect);
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

    // constructor for other classes to construct their own attack effects
    Effect(Effect_ID effect_id, int val, float prob)
    {
        id = effect_id;
        value = val;
        probability = prob;

        Mathf.Clamp(probability, 0.0f, 1.0f);
    }
}

public enum Effect_ID { damage, healing, paralysis, poison, burn };
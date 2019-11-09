using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class ExamplePlayer : Character
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

    public bool TryMove(CombatMovDir dir)
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

    public void UseAbility(int num)
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

        // FOR NOW, LET'S ASSUME THE PLAYER IS FACING TO THE RIGHT EACH TIME

        if (abil != null)
        {
            string abilType = abil.GetType().Name;

            if (abilType == "PathAbility")
            {
                Debug.Log(abilType + " used.");
                ProcessPathAbility((PathAbility)abil);
            }
            else if (abilType == "CircleAbility")
            {
                Debug.Log(abilType + " used.");
            }
            else if (abilType == "ConeAbility")
            {
                Debug.Log(abilType + " used.");
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

    private void ProcessPathAbility(PathAbility abil)
    {
        GridSpace currentGridSpace = myGridSpace;

        // depending on the direction the character is facing, the meaning of "forward", etc changes, so do something different for each case
        // PLACEHOLDER SWITCH STATEMENT, REPLACE WITH DIRECTION THE CHARACTER IS FACING
        switch (0)
        {
            // facing upwards
            case 0:
            {
                MakePathDirty(abil, currentGridSpace, "up", "left", "right", "down");
                break;
            }
            // facing downwards
            case 1:
            {
                MakePathDirty(abil, currentGridSpace, "down", "right", "left", "up");
                break;
            }
            // facing left
            case 2:
            {
                MakePathDirty(abil, currentGridSpace, "left", "down", "up", "right");
                break;
            }
            // facing right
            case 3:
            {
                MakePathDirty(abil, currentGridSpace, "right", "up", "down", "left");
                break;
            }
        }
    }

    private void MakePathDirty(PathAbility abil, GridSpace currentGridSpace, string forward, string sideways, string sidewaysOpposite, string backwards)
    {
        // loop through all the segments of the path
        for (int i = 0; i < abil.path.Count; ++i)
        {
            // check the direction, and set that part of the segment dirty based on the given direction
            switch (abil.path[i].directions)
            {
                case AbilityDirection.forwards:
                {
                    MakePathSegmentDirty(abil, ref currentGridSpace, forward, i);

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
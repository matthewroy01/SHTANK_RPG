using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class PlayerBase : Character
{
    private bool idle = true;
    private Vector3 defaultPosition;

    public Moveset moveset;

    private enum PathDirections { up, down, left, right };

    private MovementDialogueProcessor refMovementDialogueProcessor;

    private bool selected = false;

    private void Start()
    {
        refMovementDialogueProcessor = GetComponent<MovementDialogueProcessor>();
    }

    private void Update()
    {
        if (selected)
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

    public bool TryMove(CombatDirection dir, CombatGrid grid)
    {
        bool result = false;

        GridSpace tmp = grid.TryMove(dir, myGridSpace);

        if (tmp != myGridSpace)
        {
            result = true;
        }

        myGridSpace = tmp;
        transform.position = myGridSpace.obj.transform.position;

        return result;
    }

    public void Selected()
    {
        selected = true;

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

    private void MoveToGridSpace(GridSpace toMoveTo)
    {
        if (toMoveTo != null)
        {
            transform.position = toMoveTo.obj.transform.position;
            myGridSpace = toMoveTo;
        }
    }

    public void EndTurn()
    {
        selected = false;
        idle = true;
    }

    public bool GetIdle()
    {
        return idle;
    }

    public void ResetToDefaultPosition(GridSpace toReturnTo)
    {
        transform.position = defaultPosition;
        myGridSpace = toReturnTo;
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;

public class PlayerBase : Character
{
    private bool idle = true;
    private Vector3 defaultPosition;

    private AbilityProcessor refAbilityProcessor;

    public Moveset moveset;

    private enum PathDirections { up, down, left, right };

    private void Start()
    {
        refAbilityProcessor = FindObjectOfType<AbilityProcessor>();
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

    private void MoveToGridSpace(GridSpace toMoveTo)
    {
        if (toMoveTo != null)
        {
            transform.position = toMoveTo.obj.transform.position;
            myGridSpace = toMoveTo;
        }
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
                refAbilityProcessor.ProcessPathAbility((PathAbility)abil, myGridSpace, facing, flipped);
            }
            else if (abilType == "CircleAbility")
            {
                Debug.Log(abilType + " used.");
                refAbilityProcessor.ProcessCircleAbility((CircleAbility)abil, myGridSpace);
            }
            else if (abilType == "ConeAbility")
            {
                Debug.Log(abilType + " used.");
                refAbilityProcessor.ProcessConeAbility((ConeAbility)abil, myGridSpace, facing);
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

    public void EndTurn()
    {
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
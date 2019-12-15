using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSelector : MonoBehaviour
{
    public PlayerBase currentPlayer = null;
    private GridSpace defaultGridSpace;
    private bool atDefPos = true;
    private bool inputtedAbility = false;

    private GridSpace abilityGridSpace;

    private CombatGrid refCombatGrid;
    private AbilityProcessor refAbilityProcessor;

    private int savedAbilityNum;
    private CombatDirection facing = CombatDirection.up;
    private bool flipped = false;

    public LayerMask gridSpaceLayerMask;

    void Start()
    {
        refCombatGrid = FindObjectOfType<CombatGrid>();
        refAbilityProcessor = FindObjectOfType<AbilityProcessor>();
    }

    void Update()
    {
        // selecting players on the grid
        if (currentPlayer == null)
        {
            Select();
        }

        // actions with the selected player
        if (currentPlayer != null)
        {
            Ability get = refAbilityProcessor.GetAbility();

            // if an ability is ranged
            if (get != null && get.ranged)
            {
                RangedSelect();
            }
            else
            {
                abilityGridSpace = null;
            }

            // TEMPORARY CODE FOR SETTING THE COLOR OF THE GRID SPACES, THIS SHOULD PROBABLY BE HANDLED BY A SHADER
            // blue for movement
            for (int i = 0; i < currentPlayer.movementSpaces.Count; ++i)
            {
                currentPlayer.movementSpaces[i].obj.GetComponent<Renderer>().material.color = Color.Lerp(Color.Lerp(Color.blue, Color.white, 0.25f), Color.blue, Mathf.Sin(Time.time * 10.0f + (0.5f * i)));
            }

            Movement();
            Flip();
            CancelOrSave();
            DoMoves();
        }
    }

    private void Select()
    {
        // only try to select a new current player if we're not already selecting something
        if (currentPlayer == null)
        {
            // clicking on objects in scene using raycasts from: https://www.youtube.com/watch?v=EANtTI6BCxk
            // because I'm dumb and I couldn't remember how to do it myself
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out hit))
            {
                if (hit.transform)
                {
                    // try to set currentPlayer if the hit object has a player component
                    currentPlayer = hit.transform.GetComponent<PlayerBase>();

                    if (currentPlayer != null)
                    {
                        // let the player know they've been selected for things like VFX
                        currentPlayer.Selected();
                    }

                    if (currentPlayer.GetIdle() == true)
                    {
                        currentPlayer = null;
                    }
                    else
                    {
                        defaultGridSpace = currentPlayer.myGridSpace;
                    }
                }
            }
        }
    }

    private void Movement()
    {
        bool moved = false;

        if (Input.GetKeyDown(KeyCode.W))
        {
            ApplyNewDirection(CombatDirection.up, ref moved);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            ApplyNewDirection(CombatDirection.down, ref moved);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            ApplyNewDirection(CombatDirection.left, ref moved);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            ApplyNewDirection(CombatDirection.right, ref moved);
        }

        if (moved == true)
        {
            atDefPos = false;
        }
    }

    private void ApplyNewDirection(CombatDirection direction, ref bool moved)
    {
        // change the direction we're facing
        facing = direction;

        if (inputtedAbility)
        {
            // try to process an ability based on the new direction
            TryProcessAbility();
        }
        else
        {
            // try to move
            moved = currentPlayer.TryMove(direction, refCombatGrid);
        }
    }

    private void Flip()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            // toggle the flip
            flipped = !flipped;

            // try to process an ability based on the new orienatation
            TryProcessAbility();
        }
    }

    private void DoMoves()
    {
        KeyCode[] keys = { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };

        // if we have inputted an ability previously and a button is pressed again, commit to the ability
        for (int i = 0; i < keys.Length; ++i)
        {
            if (Input.GetKeyDown(keys[i]) && savedAbilityNum == i + 1)
            {
                // apply the currently saved ability
                if (refAbilityProcessor.ApplyAbility())
                {
                    // end the selected player's turn
                    EndTurn();

                    // reset our previously inputted ability
                    inputtedAbility = false;
                    savedAbilityNum = 0;
                }

                // don't bother checking for additional input this frame
                return;
            }
        }

        // if an ability hasn't already been input, or a new one is being input, save that ability's input
        for (int i = 0; i < keys.Length; ++i)
        {
            if (Input.GetKeyDown(keys[i]))
            {
                // reset ability grid space when selecting new abilities
                abilityGridSpace = null;

                SaveAbilityInput(i + 1);
            }
        }
    }

    private void SaveAbilityInput(int num)
    {
        // have the selected player prepare its ability given the input, the direction we are "facing", and whether or not we are "flipped"
        savedAbilityNum = num;

        TryProcessAbility();

        inputtedAbility = true;
    }

    private void TryProcessAbility()
    {
        if (currentPlayer != null)
        {
            refAbilityProcessor.ProcessAbility(currentPlayer, abilityGridSpace, savedAbilityNum, facing, flipped);
        }
    }

    private void CancelOrSave()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            // TEMPORARY CODE FOR RESETTING THE COLOR OF THE GRID SPACES, THIS SHOULD PROBABLY BE HANDLED BY A SHADER
            for (int i = 0; i < currentPlayer.movementSpaces.Count; ++i)
            {
                currentPlayer.movementSpaces[i].obj.GetComponent<Renderer>().material.color = Color.white;
            }

            EndTurn();
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            // TEMPORARY CODE FOR RESETTING THE COLOR OF THE GRID SPACES, THIS SHOULD PROBABLY BE HANDLED BY A SHADER
            for (int i = 0; i < currentPlayer.movementSpaces.Count; ++i)
            {
                currentPlayer.movementSpaces[i].obj.GetComponent<Renderer>().material.color = Color.white;
            }

            Cancel();
        }
    }

    private void EndTurn()
    {
        currentPlayer.EndTurn();
        currentPlayer = null;
        atDefPos = true;
    }

    private void Cancel()
    {
        // if an ability has been input, cancel it
        if (inputtedAbility)
        {
            inputtedAbility = false;
            savedAbilityNum = 0;
        }
        // otherwise, reset movement
        else
        {
            // if the character is already at their default position, deselect them
            if (atDefPos)
            {
                // let the player know they have been deselected for things like VFX
                currentPlayer.Deselected();

                currentPlayer = null;
            }
            // otherwise move the character back to their default position
            else
            {
                currentPlayer.ResetToDefaultPosition(defaultGridSpace);
                atDefPos = true;
            }
        }
    }

    private void RangedSelect()
    {
        // clicking on objects in scene using raycasts from: https://www.youtube.com/watch?v=EANtTI6BCxk
        // because I'm dumb and I couldn't remember how to do it myself
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, float.MaxValue, gridSpaceLayerMask))
        {
            if (hit.transform)
            {
                // try to set the ability grid space for this special case
                abilityGridSpace = refCombatGrid.GetGridSpace(hit.transform.gameObject);

                TryProcessAbility();
            }
        }
    }
}

public enum CombatDirection { up = 0, down, left, right };
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

public class CharacterSelector : MonoBehaviour
{
    [Header("Selected Characters (for display only)")]
    public Character currentPlayer;
    private GridSpace defaultGridSpace;
    private GridSpace tmpGridSpace;
    public Character overlayCharacter;

    [Header("Layer Masks for raycast collision from mouse")]
    public LayerMask layerMaskGrid;

    [Header("Cursor")]
    public SpriteRenderer cursor;
    private Collider cursorSpace = null;
    private Tweener cursorTween;

    [Header("Sounds")]
    public ManagedAudio audioSelect;
    public ManagedAudio audioDeselect;

    // information to send to the Ability Processor
    private bool dirty = false;
    private int selectedAbilityNum = 0;
    private bool flipped = false;
    private CombatDirection facing = CombatDirection.up;
    private GridSpace abilityGridSpace;

    private StateMachine stateMachine;

    private CombatGrid refCombatGrid;
    private AbilityProcessor refAbilityProcessor;
    private CharacterUI refCharacterUI;
    private ContextSensitiveUI refContextSensitiveUI;
    private PhaseManager refPhaseManager;
    private UtilityAudioManager refAudioManager;

    public RadialMenu refRadialMenu;

    System.Tuple<KeyCode, CombatDirection>[] keysAndDirections = {
            new System.Tuple<KeyCode, CombatDirection>(KeyCode.W, CombatDirection.up),
            new System.Tuple<KeyCode, CombatDirection>(KeyCode.S, CombatDirection.down),
            new System.Tuple<KeyCode, CombatDirection>(KeyCode.D, CombatDirection.left),
            new System.Tuple<KeyCode, CombatDirection>(KeyCode.A, CombatDirection.right),
        };

    private void Awake()
    {
        InitializeStateMachine();
    }

    private void InitializeStateMachine()
    {
        // set up state machine connections here
        stateMachine = new StateMachine((int)SelectorState.doingNothing,

            // from doing nothing
            new StateMachineConnection((int)SelectorState.doingNothing, (int)SelectorState.playerSelected),
            new StateMachineConnection((int)SelectorState.doingNothing, (int)SelectorState.enemySelected),

            // base level selecting a player or enemy
            new StateMachineConnection((int)SelectorState.playerSelected, (int)SelectorState.playerSelected),
            new StateMachineConnection((int)SelectorState.enemySelected, (int)SelectorState.enemySelected),
            new StateMachineConnection((int)SelectorState.playerSelected, (int)SelectorState.enemySelected),
            new StateMachineConnection((int)SelectorState.enemySelected, (int)SelectorState.playerSelected),
            new StateMachineConnection((int)SelectorState.enemySelected, (int)SelectorState.doingNothing),
            new StateMachineConnection((int)SelectorState.playerSelected, (int)SelectorState.doingNothing),

            // higher levels of selecting players
            new StateMachineConnection((int)SelectorState.playerSelected, (int)SelectorState.playerSelectedWithMovement),
            new StateMachineConnection((int)SelectorState.playerSelected, (int)SelectorState.playerSelectedWithAbility),
            new StateMachineConnection((int)SelectorState.playerSelectedWithMovement, (int)SelectorState.playerSelected),
            new StateMachineConnection((int)SelectorState.playerSelectedWithMovement, (int)SelectorState.playerSelectedWithAbility),
            new StateMachineConnection((int)SelectorState.playerSelectedWithAbility, (int)SelectorState.playerSelectedWithMovement),
            new StateMachineConnection((int)SelectorState.playerSelectedWithAbility, (int)SelectorState.playerSelected),
            new StateMachineConnection((int)SelectorState.playerSelectedWithAbility, (int)SelectorState.playerSelectedWithAbility),

            // ending a player's turn
            new StateMachineConnection((int)SelectorState.playerSelected, (int)SelectorState.doingNothing),
            new StateMachineConnection((int)SelectorState.playerSelectedWithAbility, (int)SelectorState.doingNothing),
            new StateMachineConnection((int)SelectorState.playerSelectedWithMovement, (int)SelectorState.doingNothing)
        );
    }

    private void Start()
    {
        refCombatGrid = FindObjectOfType<CombatGrid>();
        refAbilityProcessor = FindObjectOfType<CombatManager>().GetAbilityProcessor();
        refCharacterUI = FindObjectOfType<CharacterUI>();
        refContextSensitiveUI = FindObjectOfType<ContextSensitiveUI>();
        refPhaseManager = FindObjectOfType<PhaseManager>();
        refAudioManager = FindObjectOfType<UtilityAudioManager>();
        refRadialMenu = FindObjectOfType<RadialMenu>();

        refCharacterUI.ToggleUI(false);
    }

    private void Update()
    {
        ProcessState();

        if (refPhaseManager.currentPhase == CombatPhase.Player)
        {
            ShowEnemyInfo();
        }
        else
        {
            overlayCharacter = null;
            refCharacterUI.ToggleUI(false);
        }

        if (refAbilityProcessor.GetAbility() == null)
        {
            refContextSensitiveUI.UpdateContextSensitiveUI(stateMachine.currentState, false);
        }
        else
        {
            refContextSensitiveUI.UpdateContextSensitiveUI(stateMachine.currentState, refAbilityProcessor.GetAbility().ranged);
        }

        ShowCursor();
    }

    private void ProcessState()
    {
        switch(stateMachine.currentState)
        {
            case (int)SelectorState.doingNothing:
            {
                // select player characters
                Select();

                break;
            }
            case (int)SelectorState.playerSelected:
            {
                // input abilities
                // THIS IS HANDLED BY BUTTONS IN THE UI

                // input movement
                MovementMouse();

                // select a different character
                Select();

                // confirm/end movement
                EndTurn();

                // deselect the current character
                Deselect();

                break;
            }
            case (int)SelectorState.enemySelected:
            {
                break;
            }
            case (int)SelectorState.playerSelectedWithMovement:
            {
                // input abilities
                // THIS IS HANDLED BY BUTTONS IN THE UI

                // input movement
                MovementMouse();

                // select a different character

                // confirm/end movement
                EndTurn();

                // cancel movement
                MovementCancel();

                break;
            }
            case (int)SelectorState.playerSelectedWithAbility:
            {
                // change direction/general display
                AbilityProcess();

                // ranged aim
                AbilityRangedSelect();

                // flip/rotate ability
                AbilityFlip();
                AbilityRotate();

                // use an ability
                AbilityUse();

                // deselect the current ability
                AbilityDeselect();

                // try to display the movement ability forecast
                refAbilityProcessor.UpdateMovementAbilityForecast();

                break;
            }
        }
    }

    private void Select()
    {
        // clicking on objects in scene using raycasts from: https://www.youtube.com/watch?v=EANtTI6BCxk
        // because I'm dumb and I couldn't remember how to do it myself
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButtonDown(0) && Physics.Raycast(ray, out hit) && refCharacterUI.GetMouseIsntOverButton())
        {
            if (hit.transform)
            {
                Character tmp = null;

                if (hit.transform.TryGetComponent(out tmp) && tmp != null && tmp != currentPlayer && !tmp.GetIdle())
                {
                    // deselect the previously selected player
                    if (currentPlayer != null)
                    {
                        currentPlayer.Deselected(refCombatGrid);
                    }

                    // assign the new current player
                    currentPlayer = tmp;

                    // keep track of their starting grid space
                    defaultGridSpace = currentPlayer.myGridSpace;

                    // let the newly selected player know they have been selected
                    currentPlayer.Selected(refCombatGrid);

                    // update UI
                    refRadialMenu.Enable(currentPlayer);

                    refCharacterUI.ToggleUI(true);
                    refCharacterUI.UpdateCharacterUI(currentPlayer);

                    // update the state machine
                    stateMachine.TryUpdateConnection((int)SelectorState.playerSelected);

                    // play sound
                    refAudioManager.QueueSound(audioSelect);
                }
            }
        }
    }

    private void Deselect()
    {
        if (Input.GetMouseButtonDown(1))
        {
            currentPlayer.Deselected(refCombatGrid);
            currentPlayer = null;

            refRadialMenu.Disable();

            refCharacterUI.ToggleUI(false);

            stateMachine.TryUpdateConnection((int)SelectorState.doingNothing);

            refAudioManager.QueueSound(audioDeselect);
        }
    }

    private void MovementMouse()
    {
        // clicking on objects in scene using raycasts from: https://www.youtube.com/watch?v=EANtTI6BCxk
        // because I'm dumb and I couldn't remember how to do it myself
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Input.GetMouseButton(0) && Physics.Raycast(ray, out hit, layerMaskGrid) && refCharacterUI.GetMouseIsntOverButton())
        {
            if (hit.transform != null)
            {
                // if the A* movement was succesful (the character actually moved)
                GridSpace newGridSpace = refCombatGrid.GetGridSpace(hit.transform.gameObject);
                if (currentPlayer.TryMoveAStar(refCombatGrid, newGridSpace))
                {
                    if (tmpGridSpace != null)
                    {
                        tmpGridSpace.character = null;
                    }

                    // and the player didn't just move back to its original starting space
                    if (currentPlayer.myGridSpace != defaultGridSpace)
                    {
                        defaultGridSpace.character = null;
                        newGridSpace.character = currentPlayer;
                        tmpGridSpace = newGridSpace;

                        // update the state machine
                        stateMachine.TryUpdateConnection((int)SelectorState.playerSelectedWithMovement);
                    }
                    // but the player moved back to its original starting space
                    else
                    {
                        defaultGridSpace.character = currentPlayer;

                        // update the state machine
                        stateMachine.TryUpdateConnection((int)SelectorState.playerSelected);
                    }
                }
            }
        }
    }

    private void Movement()
    {
        for (int i = 0; i < keysAndDirections.Length; ++i)
        {
            // if input is provided and the movement occurs successfully, update the state machine since we have moved
            if (Input.GetKeyDown(keysAndDirections[i].Item1) && currentPlayer.TryMove(keysAndDirections[i].Item2, refCombatGrid))
            {
                stateMachine.TryUpdateConnection((int)SelectorState.playerSelectedWithMovement);
            }
        }
    }

    private void MovementCancel()
    {
        // cancle movement and return to a state where the selected player has yet to move
        if (Input.GetMouseButtonDown(1))
        {
            currentPlayer.ResetToDefaultPosition(defaultGridSpace);
            defaultGridSpace.character = currentPlayer;
            tmpGridSpace.character = null;

            stateMachine.TryUpdateConnection((int)SelectorState.playerSelected);

            refAudioManager.QueueSound(audioDeselect);
        }
    }

    private void EndTurn()
    {
        // end turn by pressing spacebar
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EndTurnFunctionality();

            return;
        }
    }

    private void EndTurnFunctionality()
    {
        tmpGridSpace = null;

        currentPlayer.EndTurn();
        currentPlayer = null;

        refCharacterUI.ToggleUI(false);

        refAbilityProcessor.UpdateAbilityForecast();

        stateMachine.TryUpdateConnection((int)SelectorState.doingNothing);
    }

    private void ContinueTurnFunctionality()
    {
        refAbilityProcessor.UpdateAbilityForecast();

        // return to the previous state (either state where the player is selected but an ability has not been)
        if (stateMachine.previousState == (int)SelectorState.playerSelected)
        {
            stateMachine.TryUpdateConnection((int)SelectorState.playerSelected);
        }
        else if (stateMachine.previousState == (int)SelectorState.playerSelectedWithMovement)
        {
            stateMachine.TryUpdateConnection((int)SelectorState.playerSelectedWithMovement);
        }

        refAbilityProcessor.CancelAbility();
    }

    public void AbilitySelect(int abilNum)
    {
        if (!currentPlayer.statusStunned && currentPlayer.movesetData.GetAvailability(abilNum) && stateMachine.TryUpdateConnection((int)SelectorState.playerSelectedWithAbility))
        {
            // this public function is called using delegates from UI buttons in the scene
            selectedAbilityNum = abilNum;

            // set the ability grid space to a default value
            abilityGridSpace = currentPlayer.myGridSpace;

            // update the Ability UI's colors
            refCharacterUI.SetSelectedAbilityColor(selectedAbilityNum);

            dirty = true;

            refAudioManager.QueueSound(audioSelect);
        }
    }

    private void AbilityProcess()
    {
        // tell the ability processor to process the ability
        if (dirty)
        {
            TryProcessAbility();

            refAbilityProcessor.UpdateAbilityForecast();

            dirty = false;
        }
    }

    private void AbilityDeselect()
    {
        // cancel the selected ability
        if (Input.GetMouseButtonDown(1))
        {
            selectedAbilityNum = 0;

            // return to the previous state (either state where the player is selected but an ability has not been)
            if (stateMachine.previousState == (int)SelectorState.playerSelected)
            {
                stateMachine.TryUpdateConnection((int)SelectorState.playerSelected);
            }
            else if (stateMachine.previousState == (int)SelectorState.playerSelectedWithMovement)
            {
                stateMachine.TryUpdateConnection((int)SelectorState.playerSelectedWithMovement);
            }

            // tell the ability processor to cancel whatever it has ready to process
            refAbilityProcessor.CancelAbility();

            // update the ability UI's colors
            refCharacterUI.SetSelectedAbilityColor(selectedAbilityNum);

            refAbilityProcessor.UpdateAbilityForecast();

            refAudioManager.QueueSound(audioDeselect);
        }
    }

    private void AbilityRangedSelect()
    {
        // change the ability's starting grid space if the selected ability is ranged
        if (refAbilityProcessor.GetAbility() != null && refAbilityProcessor.GetAbility().ranged)
        {
            // clicking on objects in scene using raycasts from: https://www.youtube.com/watch?v=EANtTI6BCxk
            // because I'm dumb and I couldn't remember how to do it myself
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Input.GetMouseButton(0) && Physics.Raycast(ray, out hit, float.MaxValue, layerMaskGrid) && refCharacterUI.GetMouseIsntOverButton())
            {
                Character tmp = null;

                if (hit.transform != null)
                {
                    // if the object's layer is within the grid layermask (here we are looking for grid space objects)
                    if (layerMaskGrid == (layerMaskGrid | (1 << hit.transform.gameObject.layer)))
                    {
                        // check if the grid space that was clicked is in the list of starting spaces
                        if (refAbilityProcessor.GetStartingSpaces().Contains(refCombatGrid.GetGridSpace(hit.transform.gameObject)))
                        {
                            // assign the ability's starting Grid Space
                            if ((abilityGridSpace = refCombatGrid.GetGridSpace(hit.transform.gameObject)) != null)
                            {
                                TryProcessAbility();
                            }
                        }
                    }
                    // if the object is a character, get its grid space instead
                    else if (hit.transform.TryGetComponent(out tmp) && tmp != null)
                    {
                        // check if the grid space that was clicked is in the list of starting spaces
                        if (refAbilityProcessor.GetStartingSpaces().Contains(tmp.myGridSpace))
                        {
                            // assign the ability's starting Grid Space
                            if ((abilityGridSpace = tmp.myGridSpace) != null)
                            {
                                TryProcessAbility();
                            }
                        }
                    }
                }
            }
        }
        // if the ability is not ranged, we can set the ability grid space to null, and the ability processor will take the selected character's current grid space instead
        else
        {
            abilityGridSpace = currentPlayer.myGridSpace;
        }
    }

    private void AbilityFlip()
    {
        // flip the orientation of the selected ability
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            flipped = !flipped;

            // update the ability forecast in case the flip changed what characters are within the selection
            refAbilityProcessor.UpdateAbilityForecast();

            dirty = true;
        }
    }

    private void AbilityRotate()
    {
        // calculate screen pos of the current selected player
        Vector2 playerScreenPos = Camera.main.WorldToScreenPoint(abilityGridSpace.obj.transform.position);
        playerScreenPos /= new Vector2(Screen.width, Screen.height);

        // calculate screen pos of the mouse
        Vector2 mousePos = Input.mousePosition;
        mousePos /= new Vector2(Screen.width, Screen.height);

        // get a direction based on the mouse and player position
        Vector2 direction = mousePos - playerScreenPos;

        // find the direction the ability should face
        CombatDirection tmp;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0.0f)
            {
                tmp = CombatDirection.right;
            }
            else
            {
                tmp = CombatDirection.left;
            }
        }
        else
        {
            if (direction.y > 0.0f)
            {
                tmp = CombatDirection.up;
            }
            else
            {
                tmp = CombatDirection.down;
            }
        }

        // save the previous direction
        CombatDirection previous = facing;
    
        // assign the new direction
        facing = tmp;

        if (previous != facing)
        {
            dirty = true;
        }
    }

    private void AbilityUse()
    {
        // actually use the ability and apply its effects to the grid
        if (Input.GetKeyDown(KeyCode.Space) && selectedAbilityNum != 0)
        {
            // if there is a valid ability to apply
            if (refAbilityProcessor.ApplyAbilityCheck())
            {
                if (refAbilityProcessor.turnEnding)
                {
                    currentPlayer.SaveMyGridSpace();
                }

                // apply the currently saved ability
                refAbilityProcessor.ApplyAbility();

                currentPlayer.movesetData.Use(selectedAbilityNum);
                switch (selectedAbilityNum)
                {
                    case 1:
                    {
                        currentPlayer.SendEvent(PassiveEventID.abilityUse1);
                        break;
                    }
                    case 2:
                    {
                        currentPlayer.SendEvent(PassiveEventID.abilityUse2);
                        break;
                    }
                    case 3:
                    {
                        currentPlayer.SendEvent(PassiveEventID.abilityUse3);
                        break;
                    }
                    case 4:
                    {
                        currentPlayer.SendEvent(PassiveEventID.abilityUse4);
                        break;
                    }
                }

                if (refAbilityProcessor.turnEnding == true)
                {
                    // end the selected player's turn
                    EndTurnFunctionality();

                    dirty = true;
                }
                else
                {
                    ContinueTurnFunctionality();

                    dirty = true;
                }

                // reset our previously inputted ability
                selectedAbilityNum = 0;

                // update the ability UI's colors
                refCharacterUI.SetSelectedAbilityColor(selectedAbilityNum);
            }
        }
    }

    private void TryProcessAbility()
    {
        if (currentPlayer != null)
        {
            refAbilityProcessor.ProcessAbility(currentPlayer, abilityGridSpace, selectedAbilityNum, facing, flipped);
        }
    }

    private void ShowEnemyInfo()
    {
        // clicking on objects in scene using raycasts from: https://www.youtube.com/watch?v=EANtTI6BCxk
        // because I'm dumb and I couldn't remember how to do it myself
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, layerMaskGrid) && !Input.GetMouseButton(0) && refCharacterUI.GetMouseIsntOverButton())
        {
            Character tmpEnemy;

            if (hit.transform != null && hit.transform.TryGetComponent(out tmpEnemy) && (tmpEnemy.affiliation == Character_Affiliation.enemy || tmpEnemy.affiliation == Character_Affiliation.ally))
            {
                tmpEnemy.Selected(refCombatGrid);

                // save the enemy as a special overlayed character
                overlayCharacter = tmpEnemy;

                refCharacterUI.ToggleUI(true);
                refCharacterUI.UpdateCharacterUI(tmpEnemy);
            }
            else
            {
                if (currentPlayer != null)
                {
                    refCharacterUI.UpdateCharacterUI(currentPlayer);
                }
                else
                {
                    refCharacterUI.ToggleUI(false);
                }

                overlayCharacter = null;
            }
        }
        else
        {
            if (currentPlayer != null)
            {
                refCharacterUI.UpdateCharacterUI(currentPlayer);
            }
            else
            {
                refCharacterUI.ToggleUI(false);
            }

            overlayCharacter = null;
        }
    }

    public void EndPhase()
    {
        if (currentPlayer != null)
        {
            // reset any movement if a player was selected
            currentPlayer.ResetToDefaultPosition(defaultGridSpace);
            defaultGridSpace.character = currentPlayer;
            if (tmpGridSpace != null)
            {
                tmpGridSpace.character = null;
            }

            // cancel selected abilities
            selectedAbilityNum = 0;
            refAbilityProcessor.CancelAbility();
            refCharacterUI.SetSelectedAbilityColor(selectedAbilityNum);
            refAbilityProcessor.UpdateAbilityForecast();
            refAbilityProcessor.UpdateMovementAbilityForecast();

            stateMachine.TryUpdateConnection((int)SelectorState.doingNothing);

            currentPlayer = null;
        }

        stateMachine.TryUpdateConnection((int)SelectorState.doingNothing);
    }

    private void ShowCursor()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, float.MaxValue, layerMaskGrid))
        {
            if (hit.transform != null)
            {
                if (hit.collider != cursorSpace)
                {
                    cursorSpace = hit.collider;

                    cursor.transform.position = hit.transform.position + Vector3.up * 0.01f;

                    if (cursorTween != null)
                    {
                        cursorTween.Kill();
                    }
                    cursor.transform.localScale = Vector3.one;
                    cursorTween = cursor.transform.DOPunchScale(Vector3.one * 0.25f, 0.1f, 0, 0.0f);
                }

                GridSpace gridSpace = refCombatGrid.GetGridSpace(hit.transform.gameObject);

                if (gridSpace != null && gridSpace.character != null)
                {
                    if (gridSpace.character.affiliation == Character_Affiliation.player)
                    {
                        cursor.color = Color.Lerp(Color.blue, Color.white, 0.25f);
                    }
                    else if (gridSpace.character.affiliation == Character_Affiliation.enemy)
                    {
                        cursor.color = Color.Lerp(Color.red, Color.white, 0.25f);
                    }
                    else
                    {
                        cursor.color = Color.white;
                    }
                }
                else
                {
                    cursor.color = Color.white;
                }
            }

            cursor.transform.localScale = Vector3.Lerp(cursor.transform.localScale, Vector3.one + (Vector3.one * (Mathf.Sin(Time.time * 3.0f) * 0.1f)), 0.1f);
        }
        else
        {
            cursor.transform.localScale = Vector3.Lerp(cursor.transform.localScale, Vector3.zero, 0.1f);
            cursorSpace = null;
        }
    }

    public enum SelectorState
    {
        doingNothing = 0,
        playerSelected = 1,
        enemySelected = 2,
        playerSelectedWithMovement = 3,
        playerSelectedWithAbility = 4
    }
}
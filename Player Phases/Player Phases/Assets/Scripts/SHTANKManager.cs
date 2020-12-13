using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHTANKManager : MonoBehaviour
{
    private StateMachine stateMachine;

    private OverworldManager refOverworldManager;
    private CombatManager refCombatManager;
    private DialogueProcessor refDialogueProcessor;
    private Party refParty;

    private SHTANKCamera refSHTANKCamera;

    public AudioSource musicOverworld;
    public AudioSource musicBattle;

    private Vector2 combatDirection;

    private Coroutine ignoreCollisionCoroutine;

    public DialogueDefinition debugDialogueStart;
    public DialogueDefinition debugDialogueEnd;

    private void Awake()
    {
        Screen.SetResolution(1920, 1080, true);

        InitializeStateMachine();
    }

    private void InitializeStateMachine()
    {
        // set up state machine connections here
        stateMachine = new StateMachine((int)GameState.overworld,

            // overworld and combat
            new StateMachineConnection((int)GameState.overworld, (int)GameState.combat),
            new StateMachineConnection((int)GameState.combat, (int)GameState.overworld),
            new StateMachineConnection((int)GameState.overworld, (int)GameState.dialogue),
            new StateMachineConnection((int)GameState.dialogue, (int)GameState.overworld)
        );
    }

    void Start()
    {
        refOverworldManager = FindObjectOfType<OverworldManager>();
        refCombatManager = FindObjectOfType<CombatManager>();
        
        refSHTANKCamera = FindObjectOfType<SHTANKCamera>();

        refDialogueProcessor = FindObjectOfType<DialogueProcessor>();

        refParty = FindObjectOfType<Party>();

        if (refDialogueProcessor != null)
        {
            stateMachine.TryUpdateConnection((int)GameState.dialogue);
            refDialogueProcessor.Display(debugDialogueStart);
        }
    }

    void Update()
    {
        ProcessState();
    }

    private void FixedUpdate()
    {
        ProcessStateFixed();
    }

    private void ProcessState()
    {
        switch (stateMachine.currentState)
        {
            case (int)GameState.overworld:
            {
                // run overworld behaviour
                refOverworldManager.MyUpdate();

                if (refDialogueProcessor != null && debugDialogueEnd != null)
                {
                    OverworldEnemyController[] tmp = FindObjectsOfType<OverworldEnemyController>();
                    if (tmp.Length == 0)
                    {
                        stateMachine.TryUpdateConnection((int)GameState.dialogue);
                        refDialogueProcessor.Display(debugDialogueEnd);
                        debugDialogueEnd = null;
                    }
                }

                break;
            }
            case (int)GameState.combat:
            {
                // run combat behaviour
                refCombatManager.MyUpdate();

                break;
            }
            case (int)GameState.dialogue:
            {
                // do nothing, lol
                break;
            }
        }
    }

    private void ProcessStateFixed()
    {
        switch (stateMachine.currentState)
        {
            case (int)GameState.overworld:
            {
                // use overworld camera functionality
                refSHTANKCamera.CameraFunctionalityOverworld();

                break;
            }
            case (int)GameState.combat:
            {
                // use combat camera functionality
                refSHTANKCamera.CameraFunctionalityCombat();

                break;
            }
            case (int)GameState.dialogue:
            {
                // use overworld camera functionality
                refSHTANKCamera.CameraFunctionalityOverworld();
                break;
            }
        }
    }

    public void TryBeginCombat(Vector3 position, OverworldEnemyController targetEnemy)
    {
        if (stateMachine.TryUpdateConnection((int)GameState.combat))
        {
            refCombatManager.InitiatePhase(CombatPhase.Player, position, targetEnemy);
            refOverworldManager.DisableOverworldObjects();

            musicOverworld.Pause();
            musicBattle.Play();

            combatDirection = (targetEnemy.transform.position - position).normalized;
        }
    }

    public void TryEndCombat(bool won)
    {
        if (stateMachine.TryUpdateConnection((int)GameState.overworld))
        {
            refParty.SaveHealth(FindObjectOfType<PlayerManager>().players);

            if (!won)
            {
                if (ignoreCollisionCoroutine != null)
                {
                    StopCoroutine(ignoreCollisionCoroutine);
                }
                ignoreCollisionCoroutine = StartCoroutine(IgnorePlayerEnemyCollision());
            }

            refOverworldManager.EnableOverworldObjects();
            refCombatManager.DestroyCombatObjects(won);

            musicOverworld.Play();
            musicBattle.Stop();
        }
    }

    public void TryEndDialogue()
    {
        stateMachine.TryUpdateConnection((int)GameState.overworld);
    }

    public IEnumerator IgnorePlayerEnemyCollision()
    {
        Physics.IgnoreLayerCollision(11, 12, true);

        yield return new WaitForSecondsRealtime(2.0f);

        Physics.IgnoreLayerCollision(11, 12, false);
    }

    public enum GameState
    {
        overworld = 0,
        combat = 1,
        dialogue = 2
    }
}

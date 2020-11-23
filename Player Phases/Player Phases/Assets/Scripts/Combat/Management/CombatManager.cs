using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CombatManager : MonoBehaviour
{
    private CombatGrid refCombatGrid;
    private SHTANKCamera refCombatCamera;
    private PhaseManager refPhaseManager;

    public OverworldEnemyController currentEnemy;
    public CanvasGroup combatSpecificUI;
    public ManagedAudio enterCombatAudio;

    private AbilityProcessor refAbilityProcessor;

    void Start()
    {
        refCombatGrid = GetComponent<CombatGrid>();
        refCombatCamera = FindObjectOfType<SHTANKCamera>();
        refPhaseManager = FindObjectOfType<PhaseManager>();

        combatSpecificUI.transform.DOLocalMoveY(-200, 0);
    }

    public void MyUpdate()
    {
        refPhaseManager.MyUpdate();
    }

    public void InitiatePhase(CombatPhase initialPhase, Vector3 collisionPosition, OverworldEnemyController targetEnemy)
    {
        if (initialPhase == CombatPhase.Null)
        {
            Debug.LogWarning("Combat Initiator: Initial Phase probably shouldn't be Null.");
        }

        FindObjectOfType<UtilityAudioManager>().QueueSound(enterCombatAudio);

        refCombatGrid.SpawnGrid(collisionPosition);
        refCombatCamera.InitiateCombatCamera();
        refPhaseManager.InitiateCombat(initialPhase, collisionPosition, targetEnemy);

        currentEnemy = targetEnemy;

        combatSpecificUI.transform.DOLocalMoveY(0, 0.25f);
    }

    public void DestroyCombatObjects(bool won)
    {
        refCombatGrid.DestroyGrid();
        refPhaseManager.DestroyCharacters();

        if (won)
        {
            Destroy(currentEnemy.gameObject);
        }

        combatSpecificUI.transform.DOLocalMoveY(-200, 0.25f);
    }

    public AbilityProcessor GetAbilityProcessor()
    {
        if (refAbilityProcessor == null)
        {
            refAbilityProcessor = new AbilityProcessor(FindObjectOfType<CombatGrid>(), FindObjectOfType<AbilityForecast>(), FindObjectOfType<MovementAbilityForecast>(), FindObjectOfType<PhaseManager>());
        }

        return refAbilityProcessor;
    }
}

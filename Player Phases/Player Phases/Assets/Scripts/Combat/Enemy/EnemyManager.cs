using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private PhaseManager refPhaseManager;

    public GameObject enemyPrefab;
    public List<ExampleEnemy> enemies = new List<ExampleEnemy>();

    private CombatGrid refCombatGrid;

    private List<string> names = new List<string>();

    private void Start()
    {
        refPhaseManager = FindObjectOfType<PhaseManager>();

        refCombatGrid = FindObjectOfType<CombatGrid>();

        names.Add("S-Fear");
        names.Add("P-Trol");
        names.Add("Humungalaga");
        names.Add("Some Robot");
        names.Add("M4L-Function");
        names.Add("Barely a Threat");
        names.Add("Crazy Eyes");
        names.Add("A-Nihilate");
        names.Add("D-Struction");
        names.Add("Peter Plum");
    }

    public void SpawnEnemies()
    {
        for (int i = 0; i < 1; ++i)
        {
            // spawn enemies and add them to the list
            ExampleEnemy tmp = Instantiate(enemyPrefab, transform).GetComponent<ExampleEnemy>();
            enemies.Add(tmp);
            tmp.transform.position = refCombatGrid.grid[refCombatGrid.gridWidth - i - 1, refCombatGrid.gridHeight - 1].obj.transform.position;
            refCombatGrid.grid[refCombatGrid.gridWidth - i - 1, refCombatGrid.gridHeight - 1].character = tmp;
            tmp.myGridSpace = refCombatGrid.grid[refCombatGrid.gridWidth - i - 1, refCombatGrid.gridHeight - 1];

            int rand = Random.Range(0, names.Count);
            tmp.name = names[rand];
            names.RemoveAt(rand);
        }
    }

    public void EnemyActions()
    {
        // enable enemy actions
        StartCoroutine(EnemyActionsCoroutine());
    }

    private IEnumerator EnemyActionsCoroutine()
    {
        // loop through all enemies and have them do their actions
        for (int i = 0; i < enemies.Count; ++i)
        {
            enemies[i].DoAI();

            yield return new WaitForSeconds(0.5f);
        }

        // this current phase is done, let the Phase Manager know it's time to move to the next phase
        refPhaseManager.NextPhase();
    }
}

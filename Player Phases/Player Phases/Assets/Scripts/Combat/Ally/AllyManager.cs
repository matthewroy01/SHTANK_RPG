using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllyManager : CharacterManager
{
    [Header("List of characters in combat after spawning")]
    public List<Character> allies = new List<Character>();

    private void Start()
    {
        refPhaseManager = FindObjectOfType<PhaseManager>();
        refCombatGrid = FindObjectOfType<CombatGrid>();

        refPlayerManager = FindObjectOfType<PlayerManager>();
        refEnemyManager = FindObjectOfType<EnemyManager>();
    }

    public void MyUpdate()
    {
        // remove dead allies
        for (int i = 0; i < allies.Count; ++i)
        {
            if (allies[i].GetDead() == true)
            {
                StartCoroutine(allies[i].Death());
                refEnemyManager.RemoveFromAggroTargets(allies[i]);

                allies.RemoveAt(i);
            }
        }
    }

    public override void AddCharacter(CharacterDefinition def, GridSpace gridSpace)
    {
        // spawn ally and add it to the list
        Character tmp = Instantiate(characterPrefab, transform).GetComponent<Character>();
        allies.Add(tmp);

        // get a valid spawning space for this character
        GridSpace spawnSpace = gridSpace;

        tmp.transform.position = spawnSpace.obj.transform.position;
        spawnSpace.character = tmp;
        tmp.myGridSpace = spawnSpace;

        AssignCharacterValues(tmp.gameObject, def, Character_Affiliation.ally);
        AssignPassive(tmp, def);

        allies.Add(tmp);
        refEnemyManager.AddToAggroTargets(tmp);
    }

    public void AllyActions()
    {
        for (int i = 0; i < allies.Count; ++i)
        {
            allies[i].StartTurn(refCombatGrid);
        }

        // enable enemy actions
        StartCoroutine(EnemyActionsCoroutine());
    }

    private IEnumerator EnemyActionsCoroutine()
    {
        // loop through all enemies and have them do their actions
        for (int i = 0; i < allies.Count; ++i)
        {
            allies[i].DoAI();

            while (!allies[i].GetIdle())
            {
                yield return new WaitForEndOfFrame();
            }

            yield return new WaitForSeconds(0.5f);
        }

        // this current phase is done, let the Phase Manager know it's time to move to the next phase
        refPhaseManager.NextPhase();
    }
}
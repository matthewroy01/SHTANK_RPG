using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Threading;

public class AIProcessor : MonoBehaviour
{
    public int threadComplete = 0;
    public List<Thread_GridSpace> threadedGridSpaces = new List<Thread_GridSpace>();

    public AIResult result;

    private Ability savedAbility;
    private Character savedCharacter;
    private GridSpace savedAggroTarget;
    private List<Effect> savedEffects = new List<Effect>();

    private CombatGrid refCombatGrid;
    private AbilityForecast refAbilityForecast;
    private MovementAbilityForecast refMovementAbilityForecast;
    private PhaseManager refPhaseManager;

    private void Start()
    {
        refCombatGrid = FindObjectOfType<CombatGrid>();
        refAbilityForecast = FindObjectOfType<AbilityForecast>();
        refMovementAbilityForecast = FindObjectOfType<MovementAbilityForecast>();
        refPhaseManager = FindObjectOfType<PhaseManager>();

        result = new AIResult(int.MinValue, null, CombatDirection.up, false, null, false);
    }

    public void ProcessAbility(Character character, Ability abil, List<GridSpace> movementSpaces, GridSpace aggroTarget)
    {
        result.Reset();
        threadedGridSpaces.Clear();

        List<Thread> threads = new List<Thread>();

        // save data to be used by the threads
        savedAbility = abil;
        savedCharacter = character;
        savedAggroTarget = aggroTarget;

        // fill out list of special threaded grid spaces so they can be marked if they are currently in use or not
        for (int i = 0; i < movementSpaces.Count; ++i)
        {
            threadedGridSpaces.Add(new Thread_GridSpace(movementSpaces[i]));
        }

        threadComplete = threadedGridSpaces.Count;

        for (int i = 0; i < threadedGridSpaces.Count; ++i)
        {
            threads.Add(new Thread(ProcessAbilityStart));
            threads[threads.Count - 1].Start();
        }

        StartCoroutine(CheckThreadCompletion());
    }

    private void ProcessAbilityStart()
    {
        int index = -1;

        for (int i = 0; i < threadedGridSpaces.Count; ++i)
        {
            if (threadedGridSpaces[i].processing == false)
            {
                threadedGridSpaces[i].processing = true;
                index = i;

                break;
            }
        }

        AbilityProcessor tmp = new AbilityProcessor(refCombatGrid, refAbilityForecast, refMovementAbilityForecast, refPhaseManager);

        ProcessAbilityWithDirection(tmp, index, CombatDirection.up, false);
        ProcessAbilityWithDirection(tmp, index, CombatDirection.down, false);
        ProcessAbilityWithDirection(tmp, index, CombatDirection.left, false);
        ProcessAbilityWithDirection(tmp, index, CombatDirection.right, false);

        if (savedAbility.GetType().Name == "PathAbility")
        {
            ProcessAbilityWithDirection(tmp, index, CombatDirection.up, true);
            ProcessAbilityWithDirection(tmp, index, CombatDirection.down, true);
            ProcessAbilityWithDirection(tmp, index, CombatDirection.left, true);
            ProcessAbilityWithDirection(tmp, index, CombatDirection.right, true);
        }

        threadComplete--;
        Thread.CurrentThread.Join();
    }

    private void ProcessAbilityWithDirection(AbilityProcessor ap, int index, CombatDirection direction, bool flipped)
    {
        // use the ability processor to process the saved ability given the direction and whether or not it was flipped
        List<GridSpace> gridSpaces = new List<GridSpace>();
        gridSpaces = ap.ProcessAbility(savedCharacter, threadedGridSpaces[index].gridSpace, 1, direction, flipped, false);

        int score = CalculateScore(gridSpaces, savedAbility.GetTotalDamage());
        CompareScore(score, threadedGridSpaces[index].gridSpace, direction, flipped);
    }

    private int CalculateScore(List<GridSpace> gridSpaces, int damage)
    {
        int score = 0;

        // if the grid spaces don't have the aggro target, default to the minimum possible score
        if (!gridSpaces.Contains(savedAggroTarget))
        {
            score = int.MinValue;
            return score;
        }

        // loop through grid spaces and figure out the value of this attack based on damage dealt
        for (int i = 0; i < gridSpaces.Count; ++i)
        {
            Character character = gridSpaces[i].character;

            // if there is a character present on this grid space
            if (character != null)
            {
                // subtract from the score for hitting friendlies
                if (character.affiliation == savedCharacter.affiliation)
                {
                    score -= damage;
                }
                // otherwise increase the score for hiting foes
                else
                {
                    score += damage;

                    // if the damage dealt results in a KO, this attack is very optimal
                    int trueDamage = 0;
                    for (int j = 0; j < savedEffects.Count; ++j)
                    {
                        trueDamage += character.GetTrueDamage(savedEffects[j].value, savedEffects[j]);

                        if (character.healthCurrent - trueDamage <= 0)
                        {
                            score += 100;
                        }
                    }
                }
            }
        }

        return score;
    }

    private void CompareScore(int score, GridSpace toMoveTo, CombatDirection direction, bool flipped)
    {
        // don't bother checking the score if the score the minimum possible value
        if (score != int.MinValue)
        {
            // if the score is higher than the previous best, overwrite it
            if (score > result.score)
            {
                result = new AIResult(score, toMoveTo, direction, flipped, savedAbility, true);
            }
        }
    }

    private IEnumerator CheckThreadCompletion()
    {
        float timer = 0.0f;

        while (threadComplete > 0)
        {
            timer += Time.deltaTime;

            yield return null;
        }

        Debug.Log("AI processing took " + timer + " seconds");

        // let the character that was processing AI that it has finished
        savedCharacter.AIFinished(result);
    }
}

public class Thread_GridSpace
{
    public GridSpace gridSpace;
    public bool processing = false;

    public Thread_GridSpace(GridSpace gs)
    {
        gridSpace = gs;
    }
}

public class AIResult
{
    public int score = 0;
    public GridSpace toMoveTo;
    public CombatDirection direction;
    public bool flipped;
    public Ability ability;
    public bool foundAggroTarget = false;

    public AIResult(int s, GridSpace gs, CombatDirection cd, bool f, Ability a, bool fat)
    {
        score = s;
        toMoveTo = gs;
        direction = cd;
        flipped = f;
        ability = a;
        foundAggroTarget = fat;
    }

    public void Reset()
    {
        score = int.MinValue;
        toMoveTo = null;
        ability = null;
        foundAggroTarget = false;
    }
}
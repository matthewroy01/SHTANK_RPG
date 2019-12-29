using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleEnemy : Character
{
    private CombatGrid refCombatGrid;

    private void Start()
    {
        refCombatGrid = FindObjectOfType<CombatGrid>();
    }

    public void DoAI()
    {
        List<GridSpace> path = refCombatGrid.GetAStar(refCombatGrid, myGridSpace, refCombatGrid.grid[0, 0]);
        StartCoroutine(MoveAlongPath(path));

        /*int rand = Random.Range(0, 4);
        Vector2 dir = Vector2.zero;
        
        switch (rand)
        {
            case 0:
            {
                dir = Vector2.up;
                break;
            }
            case 1:
            {
                dir = Vector2.left;
                break;
            }
            case 2:
            {
                dir = Vector2.down;
                break;
            }
            case 3:
            {
                dir = Vector2.right;
                break;
            }
            default:
            {
                break;
            }
        }

        transform.position = (Vector2)transform.position + dir;*/
    }

    IEnumerator MoveAlongPath(List<GridSpace> path)
    {
        for (int i = 0; i < path.Count; ++i)
        {
            transform.position = path[i].obj.transform.position;

            yield return new WaitForSeconds(0.5f);
        }
    }
}

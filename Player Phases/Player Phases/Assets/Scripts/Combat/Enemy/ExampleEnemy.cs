using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleEnemy : Character
{
    public void DoAI()
    {
        int rand = Random.Range(0, 4);
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

        transform.position = (Vector2)transform.position + dir;
    }
}

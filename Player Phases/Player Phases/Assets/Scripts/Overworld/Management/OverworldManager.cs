using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldManager : MonoBehaviour
{
    private OverworldPlayerController refPlayerController;

    private List<OverworldEnemyController> refEnemyControllers = new List<OverworldEnemyController>();

    private void Start()
    {
        refPlayerController = FindObjectOfType<OverworldPlayerController>();

        OverworldEnemyController[] tmp = FindObjectsOfType<OverworldEnemyController>();
        refEnemyControllers = new List<OverworldEnemyController>(tmp);
    }

    public void MyUpdate()
    {
        refPlayerController.MyUpdate();
    }

    public void DisableOverworldObjects()
    {
        // disable player
        refPlayerController.gameObject.SetActive(false);

        // disable enemies
        for (int i = 0; i < refEnemyControllers.Count; ++i)
        {
            refEnemyControllers[i].gameObject.SetActive(false);
        }
    }

    public void EnableOverworldObjects()
    {
        // enable player
        refPlayerController.gameObject.SetActive(true);

        // enable enemies
        for (int i = 0; i < refEnemyControllers.Count; ++i)
        {
            refEnemyControllers[i].gameObject.SetActive(true);
        }
    }
}

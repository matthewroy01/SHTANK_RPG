using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldManager : MonoBehaviour
{
    private OverworldPlayerController refPlayerController;

    private List<OverworldEnemyController> refEnemyControllers = new List<OverworldEnemyController>();

    private List<Collider> refOverworldCollision = new List<Collider>();

    public GameObject environmentParent;

    private void Start()
    {
        refPlayerController = FindObjectOfType<OverworldPlayerController>();

        OverworldEnemyController[] tmp = FindObjectsOfType<OverworldEnemyController>();
        refEnemyControllers = new List<OverworldEnemyController>(tmp);

        refOverworldCollision = new List<Collider>(environmentParent.GetComponentsInChildren<Collider>());

        // remove overworld colliders that aren't overworld specific
        for (int i = 0; i < refOverworldCollision.Count; ++i)
        {
            if (!(refOverworldCollision[i].gameObject.layer == 0 || refOverworldCollision[i].gameObject.CompareTag("Untagged")))
            {
                refOverworldCollision.RemoveAt(i);
                --i;
            }
        }
    }

    public void MyUpdate()
    {
        refPlayerController.MyUpdate();

        for (int i = 0; i < refEnemyControllers.Count; ++i)
        {
            refEnemyControllers[i].MyUpdate();
        }
    }

    public void DisableOverworldObjects()
    {
        // disable player
        refPlayerController.gameObject.SetActive(false);

        // disable enemies
        for (int i = 0; i < refEnemyControllers.Count; ++i)
        {
            refEnemyControllers[i].Disable();
        }

        // disable overworld-specific collision
        for (int i = 0; i < refOverworldCollision.Count; ++i)
        {
            refOverworldCollision[i].enabled = false;
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

            refEnemyControllers[i].Enable();
        }

        // enable overworld-specific collision
        for (int i = 0; i < refOverworldCollision.Count; ++i)
        {
            refOverworldCollision[i].enabled = true;
        }
    }
}

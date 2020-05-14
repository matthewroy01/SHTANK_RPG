using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldManager : MonoBehaviour
{
    private OverworldPlayerController refPlayerController;

    private void Start()
    {
        refPlayerController = FindObjectOfType<OverworldPlayerController>();
    }

    public void MyUpdate()
    {
        refPlayerController.MyUpdate();
    }

    public void DisableOverworldObjects()
    {
        refPlayerController.gameObject.SetActive(false);
    }

    public void EnableOverworldObjects()
    {
        refPlayerController.gameObject.SetActive(true);
    }
}

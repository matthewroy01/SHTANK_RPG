using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldPlayerController : MonoBehaviour
{
    [HideInInspector]
    public OverworldPlayerMovement refMovement;
    [HideInInspector]
    public OverworldPlayerAnimation refAnimation;

    private void Start()
    {
        if (!TryGetComponent(out refMovement))
        {
            Debug.LogError("OverworldPlayerMovement could not find component OverworldPlayerMovement.");
        }

        if (!TryGetComponent(out refAnimation))
        {
            Debug.LogError("OverworldPlayerMovement could not find component OverworldPlayerAnimation.");
        }
    }

    public void MyUpdate()
    {
        refMovement.MyUpdate();
        refAnimation.MyUpdate();
    }
}

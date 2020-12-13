using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldPlayerController : MonoBehaviour
{
    [HideInInspector]
    public OverworldPlayerMovement refMovement;
    [HideInInspector]
    public OverworldPlayerAnimation refAnimation;
    [HideInInspector]
    public OverworldPlayerCreateWalls refCreateWalls;

    [HideInInspector]
    public UtilityAudioManager refAudioManager;

    private void Start()
    {
        if (!TryGetComponent(out refMovement))
        {
            Debug.LogError("OverworldPlayerController could not find component OverworldPlayerMovement.");
        }

        if (!TryGetComponent(out refAnimation))
        {
            Debug.LogError("OverworldPlayerController could not find component OverworldPlayerAnimation.");
        }

        if (!TryGetComponent(out refCreateWalls))
        {
            Debug.LogError("OverworldPlayerController could not find component OverworldPlayerCreateWalls.");
        }

        refAudioManager = FindObjectOfType<UtilityAudioManager>();
    }

    public void MyUpdate()
    {
        refMovement.MyUpdate();
        refAnimation.MyUpdate();
        //refCreateWalls.MyUpdate();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldPlayerController : MonoBehaviour
{
    [HideInInspector]
    public SHTANKManager refSHTANKManager;

    [HideInInspector]
    public OverworldPlayerMovement refMovement;
    [HideInInspector]
    public OverworldPlayerAnimation refAnimation;
    [HideInInspector]
    public OverworldPlayerCreateWalls refCreateWalls;
    [HideInInspector]
    public OverworldPlayerInteract refInteract;

    [HideInInspector]
    public UtilityAudioManager refAudioManager;

    private void Start()
    {
        refSHTANKManager = FindObjectOfType<SHTANKManager>();

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

        if (!TryGetComponent(out refInteract))
        {
            Debug.LogError("OverworldPlayerController could not find component OverworldPlayerInteract.");
        }

        refAudioManager = FindObjectOfType<UtilityAudioManager>();
    }

    public void MyUpdate()
    {
        refMovement.MyUpdate();
        refAnimation.MyUpdate();
        //refCreateWalls.MyUpdate();
        refInteract.MyUpdate();
    }

    public void MyFixedUpdate()
    {
        refMovement.MyFixedUpdate();
    }
}
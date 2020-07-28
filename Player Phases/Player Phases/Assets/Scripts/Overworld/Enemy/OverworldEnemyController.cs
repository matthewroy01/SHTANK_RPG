﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldEnemyController : MonoBehaviour
{
    [HideInInspector]
    public OverworldEnemyMovement refMovement;

    private void Start()
    {
        if (!TryGetComponent(out refMovement))
        {
            Debug.LogError("OverworldEnemyController could not find OverworldEnemyMovmeent.");
        }
    }

    public void MyUpdate()
    {
        refMovement.MyUpdate();
    }
}

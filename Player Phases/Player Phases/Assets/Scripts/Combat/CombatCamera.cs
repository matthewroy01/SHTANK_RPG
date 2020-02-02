using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatCamera : MonoBehaviour
{
    [Header("Camera movement speed")]
    [Range(0.0f, 1.0f)]
    public float lerpSpeed;
    public CombatCameraMode cameraMode;

    private EnemyManager refEnemyManager;
    private PlayerManager refPlayerManager;
    private CombatGrid refCombatGrid;

    private Vector3 targetPosition;

    void Start()
    {
        refEnemyManager = FindObjectOfType<EnemyManager>();
        refPlayerManager = FindObjectOfType<PlayerManager>();
        refCombatGrid = FindObjectOfType<CombatGrid>();
    }

    void FixedUpdate()
    {
        switch(cameraMode)
        {
            case CombatCameraMode.centerOnCharacters:
            {
                CalcPosCenteredOnCharacters();
                break;
            }
            case CombatCameraMode.centerOnGrid:
            {
                CalcPosCenteredOnGrid();
                break;
            }
        }

        Move();
    }

    private void CalcPosCenteredOnCharacters()
    {
        float avgX = 0, avgY = 0;
        int i;

        for (i = 0; i < refEnemyManager.enemies.Count; ++i)
        {
            avgX += refEnemyManager.enemies[i].transform.position.x;
            avgY += refEnemyManager.enemies[i].transform.position.y;
        }

        for (i = 0; i < refPlayerManager.players.Count; ++i)
        {
            avgX += refPlayerManager.players[i].transform.position.x;
            avgY += refPlayerManager.players[i].transform.position.y;
        }

        avgX /= refEnemyManager.enemies.Count + refPlayerManager.players.Count;
        avgY /= refEnemyManager.enemies.Count + refPlayerManager.players.Count;

        targetPosition = new Vector3(avgX, avgY, transform.position.z);
    }

    private void CalcPosCenteredOnGrid()
    {
        Vector3 corner1, corner2;

        corner1 = refCombatGrid.grid[0, 0].obj.transform.position;
        corner2 = refCombatGrid.grid[refCombatGrid.gridWidth - 1, refCombatGrid.gridHeight - 1].obj.transform.position;

        targetPosition = new Vector3((corner1.x + corner2.x) / 2.0f, (corner1.y + corner2.y) / 2.0f, transform.position.z);
    }

    private void Move()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, lerpSpeed);
    }

    public enum CombatCameraMode { centerOnGrid, centerOnCharacters };
}

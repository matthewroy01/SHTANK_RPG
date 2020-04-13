using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatCamera : MonoBehaviour
{
    [Header("Camera Movement")]
    [Range(0.0f, 1.0f)]
    public float lerpSpeed;
    [Range(0.0f, 1.0f)]
    public float edgeSensitiviy;
    public CombatCameraEdgeMode edgeMode;
    public bool requireTab = false;
    public Vector2 movementAreaSize;
    private Bounds maxBounds;

    [Header("Initial Camera Placement")]
    public CombatCameraCenterMode cameraMode;

    private EnemyManager refEnemyManager;
    private PlayerManager refPlayerManager;
    private CombatGrid refCombatGrid;

    private Vector3 defaultPosition;

    void Start()
    {
        refEnemyManager = FindObjectOfType<EnemyManager>();
        refPlayerManager = FindObjectOfType<PlayerManager>();
        refCombatGrid = FindObjectOfType<CombatGrid>();

        // this function must be called after the Combat Grid has already initialized itself, otherwise a null reference will occur when trying to get the position of the [0, 0] grid space from it
        CenterCamera();

        maxBounds = new Bounds(transform.position, movementAreaSize);
    }

    void FixedUpdate()
    {
        Vector2 mousePos = Input.mousePosition;
        mousePos /= new Vector2(Screen.width, Screen.height);

        //Debug.Log(mousePos);

        Vector2 velocity = Vector2.zero;

        switch(edgeMode)
        {
            case CombatCameraEdgeMode.rectangular:
            {
                // if the mouse is near the edge of the screen, add to a velocity
                if (mousePos.x < 0.0f + edgeSensitiviy)
                {
                    velocity += Vector2.left;
                }

                if (mousePos.x > 1.0f - edgeSensitiviy)
                {
                    velocity += Vector2.right;
                }

                if (mousePos.y < 0.0f + edgeSensitiviy)
                {
                    velocity += Vector2.down;
                }

                if (mousePos.y > 1.0f - edgeSensitiviy)
                {
                    velocity += Vector2.up;
                }
                break;
            }
            case CombatCameraEdgeMode.circular:
            {
                if (mousePos.x < 0.0f + edgeSensitiviy ||
                    mousePos.x > 1.0f - edgeSensitiviy ||
                    mousePos.y < 0.0f + edgeSensitiviy ||
                    mousePos.y > 1.0f - edgeSensitiviy)
                {
                    velocity = mousePos - new Vector2(0.5f, 0.5f);
                }
                break;
            }
        }

        // calculate new position
        Vector3 newVector = Vector3.Lerp(transform.position, transform.position + (Vector3)velocity.normalized, lerpSpeed);

        // if the new position is within the camera's maxmimum bounds
        if (maxBounds.Contains(newVector))
        {
            // check if a button should be held to control the camera's movement (good for when the Insepctor needs to be interacted with in the editor)
            if (requireTab)
            {
                if (Input.GetKey(KeyCode.Tab))
                {
                    // move the camera
                    transform.position = newVector;
                }
            }
            else
            {
                // move the camera
                transform.position = newVector;
            }
        }
    }

    private void CenterCamera()
    {
        switch (cameraMode)
        {
            case CombatCameraCenterMode.centerOnCharacters:
            {
                defaultPosition = CalcPosCenteredOnCharacters();
                break;
            }
            case CombatCameraCenterMode.centerOnGrid:
            {
                defaultPosition = CalcPosCenteredOnGrid();
                break;
            }
            case CombatCameraCenterMode.maintainPosition:
            {
                defaultPosition = transform.position;
                break;
            }
        }

        transform.position = defaultPosition;
    }

    private Vector3 CalcPosCenteredOnCharacters()
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

        return new Vector3(avgX, avgY, transform.position.z);
    }

    private Vector3 CalcPosCenteredOnGrid()
    {
        Vector3 corner1, corner2;

        corner1 = refCombatGrid.grid[0, 0].obj.transform.position;
        corner2 = refCombatGrid.grid[refCombatGrid.gridWidth - 1, refCombatGrid.gridHeight - 1].obj.transform.position;

        return new Vector3((corner1.x + corner2.x) / 2.0f, (corner1.y + corner2.y) / 2.0f, transform.position.z);
    }

    public enum CombatCameraCenterMode { centerOnGrid, centerOnCharacters, maintainPosition};
    public enum CombatCameraEdgeMode { rectangular, circular };
}

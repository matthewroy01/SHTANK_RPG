using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SHTANKCamera : MonoBehaviour
{
    [Range(0, 1)]
    public float followSpeed;
    [Range(0, 1)]
    public float turnSpeed;
    public bool functionRegardlessOfState = false;

    [Header("Overworld Camera Movement")]
    public Transform cameraTargetOverworld;

    [Header("Combat Camera Movement")]
    public Transform cameraTargetCombat;
    public Vector3 initialOffset;
    [Range(0.0f, 1.0f)]
    public float moveSpeed;
    [Range(0.0f, 1.0f)]
    public float edgeSensitivity;
    [Range(0.0f, 1.0f)]
    public float smoothing;
    public CombatCameraEdgeMode edgeMode;
    public bool requireTab = false;
    public Vector2 movementAreaSize;
    private Bounds maxBounds;

    [Header("Combat Initial Camera Placement")]
    public CombatCameraCenterMode cameraMode;

    [Header("Camera Shake")]
    public bool shouldShake;
    public Vector3 shakePrevRotation;
    [Range(0, 2)]
    public float shakeIntensity;
    public float shakeDuration;

    private EnemyManager refEnemyManager;
    private PlayerManager refPlayerManager;
    private CombatGrid refCombatGrid;
    private Vector3 defaultPosition;

    void Start()
    {
        refEnemyManager = FindObjectOfType<EnemyManager>();
        refPlayerManager = FindObjectOfType<PlayerManager>();
        refCombatGrid = FindObjectOfType<CombatGrid>();

        // function regardless of state for special purposes (such as the concept art scene)
        if (functionRegardlessOfState)
        {
            InitiateCombatCamera();
        }
    }

    private void FixedUpdate()
    {
        // function regardless of state for special purposes (such as the concept art scene)
        if (functionRegardlessOfState)
        {
            MouseMovement();

            MoveCamera(cameraTargetCombat, smoothing, turnSpeed);
        }
    }

    public void CameraFunctionalityCombat()
    {
        MouseMovement();

        MoveCamera(cameraTargetCombat, smoothing, turnSpeed);
    }

    public void CameraFunctionalityOverworld()
    {
        MoveCamera(cameraTargetOverworld, followSpeed, turnSpeed);

        if (shouldShake)
        {
            Shake();
        }
    }

    private void MoveCamera(Transform target, float lerpPos, float lerpRot)
    {
        transform.position = Vector3.Lerp(transform.position, target.position, lerpPos);
        transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, lerpRot);
    }

    private void Shake()
    {
        shakePrevRotation = transform.eulerAngles;
        // reset rotation
        transform.rotation = Quaternion.Euler(shakePrevRotation);
        // generate random numbers
        float tmpx = Random.Range(-shakeIntensity, shakeIntensity);
        float tmpy = Random.Range(-shakeIntensity, shakeIntensity);
        float tmpz = Random.Range(-shakeIntensity, shakeIntensity);
        // set new rotation
        transform.rotation = Quaternion.Euler(shakePrevRotation.x + tmpx, shakePrevRotation.y + tmpy, shakePrevRotation.z + tmpz);
    }

    public void InitiateCombatCamera()
    {
        // this function must be called after the Combat Grid has already initialized itself, otherwise a null reference will occur when trying to get the position of the [0, 0] grid space from it
        CenterCamera();

        cameraTargetCombat.position = defaultPosition;
        maxBounds = new Bounds(defaultPosition, new Vector3(movementAreaSize.x, 1.0f, movementAreaSize.y));
    }

    private void MouseMovement()
    {
        Vector2 mousePos = Input.mousePosition;
        mousePos /= new Vector2(Screen.width, Screen.height);

        Vector3 velocity = Vector3.zero;

        switch (edgeMode)
        {
            case CombatCameraEdgeMode.rectangular:
            {
                // if the mouse is near the edge of the screen, add to a velocity
                if (mousePos.x < 0.0f + edgeSensitivity)
                {
                    velocity += Vector3.left;
                }

                if (mousePos.x > 1.0f - edgeSensitivity)
                {
                    velocity += Vector3.right;
                }

                if (mousePos.y < 0.0f + edgeSensitivity)
                {
                    velocity += Vector3.back;
                }

                if (mousePos.y > 1.0f - edgeSensitivity)
                {
                    velocity += Vector3.forward;
                }
                break;
            }
            case CombatCameraEdgeMode.circular:
            {
                if (mousePos.x < 0.0f + edgeSensitivity ||
                    mousePos.x > 1.0f - edgeSensitivity ||
                    mousePos.y < 0.0f + edgeSensitivity ||
                    mousePos.y > 1.0f - edgeSensitivity)
                {
                    velocity = new Vector3(mousePos.x, 0.0f, mousePos.y) - new Vector3(0.5f, 0.0f, 0.5f);
                }
                break;
            }
        }

        // calculate new position
        Vector3 newVector = Vector3.Lerp(cameraTargetCombat.position, cameraTargetCombat.position + (Vector3)velocity.normalized, moveSpeed);

        // if the new position is within the camera's maxmimum bounds
        if (maxBounds.Contains(newVector))
        {
            // check if a button should be held to control the camera's movement (good for when the Insepctor needs to be interacted with in the editor)
            if (requireTab)
            {
                if (Input.GetKey(KeyCode.Tab))
                {
                    // move the camera
                    cameraTargetCombat.position = newVector;
                }
            }
            else
            {
                // move the camera
                cameraTargetCombat.position = newVector;
            }
        }
    }

    private void CenterCamera()
    {
        switch (cameraMode)
        {
            case CombatCameraCenterMode.centerOnCharacters:
            {
                defaultPosition = CalcPosCenteredOnCharacters() + initialOffset;
                break;
            }
            case CombatCameraCenterMode.centerOnGrid:
            {
                defaultPosition = CalcPosCenteredOnGrid() + initialOffset;
                break;
            }
            case CombatCameraCenterMode.maintainPosition:
            {
                defaultPosition = cameraTargetCombat.position + initialOffset;
                break;
            }
        }

        cameraTargetCombat.position = defaultPosition;
    }

    private Vector3 CalcPosCenteredOnCharacters()
    {
        float avgX = 0, avgZ = 0;
        int i;

        for (i = 0; i < refEnemyManager.enemies.Count; ++i)
        {
            avgX += refEnemyManager.enemies[i].transform.position.x;
            avgZ += refEnemyManager.enemies[i].transform.position.z;
        }

        for (i = 0; i < refPlayerManager.players.Count; ++i)
        {
            avgX += refPlayerManager.players[i].transform.position.x;
            avgZ += refPlayerManager.players[i].transform.position.z;
        }

        avgX /= refEnemyManager.enemies.Count + refPlayerManager.players.Count;
        avgZ /= refEnemyManager.enemies.Count + refPlayerManager.players.Count;

        return new Vector3(avgX, cameraTargetCombat.position.y, avgZ);
    }

    private Vector3 CalcPosCenteredOnGrid()
    {
        Vector3 corner1, corner2;

        corner1 = refCombatGrid.grid[0, 0].obj.transform.position;
        corner2 = refCombatGrid.grid[refCombatGrid.gridWidth - 1, refCombatGrid.gridHeight - 1].obj.transform.position;

        return new Vector3((corner1.x + corner2.x) / 2.0f, cameraTargetCombat.position.y, (corner1.z + corner2.z) / 2.0f);
    }

    public enum CombatCameraCenterMode { centerOnGrid, centerOnCharacters, maintainPosition};
    public enum CombatCameraEdgeMode { rectangular, circular };
}

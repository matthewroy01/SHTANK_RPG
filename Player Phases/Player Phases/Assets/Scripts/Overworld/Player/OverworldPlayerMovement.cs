using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldPlayerMovement : MonoBehaviour
{
    [Header("Movement Speed")]
    public float speedMovement;
    public float speedAirHindrance;
    private float movHor;
    private float movVer;

    [Header("Jumping")]
    public float jumpForce;
    [HideInInspector]
    public bool grounded;
    public Transform groundCheck;
    public LayerMask groundLayerMask;
    public ManagedAudio jumpAudio;

    [Header("Artificial Gravity")]
    public float gravityMultiplier;

    [Header("Cartoon Shadow")]
    public GameObject shadow;

    [HideInInspector]
    public Rigidbody refRigidbody;

    private OverworldPlayerController controller;

    private void Start()
    {
        if (!TryGetComponent(out controller))
        {
            Debug.LogError("OverworldPlayerMovement could not find component OverworldPlayerController.");
        }

        if (!TryGetComponent(out refRigidbody))
        {
            Debug.LogError("OverworldPlayerMovement could not find component Rigidbody.");
        }
    }

    private void Update()
    {
        // move cartoon shadow
        MoveShadow();
    }

    public void MyUpdate()
    {
        // movement
        GetAxes();
        MovementWASD();

        // jumping
        CheckGround();
        Jump();

        // artificial gravity
        ApplyArtificalGravity();
    }

    private void GetAxes()
    {
        movHor = Input.GetAxis("Horizontal");
        movVer = Input.GetAxis("Vertical");
    }

    private void MovementWASD()
    {
        Vector3 newVelocity = new Vector3(movHor * speedMovement, refRigidbody.velocity.y, movVer * speedMovement);
        refRigidbody.velocity = newVelocity;
    }

    private void CheckGround()
    {
        grounded = Physics.OverlapSphere(groundCheck.position, 0.1f, groundLayerMask).Length > 0;
    }

    private void Jump()
    {
        if (grounded && Input.GetKeyDown(KeyCode.Space))
        {
            refRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            controller.refAudioManager.QueueSound(jumpAudio);
        }
    }

    private void ApplyArtificalGravity()
    {
        refRigidbody.AddForce(Vector3.down * gravityMultiplier);
    }

    private void MoveShadow()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, Vector3.down, out hit, groundLayerMask);

        if (hit.transform)
        {
            shadow.SetActive(true);

            shadow.transform.position = new Vector3(hit.point.x, hit.point.y, hit.point.z);

            float distance = Vector3.Distance(transform.position, hit.point);
            if (distance != 0)
            {
                float dividedDistance = 1.0f / distance;
                Mathf.Clamp(dividedDistance, 0.001f, 1.0f);
                shadow.transform.localScale = Vector3.ClampMagnitude(Vector3.one * dividedDistance, 3.464f);
            }
        }
        else
        {
            // don't display a shadow
            shadow.SetActive(false);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldPlayerMovement : MonoBehaviour
{
    [Header("Movement Speed")]
    public float speedMovement;
    [Range(0.0f, 1.0f)]
    public float speedAirHindrance;
    private float movHor;
    private float movVer;

    [Header("Jumping")]
    public float jumpForce;
    [HideInInspector]
    public bool grounded;
    private bool canWallJump = true;
    public Transform groundCheck;
    public LayerMask groundLayerMask;
    public ManagedAudio jumpAudio;
    public LayerMask wallLayerMask;
    public ParticleSystem wallJumpParts;
    private Vector3 jumpTrajectory;

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

    private void FixedUpdate()
    {
        // artificial gravity
        ApplyArtificalGravity();
    }

    public void MyUpdate()
    {
        // movement
        GetAxes();
        MovementWASD();

        // jumping
        CheckGround();
        Jump();
        WallJump();
    }

    private void GetAxes()
    {
        movHor = Input.GetAxis("Horizontal");
        movVer = Input.GetAxis("Vertical");
    }

    private void MovementWASD()
    {
        if (grounded)
        {
            Vector3 newVelocity = new Vector3(movHor * speedMovement, refRigidbody.velocity.y, movVer * speedMovement);
            refRigidbody.velocity = newVelocity;
        }
        else
        {
            refRigidbody.AddForce(new Vector3(movHor * speedMovement * speedAirHindrance, 0.0f, movVer * speedMovement * speedAirHindrance));

            Vector3 horizontalVelocity = new Vector3(refRigidbody.velocity.x, 0.0f, refRigidbody.velocity.z);
            if (horizontalVelocity.magnitude > speedMovement)
            {
                horizontalVelocity = horizontalVelocity.normalized * speedMovement;
                refRigidbody.velocity = new Vector3(horizontalVelocity.x, refRigidbody.velocity.y, horizontalVelocity.z);
            }
        }
    }

    private void CheckGround()
    {
        grounded = Physics.OverlapSphere(groundCheck.position, 0.1f, groundLayerMask).Length > 0;

        if (grounded && !canWallJump)
        {
            canWallJump = true;
        }
    }

    private void Jump()
    {
        if (grounded && Input.GetKeyDown(KeyCode.Space))
        {
            refRigidbody.velocity = new Vector3(refRigidbody.velocity.x, 0.0f, refRigidbody.velocity.z);
            refRigidbody.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            controller.refAudioManager.QueueSound(jumpAudio);

            jumpTrajectory = refRigidbody.velocity;
        }
    }

    private void WallJump()
    {
        Vector3 direction = new Vector3(jumpTrajectory.x, 0.0f, jumpTrajectory.z);
        RaycastHit hit;
        Physics.Raycast(transform.position, direction, out hit, 0.7f, wallLayerMask);

        if (canWallJump && !grounded && hit.transform != null && Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 reflection = Vector3.Reflect(direction, hit.normal);

            refRigidbody.velocity = Vector3.zero;
            refRigidbody.AddForce(new Vector3(reflection.x, jumpForce, reflection.z), ForceMode.Impulse);

            GameObject tmp = Instantiate(wallJumpParts, hit.point, wallJumpParts.transform.rotation).gameObject;

            Destroy(tmp, 0.5f);
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

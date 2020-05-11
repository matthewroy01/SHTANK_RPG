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

    [Header("Artificial Gravity")]
    public float gravityMultiplier;

    [HideInInspector]
    public Rigidbody refRigidbody;

    private void Start()
    {
        if (!TryGetComponent(out refRigidbody))
        {
            Debug.LogError("OverworldPlayerMovement could not find component Rigidbody.");
        }
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
        }
    }

    private void ApplyArtificalGravity()
    {
        refRigidbody.AddForce(Vector3.down * gravityMultiplier);
    }
}

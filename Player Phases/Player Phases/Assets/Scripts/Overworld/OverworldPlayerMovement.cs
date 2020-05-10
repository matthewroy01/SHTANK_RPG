using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldPlayerMovement : MonoBehaviour
{
    [Header("Movement Speed")]
    public float speedMovement;

    private Rigidbody refRigidbody;

    private void Start()
    {
        if (!TryGetComponent(out refRigidbody))
        {
            Debug.LogError("OverworldPlayerMovement could not find component Rigidbody.");
        }
    }

    void FixedUpdate()
    {
        MovementWASD();
    }

    private void MovementWASD()
    {
        refRigidbody.velocity = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }
}

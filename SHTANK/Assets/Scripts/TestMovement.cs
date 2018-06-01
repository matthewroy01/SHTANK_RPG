using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TestMovement : MonoBehaviour
{
	[Header("Movement variables")]
	public float movSpeed;

	private Rigidbody rb;
	private Vector3 defaultPosition;

	void Start()
	{
		defaultPosition = transform.position;
		rb = GetComponent<Rigidbody>();
	}
	
	void Update()
	{
		rb.velocity = new Vector3(Input.GetAxis("Horizontal") * movSpeed, rb.velocity.y, Input.GetAxis("Vertical") * movSpeed);

		if (transform.position.y < -5f)
		{
			transform.position = defaultPosition;
			rb.velocity = Vector3.zero;
		}
	}
}

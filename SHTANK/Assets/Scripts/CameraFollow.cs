using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
	public Transform target;
	[Range(0, 1)]
	public float followSpeed;
	[Range(0, 1)]
	public float rotationSpeed;

	void Start ()
	{
		
	}
	
	void Update ()
	{
		transform.position = Vector3.Lerp(transform.position, target.position, followSpeed);
		transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, rotationSpeed);
	}
}

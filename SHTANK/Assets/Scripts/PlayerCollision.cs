using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private CombatCreateGrid refCreateGrid;
   
	void Start ()
    {
		refCreateGrid = GameObject.FindObjectOfType<CombatCreateGrid>();
	}
	
	void Update ()
    {
		if (Input.GetKeyDown(KeyCode.Space))
		{
			
		}
	}

	void OnTriggerEnter(Collider other)
	{
		// collision with an enemy in the overworld
		if (other.CompareTag("Enemy"))
		{
			//refCreateGrid.ScanAndCreate();
		}
	}
}
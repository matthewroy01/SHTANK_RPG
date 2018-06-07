using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerMovement : MonoBehaviour
{
	[Header("Movement variables")]
	public float movSpeed;

	[Header("Default height while in the grid")]
	public float defaultGridY;

	[Header("Current movement type")]
	public PlayerMovementType movType;

	// this is private but 2D arrays don't appear in the Inspector anyway
	private GridSpace[,] grid;

	[Header("Current position in the grid (0, 0 is the bottom left corner)")]
	public int gridCurrentX;
	public int gridCurrentY;

	private Rigidbody rb;
	private Vector3 defaultPosition;

	void Start()
	{
		defaultPosition = transform.position;
		rb = GetComponent<Rigidbody>();

		grid = GameObject.FindObjectOfType<CombatCreateGrid>().grid;
	}
	
	void Update()
	{
		if (movType == PlayerMovementType.free)
		{
			MovementFree();
		}
		else if (movType == PlayerMovementType.grid)
		{
			UpdateGridPosition();

			MovementGrid();
		}

		KillPlane();
	}

	public void SwitchMovementType(PlayerMovementType newType)
	{
		movType = newType;
	}

	private void MovementFree()
	{
		Debug.Log("Movement free");
		rb.velocity = new Vector3(Input.GetAxis("Horizontal") * movSpeed, rb.velocity.y, Input.GetAxis("Vertical") * movSpeed);
	}

	private void MovementGrid()
	{
		// moving in four directions
		if (Input.GetButtonDown("Up"))
		{
			if (CheckGridAt(0, 1))
			{
				grid[gridCurrentX, gridCurrentY].occupied = false;
				gridCurrentY++;
				UpdateGridPosition();
			}
		}
		else if (Input.GetButtonDown("Down"))
		{
			if (CheckGridAt(0, -1))
			{
				grid[gridCurrentX, gridCurrentY].occupied = false;
				gridCurrentY--;
				UpdateGridPosition();
			}
		}
		else if (Input.GetButtonDown("Left"))
		{
			if (CheckGridAt(-1, 0))
			{
				grid[gridCurrentX, gridCurrentY].occupied = false;
				gridCurrentX--;
				UpdateGridPosition();
			}
		}
		else if (Input.GetButtonDown("Right"))
		{
			if (CheckGridAt(1, 0))
			{
				grid[gridCurrentX, gridCurrentY].occupied = false;
				gridCurrentX++;
				UpdateGridPosition();
			}
		}
	}

	private void KillPlane()
	{
		if (transform.position.y < -5f)
		{
			transform.position = defaultPosition;
			rb.velocity = Vector3.zero;
		}
	}

	private bool CheckGridAt(int x, int y)
	{
		// check X so we don't get an out of bounds error
		if (gridCurrentX + x > grid.GetLength(0) - 1 || gridCurrentX + x < 0)
		{
			return false;
		}

		// check Y so we don't get an out of bounds error
		if (gridCurrentY + y > grid.GetLength(1) - 1 || gridCurrentY + y < 0)
		{
			return false;
		}

		// check if the next grid space is accessible
		if (grid[gridCurrentX + x, gridCurrentY + y].attributes.passable == true && grid[gridCurrentX + x, gridCurrentY + y].occupied == false)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	private void UpdateGridPosition()
	{
		// update the player's position
		transform.position = new Vector3(grid[gridCurrentX, gridCurrentY].transform.position.x, defaultGridY, grid[gridCurrentX, gridCurrentY].transform.position.z);

		// set this new grid space to being occupied
		grid[gridCurrentX, gridCurrentY].occupied = true;
	}
}

public enum PlayerMovementType { free, grid, none };
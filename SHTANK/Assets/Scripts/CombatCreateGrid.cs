using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatCreateGrid : MonoBehaviour
{
	[Header("Defaults")]
	private Vector3 defaultPosition;
	public float defaultY;
	public float distanceBetween;

	[Header("Grid size (should use odd numbers so that there's a middle)")]
	public int gridWidth;
	public int gridHeight;

	[Header("Grid prefabs")]
	public LayerMask detectable;
	public GameObject gridSpacePrefab;

	[Header("Final grid")]
	public GridSpace[,] grid;

	void Start ()
	{
		// save the default position just in case
		defaultPosition = new Vector3(transform.position.x, defaultY, transform.position.z);

		// check for impossible values
		if (CheckForErrors() == true)
		return;

		grid = new GridSpace[gridWidth, gridHeight];

		// scan the surrounding area and create grid objects
		CreateGrid();
	}

	public void CreateGrid()
	{
		// calculate the starting corner of our grid
		float startingX = defaultPosition.x - (gridWidth / 2), startingY = defaultPosition.y - (gridHeight / 2);

		// multiply by distance between to move the corner the proper distance away
		startingX *= distanceBetween;
		startingY *= distanceBetween;

		int iX = 0, iY = 0;

		// loop through for all width and height
		for (float x = 0;  x < gridWidth * distanceBetween; x += distanceBetween)
		{
			for (float y = 0; y < gridHeight * distanceBetween; y += distanceBetween)
			{
				// calculate the position
				Vector3 position = new Vector3(startingX + x, defaultY, startingY + y);

				// instantiate the grid space
				grid[iX, iY] = Instantiate(gridSpacePrefab, position, gridSpacePrefab.transform.rotation).GetComponent<GridSpace>();

				// change the scale according to the distance between grid spaces
				grid[iX, iY].gameObject.transform.localScale *= distanceBetween;

				// change the parent
				grid[iX, iY].gameObject.transform.parent = gameObject.transform;

				++iY;
			}

			++iX;
			iY = 0;
		}
	}

	public void ScanGrid()
	{
		// loop through for all width and height
		for (int x = 0;  x < grid.GetLength(0); ++x)
		{
			for (int y = 0; y < grid.GetLength(1); ++y)
			{
				// check to see what is in our grid space
				CheckGridSpace(grid[x, y].transform.position, ref grid[x, y]);
			}
		}
	}

	private void CheckGridSpace(Vector3 myPosition, ref GridSpace myCurrent)
	{
		RaycastHit hit;

		// raise our raycast up just in case
		// ***THIS DOESN'T SEEM TO WORK EVEN WHEN LOWERING DEFAULT Y TO SOMETHING LESS THAN 100?
		myPosition.y += 100.0f;

		// look for detectable objects and set current to the corresponding object from grid objects
		if (Physics.Raycast(myPosition, Vector3.down, out hit, Mathf.Infinity, detectable))
		{
			if (hit.collider.gameObject.CompareTag("Wall"))
			{
				Attributes newAttributes = new Attributes(false, false, false);
				myCurrent.InitGridSpace(GridSpaceType.wall, newAttributes, Color.green);
			}
			else if (hit.collider.gameObject.CompareTag("Water"))
			{
				Attributes newAttributes = new Attributes(false, true, true);
				myCurrent.InitGridSpace(GridSpaceType.water, newAttributes, Color.blue);
			}
			else
			{
				Attributes newAttributes = new Attributes(true, true, true);
				myCurrent.InitGridSpace(GridSpaceType.normal, newAttributes, Color.white);
			}
		}
	}

	private bool CheckForErrors()
	{
		// this function used to prevent infinite loops via impossible values in the inspector
		bool threwError = false;

		if (distanceBetween <= 0)
		{
			Debug.LogError("distanceBetween cannot be <= 0.");
			threwError = true;
		}

		if (gridWidth <= 0)
		{
			Debug.LogError("gridWidth cannot be <= 0.");
			threwError = true;
		}

		if (gridHeight <= 0)
		{
			Debug.LogError("gridHeight cannot be <= 0.");
			threwError = true;
		}

		return threwError;
	}
}
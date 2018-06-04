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
	public GridSpace[] gridObjects;
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
		ScanAndCreate();
	}

	private void ScanAndCreate()
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

	            // by default, a grid space is empty
	            GridSpace current = gridObjects[0];

	            // instantiate the grid space
				grid[iX, iY] = Instantiate(gridSpacePrefab, position, gridSpacePrefab.transform.rotation).GetComponent<GridSpace>();

				// check to see what is in our grid space
	            CheckGridSpace(position, ref grid[iX, iY], distanceBetween);

	            ++iY;
			}

         ++iX;
         iY = 0;
		}
	}

   private void CheckGridSpace(Vector3 myPosition, ref GridSpace myCurrent, float scale)
   {
      RaycastHit hit;

      // raise our raycast up just in case
      // ***THIS DOESN'T SEEM TO WORK EVEN WHEN LOWERING DEFAULT Y TO SOMETHING LESS THAN 100?
      myPosition.y += 100.0f;

      // change the scale according to the distance between grid spaces
      myCurrent.gameObject.transform.localScale *= distanceBetween;

      // change the parent
      myCurrent.gameObject.transform.parent = gameObject.transform;

      // look for detectable objects and set current to the corresponding object from grid objects
      if (Physics.Raycast(myPosition, Vector3.down, out hit, Mathf.Infinity, detectable))
      {
         if (hit.collider.gameObject.CompareTag("Wall"))
         {
         	Attributes newAttributes = new Attributes(false, false, false);
			myCurrent.InitGridSpace(GridSpaceType.wall, myCurrent.gameObject, Color.green, myPosition, newAttributes);
         }
         else if (hit.collider.gameObject.CompareTag("Water"))
         {
			Attributes newAttributes = new Attributes(false, true, true);
			myCurrent.InitGridSpace(GridSpaceType.water, myCurrent.gameObject, Color.blue, myPosition, newAttributes);
         }
         else
         {
			Attributes newAttributes = new Attributes(true, true, true);
			myCurrent.InitGridSpace(GridSpaceType.water, myCurrent.gameObject, Color.white, myPosition, newAttributes);
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

		if (gridObjects.Length == 0)
		{
			Debug.LogError("gridObjects is empty.");
			threwError = true;
		}

		return threwError;
	}
}
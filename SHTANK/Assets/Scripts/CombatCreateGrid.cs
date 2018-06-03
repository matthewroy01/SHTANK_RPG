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

	void Start ()
	{
		// save the default position just in case
		defaultPosition = new Vector3(transform.position.x, defaultY, transform.position.z);

		// check for impossible values
		if (CheckForErrors() == true)
			return;

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

      // loop through for all width and height
		for (float x = 0; x < gridWidth * distanceBetween; x += distanceBetween)
		{
			for (float y = 0; y < gridHeight * distanceBetween; y += distanceBetween)
			{
            // calculate the position
				Vector3 position = new Vector3(startingX + x, defaultY, startingY + y);

            // by default, a grid space is empty
            GridSpace current = gridObjects[0];

            // check to see what is in our grid space
            CheckGridSpace(position, ref current);

            // instantiate the grid space and change the color accordingly
				GameObject tmp = Instantiate(current.obj, position, transform.rotation);
				tmp.GetComponent<Renderer>().material.color = current.color;
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
            myCurrent = gridObjects[1];
         }
         else if (hit.collider.gameObject.CompareTag("Water"))
         {
            myCurrent = gridObjects[2];
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

[System.Serializable]
public class GridSpace
{
	public GameObject obj;
	public Color color = Color.white;
	public bool passable = true;
	public bool transparent = true;
	public bool canFlyOver = true;
}

public enum GridSpaceType { normal = 0, wall = 1, water = 2};
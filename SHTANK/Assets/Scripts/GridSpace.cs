using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSpace : MonoBehaviour
{
	public GridSpaceType type = GridSpaceType.normal;
   	public Attributes attributes;
   	public bool occupied;

	private Material refMaterial;

   	void Awake()
   	{
   		// find our render material
   		refMaterial = GetComponent<Renderer>().material;

   		// set the color by default to white
   		refMaterial.color = Color.white;
   	}

	public void InitGridSpace(GridSpaceType myType, Attributes myAttributes, Color myCol)
	{
		// set the default values
		type = myType;
		attributes = myAttributes;
		occupied = false;

		// set the color
		refMaterial.color = myCol;
	}
}

[System.Serializable]
public class Attributes
{
	public Attributes(bool pass, bool trans, bool canFly)
	{
		passable = pass;
		transparent = trans;
		canFlyOver = canFly;
	}

  	public bool passable;
   	public bool transparent;
   	public bool canFlyOver;
}

public enum GridSpaceType { normal = 0, wall = 1, water = 2 };
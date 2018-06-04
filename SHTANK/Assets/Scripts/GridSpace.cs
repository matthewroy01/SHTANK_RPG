using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSpace : MonoBehaviour
{
	public GridSpaceType type = GridSpaceType.normal;
   	public GameObject obj;
   	public Color col = Color.white;
   	public Vector3 pos;
   	public Attributes attributes;

	private Material refMaterial;

   	void Awake()
   	{
   		refMaterial = GetComponent<Renderer>().material;
   	}

	public void InitGridSpace(GridSpaceType myType, GameObject myObj, Color myCol, Vector3 myPos, Attributes myAttributes)
	{
		type = myType;
		obj = myObj;
		col = myCol;
		pos = myPos;
		attributes = myAttributes;

		refMaterial.color = col;
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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveAva : Passive
{
    [Header("Shadow Walls")]
    public int shadowWallDuration;
    private List<GridSpace> shadowWallSpaces = new List<GridSpace>();
    private int counter;

    public override void ReceiveEvent(PassiveEventID id)
    {
        switch(id)
        {
            case PassiveEventID.turnStart:
            {
                CountShadowWall();
                break;
            }
        }
    }

    public override void ReceiveEvent<T>(PassiveEventID id, T param)
    {
        switch(id)
        {
            case PassiveEventID.storeGridSpace:
            {
                try
                {
                    StoreGridSpace(param as GridSpace);
                }
                catch
                {
                    Debug.LogError("Generic parameter sent into ReceiveEvent function was not a GridSpace.");
                }
                break;
            }
        }
    }

    private void StoreGridSpace(GridSpace toStore)
    {
        // if there was already a shadow wall in action, clear the existing ones
        if (counter > 0)
        {
            ClearShadowWalls();
        }

        // reset the counter and add the new grid space
        counter = 0;
        shadowWallSpaces.Add(toStore);

        // enable the shadow wall at that space if no character is occupying it
        if (toStore.character == null)
        {
            toStore.SetTerrainType(GridSpace_TerrainType.wall_artificial);
            toStore.shadowWall.SetActive(true);
        }
    }

    private void CountShadowWall()
    {
        counter++;

        // clear the walls once they have existed for long enough
        if (counter >= shadowWallDuration)
        {
            ClearShadowWalls();
        }
    }

    private void ClearShadowWalls()
    {
        for (int i = 0; i < shadowWallSpaces.Count; ++i)
        {
            shadowWallSpaces[i].SetTerrainType(shadowWallSpaces[i].GetTerrainTypeOriginal());
            shadowWallSpaces[i].shadowWall.SetActive(false);
        }

        shadowWallSpaces.Clear();
    }
}
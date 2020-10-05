using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Moveset", menuName = "Movesets/Moveset", order = 1)]
public class Moveset : ScriptableObject
{
    public Ability ability1;
    public Ability ability2;
    public Ability ability3;
    public Ability ability4;
}

public class MovesetData
{
    public AbilityData data1;
    public AbilityData data2;
    public AbilityData data3;
    public AbilityData data4;

    public float unlocked;

    public MovesetData(Moveset moveset, float unlock)
    {
        data1 = new AbilityData();
        data2 = new AbilityData();
        data3 = new AbilityData();
        data4 = new AbilityData();

        unlocked = unlock;

        Reset(moveset);
    }

    public bool GetAvailability(int abilNum)
    {
        switch (abilNum)
        {
            case 1:
            {
                if (data1.availableUses > 0 || data1.availableUses == -1)
                {
                    return true;
                }
                break;
            }
            case 2:
            {
                if (data2.availableUses > 0 || data2.availableUses == -1)
                {
                    return true;
                }
                break;
            }
            case 3:
            {
                if (data3.availableUses > 0 || data3.availableUses == -1)
                {
                    return true;
                }
                break;
            }
            case 4:
            {
                if (data4.availableUses > 0 || data4.availableUses == -1)
                {
                    return true;
                }
                break;
            }
        }

        return false;
    }

    public void Reset(Moveset moveset)
    {
        if (moveset.ability1 != null)
        {
            data1.availableUses = moveset.ability1.uses;
        }
        if (moveset.ability2 != null)
        {
            data2.availableUses = moveset.ability2.uses;
        }
        if (moveset.ability3 != null)
        {
            data3.availableUses = moveset.ability3.uses;
        }
        if (moveset.ability4 != null)
        {
            data4.availableUses = moveset.ability4.uses;
        }
    }

    public void Use(int abilNum)
    {
        switch(abilNum)
        {
            case 1:
            {
                if (data1.availableUses > 0)
                {
                    data1.availableUses--;
                }
                break;
            }
            case 2:
            {
                if (data2.availableUses > 0)
                {
                    data2.availableUses--;
                }
                break;
            }
            case 3:
            {
                if (data3.availableUses > 0)
                {
                    data3.availableUses--;
                }
                break;
            }
            case 4:
            {
                if (data4.availableUses > 0)
                {
                    data4.availableUses--;
                }
                break;
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RadialMenu : MonoBehaviour
{
    public List<TextMeshProUGUI> abilityTitles = new List<TextMeshProUGUI>();

    private int currentAbilityNum;

    public void Enable(Moveset moveset)
    {
        // reset ability number to display correctly
        ResetAbilityNum(moveset);


    }

    private void ResetAbilityNum(Moveset moveset)
    {
        // reset ability num to 0
        currentAbilityNum = 0;

        // count the number of abilities
        if (moveset.ability1 != null)
        {
            currentAbilityNum++;
        }
        if (moveset.ability2 != null)
        {
            currentAbilityNum++;
        }
        if (moveset.ability3 != null)
        {
            currentAbilityNum++;
        }
        if (moveset.ability4 != null)
        {
            currentAbilityNum++;
        }
    }

    public void Disable()
    {

    }
}
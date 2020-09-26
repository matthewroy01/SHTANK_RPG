using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Summon Ability", menuName = "Movesets/Summon Ability", order = 1)]
public class SummonAbility : Ability
{
    [Header("Character to Summon")]
    public CharacterDefinition summon;
}

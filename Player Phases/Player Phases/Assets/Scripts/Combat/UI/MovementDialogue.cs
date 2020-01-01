using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Movement Dialogue", menuName = "Dialogue/Movement Dialogue", order = 1)]
public class MovementDialogue : ScriptableObject
{
    [TextArea(1, 2)]
    public string[] quotes;

    [TextArea(1, 2)]
    public string deathQuote;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Status Definition", menuName = "Status Defintion", order = 1)]
public class StatusUIDefinition : ScriptableObject
{
    public Sprite sprite;
    [TextArea(3, 10)]
    public string toolTip;
}

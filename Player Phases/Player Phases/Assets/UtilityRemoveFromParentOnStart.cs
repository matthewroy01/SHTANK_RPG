using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilityRemoveFromParentOnStart : MonoBehaviour
{
    private void Awake()
    {
        transform.SetParent(null);
    }
}

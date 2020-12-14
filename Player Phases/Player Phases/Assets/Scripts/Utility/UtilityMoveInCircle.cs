using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilityMoveInCircle : MonoBehaviour
{
    public float multiplierSpeed = 1.0f;
    public float multiplierHor = 1.0f;
    public float multiplierVer = 1.0f;

    void FixedUpdate()
    {
        transform.localPosition = new Vector3(Mathf.Cos(Time.time * multiplierSpeed) * multiplierHor, transform.localPosition.y, Mathf.Sin(Time.time * multiplierSpeed) * multiplierVer);
    }
}

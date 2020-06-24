using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UIEffect : MonoBehaviour
{
    public abstract void DoEffect();
}

public enum UIEffect_Type { pop, shake };
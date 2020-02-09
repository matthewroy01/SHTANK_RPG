using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UtilityOpenURL : MonoBehaviour
{
    public string url;
    public bool closeBuild;

    public void OpenURL()
    {
        Application.OpenURL(url);

        if (closeBuild)
        {
            Application.Quit();
        }
    }
}

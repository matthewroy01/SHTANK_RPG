using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UtilityLoadScene : MonoBehaviour
{
    public string sceneName;

    public void LoadScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}

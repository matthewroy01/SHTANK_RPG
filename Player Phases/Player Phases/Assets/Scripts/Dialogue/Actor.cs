using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SHTANKCutscenes
{
    public class Actor
    {
        public string name;
        public GameObject obj;
        // ANIMATOR VARIABLE HERE FOR CONTROLLING ANIMATIONS

        private Coroutine animationCoroutine;

        public Actor(GameObject go, string n/*, PARAMS ARRAY HERE FOR ANIMATOR OBJECTS*/)
        {
            name = n;
            obj = go;
        }

        public Vector3 GetScreenSpacePosition()
        {
            Vector3 result = Camera.main.WorldToScreenPoint(obj.transform.position + (Vector3.up * 0.5f));

            result -= new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0.0f);
            result.z = 0.0f;
            Debug.Log(result);

            return result;
        }
    }
}
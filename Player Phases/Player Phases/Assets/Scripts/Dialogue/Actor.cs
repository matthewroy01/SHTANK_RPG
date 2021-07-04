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
    }
}
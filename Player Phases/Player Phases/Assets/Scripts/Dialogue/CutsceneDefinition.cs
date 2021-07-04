using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SHTANKCutscenes
{
    [CreateAssetMenu(fileName = "New Cutscene", menuName = "Dialogue/Cutscene", order = 1)]
    public class CutsceneDefinition : ScriptableObject
    {
        public List<Step> steps = new List<Step>();
    }

    public class Participant
    {
        public GameObject obj;
        // ANIMATOR VARIABLE HERE FOR CONTROLLING ANIMATIONS

        public Participant(GameObject go/*, PARAMS ARRAY HERE FOR ANIMATOR OBJECTS*/)
        {
            obj = go;
        }
    }

    [System.Serializable]
    public class Step
    {
        public string speaker;
        
        [TextArea(3, 10)]
        public string text;
    }
}
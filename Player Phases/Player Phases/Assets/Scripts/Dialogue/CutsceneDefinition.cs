using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SHTANKCutscenes
{
    [CreateAssetMenu(fileName = "New Cutscene", menuName = "Dialogue/Cutscene", order = 1)]
    public class CutsceneDefinition : ScriptableObject
    {
        private List<Speaker> participants = new List<Speaker>();

        public List<Step> steps = new List<Step>();
    }

    public class Participant
    {

    }

    [System.Serializable]
    public class Step
    {
        public string speaker;
        
        [TextArea(3, 10)]
        public string text;
    }
}
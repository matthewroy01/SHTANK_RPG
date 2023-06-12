using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace SHTANKCutscenes
{
    [CreateAssetMenu(fileName = "New Cutscene", menuName = "Dialogue/Cutscene", order = 1)]
    public class CutsceneDefinition : ScriptableObject
    {
        public List<string> actors = new List<string>();
        public List<Step> steps = new List<Step>();
    }

    [System.Serializable]
    public class Step
    {
        public string speaker;
        
        [TextArea(3, 10)]
        public string text;

        /* ADDITIONAL VARIABLES HERE FOR ANIMATION, ETC */
    }
}
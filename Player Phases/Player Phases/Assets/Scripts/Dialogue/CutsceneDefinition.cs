using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SHTANKCutscenes
{
    [CreateAssetMenu(fileName = "New Cutscene", menuName = "Dialogue/Cutscene", order = 1)]
    public class CutsceneDefinition : ScriptableObject
    {
        private List<Speaker> participants = new List<Speaker>();
    }

    public class Participant
    {

    }
}
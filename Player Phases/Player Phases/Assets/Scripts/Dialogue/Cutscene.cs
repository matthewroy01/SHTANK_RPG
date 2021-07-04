using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SHTANKCutscenes
{
    public class Cutscene
    {
        // will probably need to replace the data type here to whatever object controls the animation of a character
        
        public List<string> actorNames = new List<string>();
        public List<Step> steps = new List<Step>();

        public Cutscene(CutsceneDefinition cutsceneDefinition)
        {
            steps = cutsceneDefinition.steps;

            // look through the steps and compile all of the actor names
            foreach (Step step in steps)
            {
                if (!actorNames.Contains(step.speaker))
                {
                    actorNames.Add(step.speaker);
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SHTANKCutscenes
{
    public class Cutscene
    {
        // will probably need to replace the data type here to whatever object controls the animation of a character
        public List<Participant> participants = new List<Participant>();
        public List<Step> steps = new List<Step>();

        public Cutscene(List<Step> newSteps, params GameObject[] newParticipants)
        {
            // save steps
            steps = newSteps;

            // save participants
            for (int i = 0; i < newParticipants.Length; ++i)
            {
                participants.Add(new Participant(newParticipants[i]));
            }
        }
    }
}

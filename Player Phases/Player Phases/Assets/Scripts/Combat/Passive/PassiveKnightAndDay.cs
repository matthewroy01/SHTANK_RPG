using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveKnightAndDay : Passive
{
    [Header("Lumen's \"voice\"")]
    public MovementDialogueProcessor lumenDialogueProcessor;
    public List<LumenRequest> requests = new List<LumenRequest>();

    private PassiveEventID currentGoalID;

    public override void ReceiveEvent(PassiveEventID id)
    {
        switch (id)
        {
            case PassiveEventID.turnStart:
            {
                // at the start of each turn, lumen makes a request that shows up as dialogue
                LumenMakeRequest();
                break;
            }
        }
    }

    private void LumenMakeRequest()
    {
        if (requests.Count > 0)
        {
            LumenRequest request = LumenGetWeightedRequest();

            // display text to show Lumen "speaking"
            lumenDialogueProcessor.Display(request.message);

            // save goal ID to keep track of the request for this turn
            currentGoalID = request.eventID;
        }
    }

    private LumenRequest LumenGetWeightedRequest()
    {
        int rand = Random.Range(0, requests.Count - 1);
        return requests[rand];
    }

    [System.Serializable]
    public class LumenRequest
    {
        public string message;
        public int weight = 1;

        [Header("This event triggering fulfills this request")]
        public PassiveEventID eventID;
    }
}
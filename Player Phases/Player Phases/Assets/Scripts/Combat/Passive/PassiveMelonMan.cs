using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassiveMelonMan : Passive
{
    [Header("Melon's Curse Status UI")]
    public StatusUIDefinition melonsCurseStatus;

    public override List<StatusUIDefinition> GetActiveStatuses()
    {
        List<StatusUIDefinition> statuses = new List<StatusUIDefinition>();
        statuses.Add(melonsCurseStatus);

        return statuses;
    }
}

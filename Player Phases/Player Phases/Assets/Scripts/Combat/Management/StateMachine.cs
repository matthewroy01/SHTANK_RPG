using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    readonly public List<StateMachineConnection> connections = new List<StateMachineConnection>();
    public int currentState;

    public StateMachine(int startingState)
    {
        // set the current state and establish the connections
        currentState = startingState;
    }

    public StateMachine(int startingState, params StateMachineConnection[] startingConnections)
    {
        // set the current state and establish the connections
        currentState = startingState;
        connections = new List<StateMachineConnection>(startingConnections);
    }

    public bool CheckConnection(int f, int t)
    {
        // check if the provided connection exists in this state machine
        for (int i = 0; i < connections.Count; ++i)
        {
            if (connections[i].from == f && connections[i].to == t)
            {
                return true;
            }
        }

        return false;
    }

    public bool CheckConnection(StateMachineConnection connection)
    {
        // check if the provided connection exists in this state machine
        if (connections.Contains(connection))
        {
            return true;
        }

        return false;
    }

    public bool TryUpdateConnection(int t)
    {
        // check if a connection from the current state to the provided state exists and if it does, switch to the new state
        for (int i = 0; i < connections.Count; ++i)
        {
            if (connections[i].from == currentState && connections[i].to == t)
            {
                Debug.Log("StateMachine: state updated from " + currentState + " to " + t + ".");

                currentState = t;

                return true;
            }
        }

        return false;
    }
}

public class StateMachineConnection
{
    readonly public int from;
    readonly public int to;

    public StateMachineConnection(int f, int t)
    {
        from = f;
        to = t;
    }
}
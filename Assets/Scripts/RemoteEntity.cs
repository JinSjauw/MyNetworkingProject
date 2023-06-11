using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoteEntity : MonoBehaviour
{
    private Queue<StatePayload> interpolationQueue;
    private StatePayload lastProcessedState;
    private uint nextTickToProcess;
    private uint lastReceivedTick;
    private float interpTime;

    private void Awake()
    {
        interpolationQueue = new Queue<StatePayload>();
        lastProcessedState = new StatePayload
        {
            position = transform.position,
            rotation = transform.rotation,
        };
    }

    //Interpolate
    private void InterpolateNPC()
    {
        StatePayload interpolateTarget = interpolationQueue.Dequeue();
        
        //Interpolation time should be packet RTT

        while (interpTime < 1)
        {
            interpTime += 1f / Constants.MS_PER_TICK;
            Debug.Log(interpTime);
            
            transform.position = Vector3.Lerp(lastProcessedState.position, interpolateTarget.position, interpTime);
            transform.rotation = Quaternion.Slerp(lastProcessedState.rotation, interpolateTarget.rotation, interpTime).normalized;
        }
        
        lastProcessedState = interpolateTarget;
        interpTime = 0;
        
        //If it reaches target destination you can stop and go to the next state
        /*if (Vector3.Distance(transform.position, interpolateTarget.position) < 0.05)
        {
            lastProcessedState = interpolateTarget;
            interpTime = 0;
        }*/
    }
    
    public void ReceiveServerState(StatePayload _serverState)
    {
        if (_serverState.serverTick > lastReceivedTick)
        {
            interpolationQueue.Enqueue(_serverState);
            lastReceivedTick = _serverState.serverTick;
        }
    }

    public void HandleTick()
    {
        //Only start interpolating after a delay
        if (interpolationQueue.Count > 0)
        {
            InterpolateNPC();
        }
    }
}

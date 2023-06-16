using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class RemoteEntity : MonoBehaviour
{
    private Queue<StatePayload> interpolationQueue;
    private StatePayload lastProcessedState;
    private StatePayload interpolateTarget;
    private StatePayload lastReceivedState;
    private float interpTime = 0;
    private float interpDelay = 0.128f;
    //private bool interpolating = false;
    private PlayerManager playerManager;
    private void Awake()
    {
        interpolationQueue = new Queue<StatePayload>();
        playerManager = GetComponent<PlayerManager>();
    }

    //Interpolate
    private void InterpolateNPC()
    {
        //Interpolating
        //Set lastState
        //Last State null? // lastState = currentState
        //Dequeue Newest State
        //Start Interpolating after 100ms delay // lastProcessState.TimeStamp + .100 ms >= clientTime

        if (interpolationQueue.Count < 1)
        {
            playerManager.IsRunning(false, Vector2.zero);
            return;
        }
        
        if (lastProcessedState.timeStamp <= 0 && interpolationQueue.Count > 0)
        {
            lastProcessedState = interpolationQueue.Dequeue();
        }
        
        if (interpolateTarget.timeStamp <= 0 && interpolationQueue.Count > 0)
        {
            interpolateTarget = interpolationQueue.Dequeue();
        }
        else if(interpolateTarget.timeStamp <= 0 && interpolationQueue.Count <= 0)
        {
            return;
        }
        
        if (GameManager.clientTimer - lastProcessedState.timeStamp < interpDelay)
        {
            return;
        }
        //Debug.Log($"Client Clock: {GameManager.clientTimer} lastState Time: {lastProcessedState.timeStamp}  Difference: {GameManager.clientTimer - lastProcessedState.timeStamp}");

        //Debug.Log(interpolateTarget);
        //Debug.Log(lastProcessedState);

        if (Vector3.Distance(interpolateTarget.position, lastProcessedState.position) > 0.1f)
        {
            playerManager.IsRunning(true, interpolateTarget.inputDirection);
        }
        else
        {
            playerManager.IsRunning(false, interpolateTarget.inputDirection);
        }
        
        float difference = (interpolateTarget.timeStamp - lastProcessedState.timeStamp) * 1000f;
        //Debug.Log(difference);
        float timeStep = 1f / difference;
        //Debug.Log(timeStep);
        
        while (interpTime <= 1)
        {
            interpTime += timeStep;
            //Interpolate
            transform.position = Vector3.Lerp(lastProcessedState.position, interpolateTarget.position, interpTime);
            transform.rotation = Quaternion.Slerp(lastProcessedState.rotation, interpolateTarget.rotation, interpTime).normalized;
        }

        lastProcessedState = interpolateTarget;
        if (interpolationQueue.Count > 0)
        {
            interpolateTarget = interpolationQueue.Dequeue();
            interpTime = 0;
        }
        
    }
    
    public void ReceiveServerState(StatePayload _serverState)
    {
        if (_serverState.timeStamp > lastReceivedState.timeStamp)
        {
            if(_serverState.position != lastReceivedState.position || 
               _serverState.rotation != lastReceivedState.rotation)
                
            interpolationQueue.Enqueue(_serverState);
            lastReceivedState = _serverState;
        }
    }

    public void HandleTick()
    {
        InterpolateNPC();
    }
}

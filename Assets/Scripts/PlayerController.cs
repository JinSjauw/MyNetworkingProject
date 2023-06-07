using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct InputPayload
{
    public uint tick;
    public bool[] inputs;
    
    public override string ToString()
    {
        return $"Tick: {tick}  Inputs: {inputs}";
    }
}

public struct StatePayload
{
    public uint tick;
    public Vector3 position;

    public override string ToString()
    {
        return $"Tick: {tick}  Position: {position}";
    }
}

public class PlayerController : MonoBehaviour
{
    private uint currentTick;
    private float timer;
    private float tickLength;

    private StatePayload[] stateBuffer;
    private InputPayload[] inputBuffer;
    private float moveSpeed = 5f;

    private ClientPrediction clientPrediction;
    
    private void Awake()
    {
        tickLength = Constants.MS_PER_TICK;
        tickLength /= 1000;

        stateBuffer = new StatePayload[Constants.BUFFER_SIZE];
        inputBuffer = new InputPayload[Constants.BUFFER_SIZE];

        moveSpeed /= Constants.MS_PER_TICK;

        clientPrediction = new ClientPrediction();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        while (timer >= tickLength)
        {
            timer -= tickLength;
            HandleTick();
            currentTick++;
        }
        
        /*Debug.Log(currentTick);*/
    }

    private void HandleTick()
    {
        uint bufferIndex = currentTick % Constants.BUFFER_SIZE;
        bool[] inputs = SetInputs();
        InputPayload inputPayload = new InputPayload();
        inputPayload.tick = currentTick;
        inputPayload.inputs = inputs;
        inputBuffer[bufferIndex] = inputPayload;
        stateBuffer[bufferIndex] = ProcessMovement(inputs);
        
        ClientSend.PlayerMovement(currentTick, inputs);
    }

    private StatePayload ProcessMovement(bool[] _inputs)
    {
        Vector3 movement = clientPrediction.HandleMovement(_inputs, moveSpeed);
        transform.position += movement;
        return new StatePayload()
        {
            tick = currentTick,
            position = transform.position
        };
    }

    private bool[] SetInputs()
    {
        
        bool[] _inputs = new bool[]
        {
            Input.GetKey(KeyCode.W),
            Input.GetKey(KeyCode.S),
            Input.GetKey(KeyCode.A),
            Input.GetKey(KeyCode.D),
        };
        
        //ClientSend.PlayerMovement(_inputs);
        return _inputs;
    }

    public void Reconcile(StatePayload _serverState)
    {
        uint serverBufferIndex = _serverState.tick % Constants.BUFFER_SIZE;
        StatePayload toReconcileState = stateBuffer[serverBufferIndex];
        
        float positionError = Vector3.Distance(_serverState.position, toReconcileState.position);
        if (positionError > 0.01)
        {
            Debug.Log(_serverState);
            Debug.Log(toReconcileState);
            transform.position = _serverState.position;
            uint tickToProcess = _serverState.tick + 1;

            while (tickToProcess < currentTick)
            {
                uint bufferIndex = tickToProcess % Constants.BUFFER_SIZE;
                StatePayload statePayload = ProcessMovement(inputBuffer[bufferIndex].inputs);
                stateBuffer[bufferIndex] = statePayload;

                tickToProcess++;
            }
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct InputPayload
{
    public uint tick;
    public bool[] inputs;
    
    public override string ToString()
    {
        return $"Tick: {tick}  Inputs: {inputs}";
    }
}
[Serializable]
public struct StatePayload
{
    public uint tick;
    public Vector3 position;
    public Quaternion rotation;

    public override string ToString()
    {
        return $"Tick: {tick}  Position: {position}";
    }
}

public class PlayerController : MonoBehaviour
{
    public uint currentTick;
    private float timer;
    private float tickLength;

    private Queue<StatePayload> serverStates;
    [SerializeField] private StatePayload[] stateBuffer;
    [SerializeField] private InputPayload[] inputBuffer;
    [SerializeField] private uint lastReceivedTick;
    [SerializeField] private uint nextTickToProcess;

    private float moveSpeed = 3f;

    private ClientPrediction clientPrediction;
    private CameraController cameraController;
    private PlayerManager playerManager;
    
    private void Awake()
    {
        tickLength = Constants.MS_PER_TICK;
        tickLength /= 1000;

        serverStates = new Queue<StatePayload>();
        stateBuffer = new StatePayload[Constants.BUFFER_SIZE];
        inputBuffer = new InputPayload[Constants.BUFFER_SIZE];

        moveSpeed /= Constants.MS_PER_TICK;
        
        clientPrediction = new ClientPrediction();
        cameraController = GetComponentInChildren<CameraController>();
        playerManager = GetComponent<PlayerManager>();
    }

    private void Update()
    {
        timer += Time.deltaTime;
        while (timer >= tickLength)
        {
            HandleTick();
            timer -= tickLength;
            currentTick++;
        }
    }

    private void HandleTick()
    {
        //Handle Reconciliation
        if (lastReceivedTick != nextTickToProcess)
        {
            if (serverStates.Count > 0)
            {
                StatePayload serverState = serverStates.Dequeue();
                playerManager.playerGhost.position = serverState.position;
                playerManager.playerGhost.rotation = serverState.rotation;
                if (serverState.tick > nextTickToProcess)
                {
                    Reconcile(serverState);
                }
            }
        }

        uint bufferIndex = currentTick % Constants.BUFFER_SIZE;
        bool[] inputs = SetInputs();
        InputPayload inputPayload = new InputPayload();
        inputPayload.tick = currentTick;
        inputPayload.inputs = inputs;
        inputBuffer[bufferIndex] = inputPayload;
        
        Quaternion newRotation = cameraController.Rotate();
        Vector3 newPosition = ProcessMovement(inputs, newRotation);

        ClientSend.PlayerMovement(currentTick, inputs, newRotation);
        
        transform.position = newPosition;
        transform.rotation = newRotation;
        
        stateBuffer[bufferIndex] = new StatePayload()
        {
            tick = currentTick,
            position = newPosition,
            rotation = newRotation,
        };
    }

    private Vector3 ProcessMovement(bool[] _inputs, Quaternion _rotation)
    {
        Vector3 movement = clientPrediction.HandleMovement(_inputs, moveSpeed, _rotation);
        Vector3 position = transform.position;
        position += movement;

        return position;
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

    public void ReceiveServerState(StatePayload _serverState)
    {
        serverStates.Enqueue(_serverState);
        lastReceivedTick = _serverState.tick;
    }
    
    //I need to reconcile in HandleTick?
    public void Reconcile(StatePayload _serverState)
    {
        uint serverBufferIndex = _serverState.tick % Constants.BUFFER_SIZE;
        StatePayload predictedState = stateBuffer[serverBufferIndex];
        
        //Debug.Log("Client Rot: " + predictedState.rotation + " Server Rot: " + _serverState.rotation);
        
        float positionError = Vector3.Distance(_serverState.position, predictedState.position);
        if (positionError > 0.01)
        {
            transform.position = _serverState.position;

            uint tickToProcess = _serverState.tick + 1;
            stateBuffer[serverBufferIndex] = _serverState;
            
            //Apply authoritative state
            while (tickToProcess < currentTick)
            {
                uint bufferIndex = tickToProcess % Constants.BUFFER_SIZE;
                Vector3 correctPosition = ProcessMovement(inputBuffer[bufferIndex].inputs, stateBuffer[bufferIndex].rotation);
                transform.position = correctPosition;
                
                StatePayload statePayload = new StatePayload()
                {
                    tick = tickToProcess,
                    position = correctPosition,
                    rotation = stateBuffer[bufferIndex].rotation,
                };
                
                stateBuffer[bufferIndex] = statePayload;

                tickToProcess++;
            }
        }
        
        //next tick
        nextTickToProcess = _serverState.tick + 1;
    }
}

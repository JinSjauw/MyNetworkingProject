using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
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
    public float timeStamp;
    public float roundTripTime;
    public Vector3 position;
    public Quaternion rotation;

    public override string ToString()
    {
        return $"Tick: {tick} TimeStamp: {timeStamp} Position: {position} Rotation: {rotation}";
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
    private bool isColliding = false;
    
    private ClientPrediction clientPrediction;
    private PlayerManager playerManager;
    [SerializeField] private List<Collider> colliderList;
    [SerializeField] private Camera camera;
    [SerializeField] private CameraFollow cameraFollow;
    
    private void Awake()
    {
        tickLength = Constants.MS_PER_TICK;
        tickLength /= 1000;

        serverStates = new Queue<StatePayload>();
        stateBuffer = new StatePayload[Constants.BUFFER_SIZE];
        inputBuffer = new InputPayload[Constants.BUFFER_SIZE];

        moveSpeed /= Constants.MS_PER_TICK;
        
        clientPrediction = new ClientPrediction();
        playerManager = GetComponent<PlayerManager>();
        colliderList = new List<Collider>();
    }

    private void Update()
    {
        /*bool[] inputs = SetInputs();
        Quaternion newRotation = transform.rotation;
        Vector3 newPosition = ProcessMovement(inputs, newRotation);

        transform.position = newPosition;*/
    }

    public void HandleTick()
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
        //Debug.Log(bufferIndex + " " + inputBuffer.Length);
        bool[] inputs = SetInputs();
        InputPayload inputPayload = new InputPayload();
        inputPayload.tick = currentTick;
        inputPayload.inputs = inputs;
        inputBuffer[bufferIndex] = inputPayload;

        LookAtMouse();
        
        Quaternion newRotation = transform.rotation;
        Vector3 newPosition = ProcessMovement(inputs, newRotation);

        transform.position = newPosition;

        ClientSend.PlayerMovement(currentTick, inputs, newRotation);

        stateBuffer[bufferIndex] = new StatePayload()
        {
            tick = currentTick,
            position = newPosition,
            rotation = newRotation,
        };
        
        cameraFollow.UpdateCamera();
        currentTick++;
    }

    private Quaternion ProccessRotation()
    {
        Vector2 direction = camera.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        Quaternion newRotation = Quaternion.AngleAxis(angle - 180, Vector3.up);

        return newRotation;
    }
    
    private void LookAtMouse()
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Ground")))
        {
            Vector3 target = hit.point;
            Vector3 direction = target - transform.position;
            float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, angle, 0);
        }
    }
    
    private Vector3 ProcessMovement(bool[] _inputs, Quaternion _rotation)
    {
        Vector3 movement = clientPrediction.HandleMovement(_inputs, moveSpeed, _rotation);
        Vector3 position = transform.position;

        if (!DetectCollision(position + movement))
        {
            position += movement;
        }
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
        
        return _inputs;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Added: " + other);
        colliderList.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Removed: " + other);
        colliderList.Remove(other);
    }

    private bool DetectCollision(Vector3 _position)
    {
        foreach (Collider _collider in colliderList)
        {
            Vector3 playerPosition = _position;
            Vector3 colliderPosition = _collider.transform.position;
            Vector3 colliderSize = _collider.bounds.size;
        
            if (playerPosition.x < colliderPosition.x + colliderSize.x + .5f &&
                playerPosition.x + .5f > colliderPosition.x &&
                playerPosition.z < colliderPosition.z + colliderSize.z + .5f &&
                playerPosition.z + .5f > colliderPosition.z)
            {
                Debug.Log("Collision!");
                return true;
            }
        }

        return false;
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

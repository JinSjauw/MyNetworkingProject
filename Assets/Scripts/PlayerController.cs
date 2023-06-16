using System;
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
    public float timeStamp;
    public float roundTripTime;
    public Vector3 position;
    public Quaternion rotation;
    public Vector2 inputDirection;

    public override string ToString()
    {
        return $"Tick: {tick} TimeStamp: {timeStamp} Position: {position} Rotation: {rotation}";
    }
}

public class PlayerController : MonoBehaviour
{
    public uint currentTick;

    private Queue<StatePayload> serverStates;
    private StatePayload[] stateBuffer;
    private InputPayload[] inputBuffer;
    private uint lastReceivedTick;
    private uint nextTickToProcess;
    
    private float moveSpeed = 5f;
    private bool hasFired;
    private Vector3 lookingDirection;
    
    private ClientPrediction clientPrediction;
    private PlayerManager playerManager;
    [SerializeField] private List<Collider> colliderList;
    [SerializeField] private Camera camera;
    [SerializeField] private CameraFollow cameraFollow;
    [SerializeField] private Transform projectileOrigin;
    
    private void Awake()
    {
        serverStates = new Queue<StatePayload>();
        stateBuffer = new StatePayload[Constants.BUFFER_SIZE];
        inputBuffer = new InputPayload[Constants.BUFFER_SIZE];

        //moveSpeed /= Constants.MS_PER_TICK;
        
        clientPrediction = new ClientPrediction();
        playerManager = GetComponent<PlayerManager>();
        colliderList = new List<Collider>();
        cameraFollow.StartCamera(transform, camera);
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0) && hasFired)
        {
            hasFired = false;
        }
    }

    public void ClearServerStates()
    {
        serverStates.Clear();
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
        bool[] inputs = GetMovementInputs();
        InputPayload inputPayload = new InputPayload();
        inputPayload.tick = currentTick;
        inputPayload.inputs = inputs;
        inputBuffer[bufferIndex] = inputPayload;

        LookAtMouse();
        ProcessMouseInput();
        
        Quaternion newRotation = transform.rotation;
        Vector3 newPosition = ProcessMovement(inputs, newRotation);

        transform.position = newPosition;
        //Debug.Log("Player Moving To: " + newPosition);
        
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

    private void LookAtMouse()
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Ground")))
        {
            Vector3 target = hit.point;
            lookingDirection = target - transform.position;
            float angle = Mathf.Atan2(lookingDirection.x, lookingDirection.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, angle, 0);
        }
    }
    
    private Vector3 ProcessMovement(bool[] _inputs, Quaternion _rotation)
    {
        Vector2 inputDirection = clientPrediction.GetInputDirection(_inputs);
        Vector3 movement = clientPrediction.HandleMovement(moveSpeed, _rotation, inputDirection);
        Vector3 position = transform.position;

        if (!DetectCollision(position + movement))
        {
            position += movement;
        }
        return position;
    }

    private void ProcessMouseInput()
    {
        if (Input.GetMouseButton(0) && !hasFired)
        {
            hasFired = true;
            Shoot();
        }
    }
    
    private void Shoot()
    {
        Debug.Log("Shoot");
        //Send server a shoot packet
        ClientSend.PlayerShoot(currentTick, projectileOrigin.position, lookingDirection / 2, 20f);
        playerManager.IsShooting();
        //Spawn projectile effects on here;
        //Audio
        
        //If it collides with anything spawn blood
        //Only call Die() when server says so
        
        //server only updates projectile when it hits!
        //Fire shoot event
    }
    
    private bool[] GetMovementInputs()
    {
        bool[] _inputs = new bool[]
        {
            Input.GetKey(KeyCode.W),
            Input.GetKey(KeyCode.S),
            Input.GetKey(KeyCode.A),
            Input.GetKey(KeyCode.D),
        };

        int inputsPressed = 0;
        foreach (var input in _inputs)
        {
            if (input)
            {
                inputsPressed++;
            }
        }

        Vector2 inputDirection = clientPrediction.GetInputDirection(_inputs);
        
        if (inputsPressed > 0)
        {
            playerManager.IsRunning(true, inputDirection);
        }
        else
        {
            playerManager.IsRunning(false, inputDirection);
        }

        return _inputs;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ground"))
        {
            Debug.Log("Added: " + other);
            colliderList.Add(other);   
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Ground") && colliderList.Contains(other))
        {
            Debug.Log("Removed: " + other);
            colliderList.Remove(other);   
        }
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
                Debug.Log("Collision! " + _collider.name);
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
    private void Reconcile(StatePayload _serverState)
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

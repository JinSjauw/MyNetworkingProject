using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string username;
    public Transform playerGhost;
    /*public PlayerController playerController;
    public RemoteEntity remotePlayer;*/
    public Action HandleTick;
    public Action<StatePayload> ReceiveServerState;
    
    private uint nextTickToProcess;
    private uint lastReceivedTick;
    private float interpTime;

    public void Tick()
    {
        HandleTick();
    }
}

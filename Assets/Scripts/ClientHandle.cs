using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet _packet)
    {
        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();

        Debug.Log($"Message from server: {_msg}");
        Client.Instance.myId = _myId;
        ClientSend.WelcomeReceived();
        Client.Instance.udp.Connect(((IPEndPoint)Client.Instance.tcp.socket.Client.LocalEndPoint).Port);
        ClientSend.RequestTime();
    }

    public static void UDPTest(Packet _packet)
    {
        string _msg = _packet.ReadString();
        
        Debug.Log($"Received packet via UDP. MESSAGE: {_msg}");
        ClientSend.UDPTestReceived();
    }
    
    public static void ReceiveTime(Packet _packet)
    {
        float serverTime = _packet.ReadFloat();
        
        GameManager.stopWatch.Stop();
        float rtt = GameManager.stopWatch.ElapsedMilliseconds;
        GameManager.clientTimer = serverTime + (rtt / 1000f);
        
        Debug.Log($"Received Server Time: {serverTime} RTT: {rtt}");
    }

    public static void SpawnPlayer(Packet _packet)
    {
        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();
        
        Debug.Log($"Player ID {_id} Name: {_username} ");

        GameManager.Instance.SpawnPlayer(_id, _username, _position, _rotation);
    }

    public static void PlayerPosition(Packet _packet)
    {
        int _id = _packet.ReadInt();
        uint _packetTick = _packet.ReadUint();
        uint _serverTick = _packet.ReadUint();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();
        
        //Debug.Log($"PacketID: {_packetTick}");

        StatePayload receivedState = new StatePayload
        {
            tick = _packetTick,
            serverTick = _serverTick,
            position = _position,
            rotation = _rotation,
        };
       
        
        if (GameManager.players.ContainsKey(_id))
        {
            if (Client.Instance.myId == _id)
            {
                GameManager.players[_id].ReceiveServerState(receivedState);
            }
            else
            {
                GameManager.players[_id].ReceiveServerState(receivedState);
            }
        }
    }
    
    //Not in use
    public static void PlayerRotation(Packet _packet)
    {
        int _id = _packet.ReadInt();
        int _packetTick = _packet.ReadInt();
        Quaternion _rotation = _packet.ReadQuaternion();

        if (GameManager.players.ContainsKey(_id))
        {
            GameManager.players[_id].transform.rotation = _rotation;
            GameManager.players[_id].playerGhost.rotation = _rotation;
        }
    }
}

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
        Client.instance.myId = _myId;
        ClientSend.WelcomeReceived();
        
        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void UDPTest(Packet _packet)
    {
        string _msg = _packet.ReadString();
        
        Debug.Log($"Received packet via UDP. MESSAGE: {_msg}");
        ClientSend.UDPTestReceived();
    }

    public static void SpawnPlayer(Packet _packet)
    {
        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();
        
        Debug.Log($"Player ID {_id} Name: {_username} ");

        GameManager.instance.SpawnPlayer(_id, _username, _position, _rotation);
    }

    public static void PlayerPosition(Packet _packet)
    {
        int _id = _packet.ReadInt();
        uint _packetTick = _packet.ReadUint();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();
        
        //Debug.Log($"PacketID: {_packetTick}");
        
        if (GameManager.players.ContainsKey(_id))
        {
            if (Client.instance.myId == _id)
            {
                GameManager.players[_id].playerController.ReceiveServerState( 
                    new StatePayload()
                    {
                        tick = _packetTick,
                        position = _position,
                        rotation = _rotation,
                    });
                /*GameManager.players[_id].playerGhost.position = _position;
                GameManager.players[_id].playerGhost.rotation = _rotation;*/
            }
            else
            {
                GameManager.players[_id].transform.position = _position;
                GameManager.players[_id].transform.rotation = _rotation;
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

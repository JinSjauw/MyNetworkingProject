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
        GameManager.clientTimer = serverTime + (rtt / 1000f) / 2;
        
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
        float _timeSent = _packet.ReadFloat();
        float _packetTimestamp = _packet.ReadFloat();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();
        Vector2 _inputDirection = _packet.ReadVector2();
        
        //Debug.Log($"PacketID: {_packetTick}");
        //Calculate RTT with timeSent

        float rtt = GameManager.clientTimer - _timeSent;
        //Debug.Log($"Packet ID: {_packetTick} | RTT: {rtt}");
        
        StatePayload receivedState = new StatePayload
        {
            tick = _packetTick,
            timeStamp = _packetTimestamp,
            roundTripTime = rtt,
            position = _position,
            rotation = _rotation,
            inputDirection = _inputDirection,
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
    
    public static void SpawnProjectile(Packet _packet)
    {
        int _id = _packet.ReadInt();
        float _packetTimestamp = _packet.ReadFloat();
        int _projectileID = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();
        Vector3 _direction = _packet.ReadVector3();
        float _velocity = _packet.ReadFloat();
        
        if (GameManager.players.ContainsKey(_id))
        {
            if (_id == Client.Instance.myId)
            {
                GameManager.players[_id].IsShooting();
            }
        }
        
        //Create a new Projectile
        //Object pool that spawns projectile
        GameManager.Instance.SpawnProjectile(_projectileID, _position, _direction, _velocity);
    }

    public static void UpdateProjectile(Packet _packet)
    {
        int _id = _packet.ReadInt();
        float _packetTimestamp = _packet.ReadFloat();
        int _projectileID = _packet.ReadInt();
        int _hitClientID = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        if (GameManager.projectilesList.ContainsKey(_projectileID))
        {
            GameManager.projectilesList[_projectileID].UpdateProjectile(_position);
        }
    }

    public static void PlayerDamage(Packet _packet)
    {
        int _hitID = _packet.ReadInt();
        float _packetTimeStamp = _packet.ReadFloat();
        int _damage = _packet.ReadInt();

        if (GameManager.players.ContainsKey(_hitID))
        {
            GameManager.players[_hitID].TakeDamage(_damage);
        }
    }

    public static void PlayerDie(Packet _packet)
    {
        int _clientID = _packet.ReadInt();
        float _packetTimeStamp = _packet.ReadFloat();

        if (GameManager.players.ContainsKey(_clientID))
        {
            GameManager.players[_clientID].Die();
        }
    }

    public static void PlayerRespawn(Packet _packet)
    {
        int _clientID = _packet.ReadInt();
        float _packetTimeStamp = _packet.ReadFloat();
        Vector2 _position = _packet.ReadVector2();
        int _hp = _packet.ReadInt();
        
        if (GameManager.players.ContainsKey(_clientID))
        {
            GameManager.players[_clientID].Respawn(_position, _hp);
        }
    }

    public static void PlayerScoreKill(Packet _packet)
    {
        int _clientID = _packet.ReadInt();

        if (GameManager.players.ContainsKey(_clientID))
        {
            GameManager.players[_clientID].AddKill();
        }
    }
}

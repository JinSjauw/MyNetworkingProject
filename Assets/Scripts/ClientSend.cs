using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    private static void SendTCPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.Instance.tcp.SendData(_packet);
    }

    private static void SendUDPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.Instance.udp.SendData(_packet);
    }
    
    #region Packets
    public static void WelcomeReceived()
    {
        using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            _packet.Write(Client.Instance.myId);
            _packet.Write(UIManager.instance.usernameField.text);
            SendTCPData(_packet);
        }
    }

    public static void UDPTestReceived()
    {
        using (Packet _packet = new Packet((int)ClientPackets.udpTestReceive))
        {
            _packet.Write("Received UDP Packet");
            SendUDPData(_packet);
        }
    }
    
    public static void RequestTime()
    {
        GameManager.stopWatch.Restart();
        using (Packet _packet = new Packet((int)ClientPackets.timeRequest))
        {
            SendUDPData(_packet);
        }
    }
    

    public static void PlayerMovement(uint _tick, bool[] _inputs, Quaternion _rotation)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerMovement))
        {
            _packet.Write(_tick);
            _packet.Write(GameManager.clientTimer);
            _packet.Write(_inputs.Length);
            foreach (bool _input in _inputs)
            {
                _packet.Write(_input);
            }
            _packet.Write(_rotation);
            SendUDPData(_packet);
        }
    }

    public static void PlayerShoot(uint _tick, Vector3 _position, Vector3 _direction, float _velocity)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerShoot))
        {
            _packet.Write(_tick);
            _packet.Write(GameManager.clientTimer);
            _packet.Write(_position);
            _packet.Write(_direction);
            _packet.Write(_velocity);

            SendUDPData(_packet);
        }
    }

    #endregion
}

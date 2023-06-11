using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

    public static Stopwatch stopWatch = new Stopwatch();
    public static float clientTimer = 0;
    
    public GameObject localPlayerPrefab;
    public GameObject playerGhostPrefab;
    public GameObject playerPrefab;
    public uint currentTick;

    private float timeRequestInterval = 5f;
    private float timerA, timerB;
    private float tickLength;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if(Instance != this)
        {
            Debug.Log("Instance Already exist, destroying object");
            Destroy(this);
        }

        tickLength = Constants.MS_PER_TICK;
        tickLength /= 1000;
        Application.runInBackground = true;
    }

    private void Update()
    {
        if (players.Count <= 0)
        {
            return;
        }
        
        timerA += Time.deltaTime;
        
        if (clientTimer - timerB > timeRequestInterval)
        {
            timerB = clientTimer;
            ClientSend.RequestTime();
        }
        
        while (timerA >= tickLength)
        {
            foreach (var player in players)
            {
                player.Value.Tick();
            }
            timerA -= tickLength;
            clientTimer += Constants.MS_PER_TICK / 1000f;
            currentTick++;
        }
    }

    public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation)
    {
        GameObject _player;
        GameObject _playerGhost;
        if (_id == Client.Instance.myId)
        {
            _player = Instantiate(localPlayerPrefab, _position, _rotation);
            _playerGhost = Instantiate(playerGhostPrefab, _position, _rotation);
            PlayerManager playerManager = _player.GetComponent<PlayerManager>();
            playerManager.HandleTick = _player.GetComponent<PlayerController>().HandleTick;
            playerManager.ReceiveServerState = _player.GetComponent<PlayerController>().ReceiveServerState;
            _player.GetComponent<PlayerManager>().playerGhost = _playerGhost.transform;
        }
        else
        {
            _player = Instantiate(playerPrefab, _position, _rotation);
            PlayerManager playerManager = _player.GetComponent<PlayerManager>();
            playerManager.HandleTick = _player.GetComponent<RemoteEntity>().HandleTick;
            playerManager.ReceiveServerState = _player.GetComponent<RemoteEntity>().ReceiveServerState;
        }

        _player.GetComponent<PlayerManager>().id = _id;
        _player.GetComponent<PlayerManager>().username = _username;

        players.Add(_id, _player.GetComponent<PlayerManager>());
        
        Debug.Log($"Username: {Client.Instance.myId} || Added: {_username} : {_id}");
    }
}

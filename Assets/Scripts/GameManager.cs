using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public static Dictionary<int, PlayerManager> players;
    public static Dictionary<int, Projectile> projectilesList;

    public static Stopwatch stopWatch = new Stopwatch();
    public static float clientTimer = 0;
    
    public GameObject localPlayerPrefab;
    public GameObject playerGhostPrefab;
    public GameObject playerPrefab;
    public GameObject projectilePrefab;
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
        players = new Dictionary<int, PlayerManager>();
        projectilesList = new Dictionary<int, Projectile>();
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

            foreach (Projectile projectile in projectilesList.Values)
            {
                projectile.HandleTick();
            }
            
            timerA -= tickLength;
            clientTimer += Constants.MS_PER_TICK / 1000f;
            currentTick++;
        }
    }

    public void SpawnProjectile(int _projectileID, Vector3 _position, Vector3 _direction, float _velocity)
    {
        Projectile spawnedProjectile = Instantiate(projectilePrefab).GetComponent<Projectile>();
        spawnedProjectile.Init(_projectileID, _position, _direction, _velocity);
        projectilesList[_projectileID] = spawnedProjectile;
    }
    
    public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation)
    {
        GameObject _player;
        GameObject _playerGhost;
        if (_id == Client.Instance.myId)
        {
            _player = Instantiate(localPlayerPrefab, _position, _rotation);
            _playerGhost = Instantiate(playerGhostPrefab, _position, _rotation);
            PlayerManager playerManager = _player.GetComponentInChildren<PlayerManager>();
            playerManager.HandleTick = _player.GetComponentInChildren<PlayerController>().HandleTick;
            playerManager.ReceiveServerState = _player.GetComponentInChildren<PlayerController>().ReceiveServerState;
            _player.GetComponentInChildren<PlayerManager>().playerGhost = _playerGhost.transform;
        }
        else
        {
            _player = Instantiate(playerPrefab, _position, _rotation);
            PlayerManager playerManager = _player.GetComponent<PlayerManager>();
            playerManager.HandleTick = _player.GetComponent<RemoteEntity>().HandleTick;
            playerManager.ReceiveServerState = _player.GetComponent<RemoteEntity>().ReceiveServerState;
        }

        _player.GetComponentInChildren<PlayerManager>().id = _id;
        _player.GetComponentInChildren<PlayerManager>().username = _username;

        players.Add(_id, _player.GetComponentInChildren<PlayerManager>());
        
        Debug.Log($"Username: {Client.Instance.myId} || Added: {_username} : {_id}");
    }
}

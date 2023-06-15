using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string username;
    public Transform playerGhost;
    public Action HandleTick;
    public Action<StatePayload> ReceiveServerState;
    
    public int kills;
    public int deaths;
    
    private uint nextTickToProcess;
    private uint lastReceivedTick;
    private float interpTime;
    private Animator animator;
    
    [SerializeField] private int HP = 100;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void Tick()
    {
        HandleTick();
    }

    public void IsRunning(bool _state)
    {
        animator.SetBool("IsRunning", _state);
    }
    
    public void TakeDamage(int _damage)
    {
        HP -= _damage;
        Debug.Log($"Player: {id} took {_damage} damage");
    }
    
    public void Die()
    {
        Debug.Log("I AM DEAD: " + id);
        deaths++;
        //gameObject.SetActive(false);
        //Spawn Ragdoll
        //Sent Respawn Request To Server
    }

    public void Respawn(Vector2 _respawnPosition)
    {
        PlayerController player = GetComponent<PlayerController>();
        if (player != null)
        {
            player.ClearServerStates();
        }
        Debug.Log($"Respawning! {_respawnPosition}");
        transform.position = new Vector3(_respawnPosition.x, 0 , _respawnPosition.y);
    }
}

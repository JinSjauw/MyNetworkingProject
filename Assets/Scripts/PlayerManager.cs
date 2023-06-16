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
    private RagDollSpawner ragdollSpawner;
    
    [SerializeField] private int HP = 100;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        ragdollSpawner = GetComponent<RagDollSpawner>();
    }

    public void Tick()
    {
        HandleTick();
    }

    public void IsRunning(bool _state, Vector2 inputDirection)
    {
        animator.SetBool("IsRunning", _state);
        animator.SetFloat("VelocityX", inputDirection.x);
        animator.SetFloat("VelocityZ", inputDirection.y);
    }

    public void IsShooting()
    {
        animator.SetTrigger("hasShot");
    }
    
    public void TakeDamage(int _damage)
    {
        HP -= _damage;
        Debug.Log($"Player: {id} took {_damage} damage");
        //Flash red?
    }
    
    public void Die()
    {
        Debug.Log("I AM DEAD: " + id);
        deaths++;
        ragdollSpawner.Spawn();
        //gameObject.SetActive(false);
        //Spawn Ragdoll
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

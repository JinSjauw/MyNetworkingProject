using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string username;
    public Transform playerGhost;
    public Action HandleTick;
    public Action<StatePayload> ReceiveServerState;
    
    public int kills = 1;
    public int deaths = 1;
    
    private uint nextTickToProcess;
    private uint lastReceivedTick;
    private float interpTime;
    
    private Animator animator;
    private RagDollSpawner ragdollSpawner;
    private AudioSource muzzleSource;
    
    private int HP = 100;

    [SerializeField] private TextMeshProUGUI usernameText;
    [SerializeField] private TextMeshProUGUI killsText;
    [SerializeField] private TextMeshProUGUI deathsText;
    [SerializeField] private TextMeshProUGUI hpText;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        ragdollSpawner = GetComponent<RagDollSpawner>();
        muzzleSource = GetComponent<AudioSource>();

        hpText.text = HP.ToString();
    }

    private void Start()
    {
        if (usernameText != null)
        {
            usernameText.text = username;
        }
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
        muzzleSource.Play();
    }
    
    public void TakeDamage(int _damage)
    {
        HP -= _damage;
        Debug.Log($"Player: {id} took {_damage} damage");
        //Flash red?
        if (hpText != null)
        {
            hpText.text = HP.ToString();
        }
    }
    
    public void AddKill()
    {
        if (killsText != null)
        {
            killsText.text = "Kills: " + kills++;
        }
    }
    
    public void Die()
    {
        Debug.Log("I AM DEAD: " + id);

        if (deathsText != null)
        {
            deathsText.text = "Deaths: " + deaths++;
        }
        
        gameObject.SetActive(false);
        ragdollSpawner.Spawn();
        //gameObject.SetActive(false);
        //Spawn Ragdoll
    }

    public void Respawn(Vector2 _respawnPosition, int _hp)
    {
        PlayerController player = GetComponent<PlayerController>();
        if (player != null)
        {
            player.ClearServerStates();
        }
        HP = _hp;

        if (hpText != null)
        {
            hpText.text = _hp.ToString();
        }
        
        Debug.Log($"Respawning! {_respawnPosition}");
        transform.position = new Vector3(_respawnPosition.x, 0 , _respawnPosition.y);
        gameObject.SetActive(true);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagDollSpawner : MonoBehaviour
{
    [SerializeField] private Transform ragdollPrefab;
    [SerializeField] private Transform originalRootBone;

    //private HealthSystem healthSystem;

    private void Awake()
    {
        //healthSystem = GetComponent<HealthSystem>();

        //healthSystem.OnDeath += HealthSystem_OnDeath;
    }

    public void Spawn()
    {
        Transform ragdollTransform = Instantiate(ragdollPrefab, transform.position, transform.rotation);
        RagDollObject unitRagdoll = ragdollTransform.GetComponent<RagDollObject>();
        unitRagdoll.Init(originalRootBone);
    }
    
    /*private void HealthSystem_OnDeath(object _sender, EventArgs _e)
    {
        Transform ragdollTransform = Instantiate(ragdollPrefab, transform.position, transform.rotation);
        RagDollObject unitRagdoll = ragdollTransform.GetComponent<RagDollObject>();
        unitRagdoll.Init(originalRootBone);
    }*/
}

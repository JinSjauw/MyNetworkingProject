using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private int projectileID;
    private Vector3 position;
    private Vector3 direction;
    private float velocity;
    private bool hasHit = false;
    private int enviromentLayer = 7;
    private int playerLayer = 8;
    private int combinedLayers;

    [SerializeField] private Transform impactEffect;
    
    public void Init(int _projectileID, Vector3 _position, Vector3 _direction, float _velocity)
    {
        projectileID = _projectileID;
        position = _position;
        direction = _direction;
        velocity = _velocity;
        
        transform.position = position;
        combinedLayers = (1 << enviromentLayer) | (1 << playerLayer);
    }
    
    //public update/HandleTick function
    public void HandleTick()
    {
        if (hasHit)
        {
            return;
        }
        
        //Move projectile
        Vector3 newPosition = MoveProjectile(direction, velocity);
        
        //Since the direction is the same we use a different collision detection than on the server
        transform.position += newPosition;
    }

    public Vector3 MoveProjectile(Vector3 _direction, float _velocity)
    {
        System.Numerics.Vector2 direction = new System.Numerics.Vector2(_direction.x, _direction.z);
        
        System.Numerics.Vector3 _moveDirection = new System.Numerics.Vector3(direction.X, 0, direction.Y);
        System.Numerics.Vector3 newPosition = _moveDirection * _velocity * Constants.MS_PER_SECOND;

        return new Vector3(newPosition.X, newPosition.Y, newPosition.Z);
    }

    public void UpdateProjectile(Vector3 _position)
    {
        //Raycast forward
        //return first hit. This is the impact point
        Ray ray = new Ray(_position, direction);
        if (Physics.Raycast(ray, out RaycastHit hit, velocity, combinedLayers))
        {
            hasHit = true;
            Debug.Log("hit: " + hit.collider.name);
            transform.position = hit.point;
            Transform impact = Instantiate(impactEffect, hit.point, Quaternion.identity);
            impact.forward = hit.normal;
        }
    }
}

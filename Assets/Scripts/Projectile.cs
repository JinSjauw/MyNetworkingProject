using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int projectileID;
    public Vector3 position;
    public Vector3 direction;
    public float velocity;
    private ClientPrediction _clientPrediction = new ClientPrediction();
    
    public void Init(int _projectileID, Vector3 _position, Vector3 _direction, float _velocity)
    {
        projectileID = _projectileID;
        position = _position;
        direction = _direction;
        velocity = _velocity;
        
        transform.position = position;
    }
    
    //public update/HandleTick function
    public void UpdateProjectile()
    {
        //Move projectile
        Vector3 newPosition = _clientPrediction.HandleProjectile(direction, velocity);
        
        //Since the direction is the same we use a different collision detection than on the server
        //

        transform.position += newPosition;
        
        
    }
}

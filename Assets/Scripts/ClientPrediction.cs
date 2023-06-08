using System.Collections;
using System.Collections.Generic;
using System.Numerics;

public class ClientPrediction
{
    public Vector3 movement;
    public Quaternion rotation;

    public ClientPrediction()
    {
        rotation = Quaternion.Identity;
    }
    
    public UnityEngine.Vector3 HandleMovement(bool[] _inputs, float _moveSpeed, UnityEngine.Quaternion _rotation)
    {
        Vector2 inputDirection = Vector2.Zero;
         
        if (_inputs[0])
        {
            inputDirection.Y += 1;
        }
        if (_inputs[1])
        {
            inputDirection.Y -= 1;
        }
        if (_inputs[2])
        {
            inputDirection.X += 1;
        }
        if (_inputs[3])
        {
            inputDirection.X -= 1;
        }

        rotation = new Quaternion(_rotation.x, _rotation.y, _rotation.z, _rotation.w);
        Vector3 _forward = Vector3.Transform(new Vector3(0, 0, 1), rotation);
        Vector3 _right = Vector3.Normalize(Vector3.Cross(_forward, new Vector3(0, 1, 0)));

        Vector3 _moveDirection = _right * inputDirection.X + _forward * inputDirection.Y;
        movement = _moveDirection * _moveSpeed;

        return new UnityEngine.Vector3(movement.X, movement.Y, movement.Z);
    }
}

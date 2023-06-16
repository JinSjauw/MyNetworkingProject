using System.Numerics;

public class ClientPrediction
{
    public Vector3 position;
    public Quaternion rotation;

    public ClientPrediction()
    {
        rotation = Quaternion.Identity;
    }
    
    public UnityEngine.Vector3 HandleMovement(float _moveSpeed, UnityEngine.Quaternion _rotation, UnityEngine.Vector2 _inputDirection)
    {
        Vector2 inputDirection = new Vector2(_inputDirection.x, _inputDirection.y);

        rotation = new Quaternion(_rotation.x, _rotation.y, _rotation.z, _rotation.w);
        Vector3 _forward = Vector3.Transform(new Vector3(0, 0, 1), rotation);
        Vector3 _right = Vector3.Normalize(Vector3.Cross(_forward, new Vector3(0, 1, 0)));

        Vector3 _moveDirection = _right * inputDirection.X + _forward * inputDirection.Y;
        position = _moveDirection * _moveSpeed * Constants.MS_PER_SECOND;

        return new UnityEngine.Vector3(position.X, position.Y, position.Z);
    }

    public UnityEngine.Vector2 GetInputDirection(bool[] _inputs)
    {
        UnityEngine.Vector2 inputDirection = UnityEngine.Vector2.zero;
         
        if (_inputs[0])
        {
            inputDirection.y += 1;
        }
        if (_inputs[1])
        {
            inputDirection.y -= 1;
        }
        if (_inputs[2])
        {
            inputDirection.x += 1;
        }
        if (_inputs[3])
        {
            inputDirection.x -= 1;
        }

        return inputDirection;
    }
    
    
    public UnityEngine.Vector3 HandleProjectile(UnityEngine.Vector3 _direction, float _velocity)
    {
        Vector2 direction = new Vector2(_direction.x, _direction.z);
        
        Vector3 _moveDirection = new Vector3(direction.X, 0, direction.Y);
        position = _moveDirection * _velocity * Constants.MS_PER_SECOND;

        return new UnityEngine.Vector3(position.X, position.Y, position.Z);
    }
    
}

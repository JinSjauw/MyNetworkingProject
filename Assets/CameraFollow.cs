using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform player;
    private Camera playerCamera;
    [SerializeField] private Transform pointPrefab, point2Prefab;

    private Vector2 mousePosition;
    private Vector3 target, refVel;
    private float yStart;
    [SerializeField] private float smoothTime = 0.2f;
    [SerializeField] private float distanceThreshold;
    
    // Start is called before the first frame update
    
    public void StartCamera(Transform _player, Camera _playerCamera)
    {
        player = _player;
        playerCamera = _playerCamera;
        target = player.position;
        yStart = transform.position.y;
    }

    // Update is called once per frame
    public void UpdateCamera()
    {
        mousePosition = GetMousePosition();
        target = UpdateTarget();
        UpdateCameraPosition();
    }

    Vector2 GetMousePosition()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, LayerMask.GetMask("Ground")))
        {
            pointPrefab.position = hit.point;
            return new Vector2(hit.point.x, hit.point.z);
        }
        
        return Vector2.zero;
    }

    Vector3 UpdateTarget()
    {
        Vector3 playerPosition = player.position;
        Vector3 result = (new Vector3(mousePosition.x, 0, mousePosition.y) - playerPosition);
        result.y = yStart;
        
        result = Vector3.ClampMagnitude(result, distanceThreshold);
        result *= 0.8f;
        
        point2Prefab.position = playerPosition + result;
        
        return playerPosition + result;
    }

    private void UpdateCameraPosition()
    {
        Vector3 tempPosition = Vector3.SmoothDamp(transform.position, target, ref refVel, smoothTime);
        transform.position = tempPosition;
    }
}

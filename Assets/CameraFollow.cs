using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform pointPrefab, point2Prefab;

    private Vector2 mousePosition;
    private Vector3 target, refVel;
    private float yStart;
    [SerializeField] private float smoothTime = 0.2f;
    
    // Start is called before the first frame update
    void Start()
    {
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
        Vector3 result = (new Vector3(mousePosition.x, 0, mousePosition.y) + player.position) / 2f;

        result.y = yStart;

        point2Prefab.position = result;
        
        return result;
    }

    private void UpdateCameraPosition()
    {
        Vector3 tempPosition = Vector3.SmoothDamp(transform.position, target, ref refVel, smoothTime);
        transform.position = tempPosition;
    }
}

using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Target for the camera to follow, most likely the player
    public Transform targetTransform;
    
    [Header("Camera Positions")] 
    public float distance = -10f;
    public float height = 3f;
    // Amount of delay before the camera follows the player
    public float damping = 5f;

    private void Update()
    {
        // Get the player position and smoothly transition the camera
        var targetPosition = targetTransform.TransformPoint(0, height, distance);
        transform.position = Vector3.Lerp(transform.position, targetPosition, (Time.deltaTime * damping));
    }
}
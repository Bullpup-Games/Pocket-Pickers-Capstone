using System;
using UnityEngine;

namespace _Scripts
{
    public class CameraController : MonoBehaviour
    {
        private Camera _cam;
        // Target for the camera to follow, most likely the player
        public Transform targetTransform;
    
        [Header("Camera Positions")] 
        public float distance = -10f;
        public float height = 3f;
        // Amount of delay before the camera follows the player
        public float damping = 5f;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
        }

        private void Update()
        {
            // Get the player position and smoothly transition the camera
            var targetPosition = targetTransform.TransformPoint(0, height, distance);
            transform.position = Vector3.Lerp(transform.position, targetPosition, (Time.deltaTime * damping));
        }
        
        // Returns the current bounds of the camera
        public Bounds OrthographicBounds()
        {
            var screenAspect = (float)Screen.width / (float)Screen.height;
            var cameraHeight = _cam.orthographicSize * 2;
            var bounds = new Bounds(
                _cam.transform.position,
                new Vector3(cameraHeight * screenAspect, cameraHeight, 0));
            return bounds;
        }
    }
}
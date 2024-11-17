using System.Collections;
using UnityEngine;

namespace _Scripts.Camera
{
    public class CameraController : MonoBehaviour
    {
        // ROOM 1: 0, 0
        // ROOM 2: 39.5, 0

        public float lerpSpeed;
        private UnityEngine.Camera _cam;
        
        private int _currentRoom;
        private Vector3 _currentAnchorPoint;

        public int GetCurrentRoom() => _currentRoom;
        
        #region Singleton

        public static CameraController Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType(typeof(CameraController)) as CameraController;

                return _instance;
            }
            set { _instance = value; }
        }

        private static CameraController _instance;

        #endregion
        private void Awake()
        {
            _cam = GetComponent<UnityEngine.Camera>();
            _currentRoom = 1;
            _currentAnchorPoint = new Vector3(0f, 0f, -10f);
        }

        public void SwitchRooms(Vector3 anchorPoint, int roomNumber)
        {
            if (roomNumber == _currentRoom) return;

            _currentRoom = roomNumber;
            _currentAnchorPoint = anchorPoint;
            StartCoroutine(SmoothSwitch(anchorPoint));

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

        private IEnumerator SmoothSwitch(Vector3 targetAnchorPoint)
        {
            var elapsedTime = 0f;
            var startingPos = _cam.transform.position;
            var duration = 1f / lerpSpeed;

            while (elapsedTime < duration)
            {
                _cam.transform.position = Vector3.Lerp(startingPos, targetAnchorPoint, (elapsedTime / duration));
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            _cam.transform.position = targetAnchorPoint;
        }
    }
}
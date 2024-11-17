using UnityEngine;

namespace _Scripts.Camera
{
    public class CameraRoomHandler : MonoBehaviour
    {
        public int roomNumber;
        public Vector3 anchorPoint;

        public LayerMask playerLayer;

        private void OnTriggerEnter2D(Collider2D col)
        {
            if ((playerLayer.value & (1 << col.gameObject.layer)) == 0) return;
            Debug.Log("Player Entered Room# " + roomNumber);
            if (CameraController.Instance.GetCurrentRoom() == roomNumber) return;
            
            CameraController.Instance.SwitchRooms(anchorPoint, roomNumber);
        }
    }
}
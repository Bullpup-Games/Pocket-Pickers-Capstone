using UnityEngine;

namespace _Scripts.Camera
{
    public class CameraRoomHandler : MonoBehaviour
    {
        public int roomNumber;
        public Vector3 anchorPoint;

        public LayerMask playerLayer;

        private void OnTriggerStay2D(Collider2D col)
        {
            if ((playerLayer.value & (1 << col.gameObject.layer)) == 0) return;
            if (CameraController.Instance.GetCurrentRoom() == roomNumber) return;
            
            CameraController.Instance.SwitchRooms(anchorPoint, roomNumber);
        }
    }
}
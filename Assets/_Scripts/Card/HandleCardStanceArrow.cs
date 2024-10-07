using UnityEngine;

namespace _Scripts.Card
{
    /**
     * HandleCardStanceArrow is responsible for displaying and updating the directional indicator
     * whenever the player is in 'Card Stance'
     */
    public class HandleCardStanceArrow : MonoBehaviour
    {
        public Transform player;
        public GameObject directionalArrowPrefab;  // Arrow prefab to instantiate
        private GameObject _directionalArrowInstance;  // The instantiated arrow in the scene
    
        public Vector2 currentDirection;  // Stores the current direction of the arrow

        #region Singleton

        public static HandleCardStanceArrow Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType(typeof(HandleCardStanceArrow)) as HandleCardStanceArrow;

                return _instance;
            }
            set
            {
                _instance = value;
            }
        }
        private static HandleCardStanceArrow _instance;
        #endregion

        public void InstantiateDirectionalArrow()
        {
            // Instantiate the arrow as a child of the player
            _directionalArrowInstance = Instantiate(
                directionalArrowPrefab,
                player.position,
                Quaternion.identity,
                player
            );
            
            //TODO: Fix the horizontal starting position
            // Calculate the starting rotation angle based on the direction
            // var angle = (PlayerVariables.Instance.isFacingRight) ? 90f : -90f; // Left: 90°, Right: -90°
            // Debug.Log(angle);
            // _directionalArrowInstance.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        private void Update()
        {
            if (_directionalArrowInstance == null) return;
            UpdateArrow();
        }

        private void UpdateArrow()
        {
            // Get the joystick input (currently left joystick)
            // TODO: Add mouse support
            var horizontal = Input.GetAxis("Horizontal");
            var vertical = Input.GetAxis("Vertical");

            var inputDirection = new Vector2(horizontal, vertical);

            if (inputDirection.sqrMagnitude > 0.01f) // Dead zone to avoid noise
            {
                currentDirection = inputDirection.normalized;
            }

            // Calculate the angle in radians
            var angleRad = Mathf.Atan2(currentDirection.y, currentDirection.x);

            // Calculate the arrow's position relative to the player
            var offset = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad) + CardManager.Instance.verticalOffset, 0) * CardManager.Instance.horizontalOffset;
            var arrowPosition = player.position + offset;
            _directionalArrowInstance.transform.position = arrowPosition;

            // Rotate the arrow to point in the direction of the joystick
            var angleDeg = angleRad * Mathf.Rad2Deg;
            _directionalArrowInstance.transform.rotation = Quaternion.Euler(0, 0, angleDeg - 90f);
        }

        // Destroys the directional arrow when the player leaves card stance or throws a card TODO: link up card throwing
        public void DestroyDirectionalArrow()
        {
            if (_directionalArrowInstance == null) return;
            Destroy(_directionalArrowInstance);
            currentDirection = new Vector2();
            _directionalArrowInstance = null;
        }
    }
}
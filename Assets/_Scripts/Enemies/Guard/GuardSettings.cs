using System;
using _Scripts.Enemies.ViewTypes;
using UnityEngine;

namespace _Scripts.Enemies.Guard
{
    public class EnemySettings : MonoBehaviour
    {
        [Header("General Settings")]
        public bool isFacingRight = true;
        public float movementSpeed = 4f;
        

        [Header("Patrol Settings")]
        public float leftPatrolDistance = 3f;
        public float rightPatrolDistance = 3f;
        public float waitTimeAtEnds = 2f;
        [HideInInspector] public float patrolOrigin;

        [Header("Detection Settings")]
        public float baseDetectionTime = 4.0f;
        public float quickDetectionTime = 0.25f;
        
        [Header("Aggro Settings")]
        public float aggroMovementSpeed = 10f;
        public float checkLastKnownLocationTimeout = 5f;
        
        [Header("Quicktime Event Settings")]
        public float qteTimeLimit = 4f;
        public float timeLostPerEncounter = 0.5f;
        public int counterGoal = 15;
        
        #region SinModifiers
        private float _detectionModifier = 1.0f; // Speed modifier for detecting the player
        private float _viewModifier = 1.0f; // View width, radius, length, etc modifier for detecting the player
        public event Action<float> OnDetectionSpeedChanged;
        public event Action<float> OnViewModifierChanged;
        public float DetectionModifier
        {
            get => _detectionModifier;
            set
            {
                if (!(Math.Abs(_detectionModifier - value) > 0)) return;
                _detectionModifier = value;
                OnDetectionSpeedChanged?.Invoke(_detectionModifier); // Trigger event when detectionSpeed is modified
            }
        }
        public float ViewModifier
        {
            get => _viewModifier;
            set
            {
                if (!(Math.Abs(_viewModifier - value) > 0)) return;
                _viewModifier = value;
                OnViewModifierChanged?.Invoke(_viewModifier);  // Trigger event when viewModifier is modified
            }
        }
        #endregion

        [Header("Searching Settings")]
        public float totalSearchTime = 16f;
        public float searchIntervalTime = 4f;

        [Header("Stun Settings")]
        public float stunLength = 3.0f;

        [Header("Physics Settings")]
        public float maxFallSpeed = 20f;
        public float gravity = 10f;

        [Header("Ground Detection")]
        public float groundCheckDistance = 0.5f;
        public LayerMask groundLayer;
        public Color groundRayColor = Color.red;
        private bool _isGrounded;
        public bool IsGrounded() => _isGrounded;

        private Rigidbody2D _rb;
        private IViewType[] _viewTypes;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _viewTypes = GetComponents<IViewType>();
            patrolOrigin = transform.position.x;
        }

        private void Update()
        {
            isFacingRight = transform.localScale.x > 0;
            foreach (var view in _viewTypes)
            {
                view.SetView();
            }
        }

        public void HandleGroundDetection()
        {
            var hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
            _isGrounded = hit.collider != null;
        }

        public void HandleGravity()
        {
            if (_isGrounded) return;

            // Apply gravity to make the guard fall
            var newYVelocity = Mathf.MoveTowards(_rb.velocity.y, -maxFallSpeed, gravity * Time.deltaTime);
            _rb.velocity = new Vector2(_rb.velocity.x, newYVelocity);
        }

        public void FlipLocalScale()
        {
            var localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;
        }

        
        // Gizmos of the patrol distance and ground detection ray visible in the editor
        private void OnDrawGizmos()
        {
            Gizmos.color = groundRayColor;

            // Starting point of the ray
            var start = transform.position;

            // Ending point of the ray
            var end = start + Vector3.down * groundCheckDistance;

            // Draw the ground detection ray
            Gizmos.DrawLine(start, end);

            // Optionally, draw a sphere at the end point to make it more visible
            Gizmos.DrawSphere(end, 0.1f);
        }
    }
}
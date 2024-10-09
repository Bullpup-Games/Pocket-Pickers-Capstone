using System;
using UnityEngine;

namespace _Scripts.Enemies.ViewTypes
{
    public class ConeView : MonoBehaviour, IViewType
    {
        [Header("Detection Settings")]
        [Tooltip("If enabled cuts the detection time down to a quarter")]
        public bool quickDetection;
        [Tooltip("Upper bound for the modifier given to the detection timer when the player is at the far edge of the view"),
         Range(1.0f, 10.0f)]
        public float maxDistanceModifier = 3.0f;
        [Tooltip("Lower bound for the modifier given to the detection timer when the player is close to the origin of the view"),
         Range(0.01f, 1.0f)]
        public float minDistanceModifier = 0.25f;
        [Header("Cone View Settings")] 
        public float normalViewAngle = 45f;
        public float alertedViewAngle = 60f;
        public float normalViewDistance = 5f;
        public float alertedViewDistance = 10f;
        public LayerMask targetLayer;
        public LayerMask environmentLayer;
        public Vector2 offset; // (0.4, 1.0) for current enemy model

        private float _viewAngle;
        private float _viewDistance;
        private EnemySettings _settings;
        private EnemyStateManager _stateManager;
        private bool _playerDetectedThisFrame = false;

        public event Action<bool, float> PlayerDetected;
        public event Action NoPlayerDetected;
        private void Awake()
        {
            InitializeSettings(); 
        }

        private void Update()
        {
            if (_stateManager.state is EnemyState.Aggro or EnemyState.Searching)
            {
                // TODO: Make this available in the editor
                _viewAngle = alertedViewAngle;
                _viewDistance = alertedViewDistance;
            }
            else
            {
                _viewAngle = normalViewAngle;
                _viewDistance = normalViewDistance;
            }
        }

        private void OnEnable()
        {
            _settings.OnViewModifierChanged += UpdateView;
        }

        private void OnDisable()
        {
            _settings.OnViewModifierChanged -= UpdateView;
        }

        public void SetView()
        {
            UpdateHorizontalOffset();
            var position = (Vector2)transform.position + offset;
            var direction = _settings.isFacingRight ? Vector2.right : Vector2.left;

            // check for player collider within the view distance
            var targetsInViewRadius = Physics2D.OverlapCircleAll(position, _viewDistance, targetLayer);

            Vector3 directionToTarget;
            foreach (var target in targetsInViewRadius)
            {
                var targetPos = target.transform.position;
                directionToTarget = (targetPos - transform.position).normalized;
                var distanceToTarget = Vector2.Distance(position, targetPos);

                // Check if the target is within the adjusted view angle
                var angleBetween = Vector2.Angle(direction, directionToTarget);

                if (!(angleBetween < _viewAngle / 2)) continue;
                // TODO: When adding variable detection lengths based on distance get the distance from here
                // Check for obstacles between the enemy and the target
                var obstacleHit = Physics2D.Raycast((Vector2)transform.position, directionToTarget, _viewDistance, environmentLayer);

                if (obstacleHit.collider == null)
                {
                    // Target is detected
                    // Debug.Log("Target in Line of Sight " + target.gameObject);
                    // Debug.Log("cone hit");
                    var modifier = CalculateModifier(distanceToTarget);
                    OnTargetDetected(modifier);
                    return;
                }
            }
            
            // Debug.Log("Direction:" + directionToTarget);
            OnNoTargetDetected();
        }

        private void OnTargetDetected(float modifier)
        {
            _playerDetectedThisFrame = true;
            PlayerDetected?.Invoke(quickDetection, modifier);
        }

        private void OnNoTargetDetected()
        {
            // Debug.Log("No target found");
            _playerDetectedThisFrame = false;
            NoPlayerDetected?.Invoke();
        }
        
        public bool IsPlayerDetectedThisFrame() => _playerDetectedThisFrame;
        
        private float CalculateModifier(float distanceToPlayer)
        {
            var minDistance = 0.5f;
            var maxDistance = _viewDistance;
            distanceToPlayer = Mathf.Clamp(distanceToPlayer, minDistance, maxDistance);

            var t = (distanceToPlayer - minDistance) / (maxDistance - minDistance);
            // Inverse Lerp to get modifier between 2 and 0.5
            var modifier = Mathf.Lerp(maxDistanceModifier, minDistanceModifier, t);
            return modifier;
        }

        public void UpdateView(float modifier)
        {
             _viewAngle *= modifier;
        }

        private void OnDrawGizmos()
        {
            if (_settings == null) InitializeSettings();
            
            UpdateHorizontalOffset();
            
            Gizmos.color = Color.red;
            
            var position = (Vector2)transform.position + offset;
            var direction = _settings.isFacingRight ? Vector2.right : Vector2.left;

            var leftBoundary = Quaternion.Euler(0, 0, _viewAngle / 2) * direction;
            var rightBoundary = Quaternion.Euler(0, 0, -_viewAngle / 2) * direction;

            Gizmos.DrawLine(position, position + (Vector2)(leftBoundary * _viewDistance));
            Gizmos.DrawLine(position, position + (Vector2)(rightBoundary * _viewDistance));
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(position, _viewDistance);
        }

        /*
         * Updates the horizontal offset of the cone when the guard switches directions
         * If this isn't done the cone will be in an improper offset on one side or the other unless its origin
         * is exactly the same as the enemy's horizontal origin
         */
        private void UpdateHorizontalOffset()
        {
            if (_settings.isFacingRight)
            {
                offset.x = -Mathf.Abs(offset.x);
            }
            else
            {
                offset.x = Mathf.Abs(offset.x);
            }
        }
        
        // Wrapping GetComponent calls here so they can be called for the Gizmos in editor mode
        private void InitializeSettings()
        {
            _settings = GetComponent<EnemySettings>();
            _stateManager = GetComponent<EnemyStateManager>();
            if (_settings == null)
            {
                Debug.LogError("GuardSettings component not found on " + gameObject.name);
            }
        }
    }
}
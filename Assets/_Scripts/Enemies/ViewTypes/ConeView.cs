using System;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Enemies
{
    public class ConeView : MonoBehaviour, IViewType
    {
        [Header("Cone View Settings")]
        public float viewAngle = 45f;
        public float viewDistance = 5f;
        public LayerMask targetLayer;
        public LayerMask environmentLayer;
        public Vector2 offset; // (0.4, 1.0) for current enemy model

        private EnemySettings _settings;
        private DetectionLogic _detectionLogic;

        public event Action PlayerDetected;
        public event Action NoPlayerDetected;
        private void Awake()
        {
            InitializeSettings(); 
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
            var targetsInViewRadius = Physics2D.OverlapCircleAll(position, viewDistance, targetLayer);

            foreach (var target in targetsInViewRadius)
            {
                var directionToTarget = (target.transform.position - transform.position).normalized;

                // Check if the target is within the adjusted view angle
                var angleBetween = Vector2.Angle(direction, directionToTarget);

                if (!(angleBetween < viewAngle / 2)) continue;
                // TODO: When adding variable detection lengths based on distance get the distance from here
                // Check for obstacles between the enemy and the target
                var hit = Physics2D.Raycast(position, directionToTarget, viewDistance, environmentLayer);

                if (hit.collider == null)
                {
                    // Target is detected
                    PlayerDetected?.Invoke();
                    return;
                }
            }
            
            NoPlayerDetected?.Invoke();
        }

        public void UpdateView(float modifier)
        {
             viewAngle *= modifier;
        }

        // Callback when a target is detected
        private void OnTargetDetected(GameObject target)
        {
            // Debug.Log("Player detected");
        }

        private void OnDrawGizmosSelected()
        {
            if (_settings == null) InitializeSettings();
            
            UpdateHorizontalOffset();
            
            Gizmos.color = Color.red;
            
            var position = (Vector2)transform.position + offset;
            var direction = _settings.isFacingRight ? Vector2.right : Vector2.left;

            var leftBoundary = Quaternion.Euler(0, 0, viewAngle / 2) * direction;
            var rightBoundary = Quaternion.Euler(0, 0, -viewAngle / 2) * direction;

            Gizmos.DrawLine(position, position + (Vector2)(leftBoundary * viewDistance));
            Gizmos.DrawLine(position, position + (Vector2)(rightBoundary * viewDistance));
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(position, viewDistance);
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
            _detectionLogic = GetComponent<DetectionLogic>();
            if (_settings == null)
            {
                Debug.LogError("GuardSettings component not found on " + gameObject.name);
            }
        }
    }
}
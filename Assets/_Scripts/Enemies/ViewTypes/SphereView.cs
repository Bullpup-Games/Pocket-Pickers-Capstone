using System;
using UnityEngine;

namespace _Scripts.Enemies.ViewTypes
{
    public class SphereView : MonoBehaviour, IViewType
    {
        [Header("Sphere View Settings")]
        public float normalViewRadius = 5f;    // Normal detection radius
        public float alertedViewRadius = 10f;  // Detection radius when alerted
        public LayerMask targetLayer;          // Layer of the player
        public LayerMask environmentLayer;     // Layers considered as obstacles
        public Vector2 offset;                 // Offset from the enemy's position

        public Color color = Color.blue;

        private float _viewRadius;
        private EnemySettings _settings;
        private EnemyStateManager _stateManager;
        private DetectionLogic _detectionLogic;
        private bool _playerDetectedThisFrame = false;

        private float _baseNormalViewRadius;
        private float _baseAlertedViewRadius;

        public event Action PlayerDetected;
        public event Action NoPlayerDetected;

        private void Awake()
        {
            InitializeSettings();
        }

        private void Update()
        {
            // Update the view radius based on the enemy's current state
            if (_stateManager.state == EnemyState.Aggro || _stateManager.state == EnemyState.Searching)
            {
                _viewRadius = alertedViewRadius;
            }
            else
            {
                _viewRadius = normalViewRadius;
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

            // Check for player colliders within the view radius
            var targetsInViewRadius = Physics2D.OverlapCircleAll(position, _viewRadius, targetLayer);

            foreach (var target in targetsInViewRadius)
            {
                var directionToTarget = ((Vector2)target.transform.position - position).normalized;

                // Check for obstacles between the enemy and the target
                var hit = Physics2D.Raycast(position, directionToTarget, _viewRadius, environmentLayer);

                if (hit.collider == null)
                {
                    // Target is detected
                    OnTargetDetected();
                    return;
                }
            }

            OnNoTargetDetected();
        }

        private void OnTargetDetected()
        {
            _playerDetectedThisFrame = true;
            PlayerDetected?.Invoke();
        }

        private void OnNoTargetDetected()
        {
            _playerDetectedThisFrame = false;
            NoPlayerDetected?.Invoke();
        }

        public bool IsPlayerDetectedThisFrame() => _playerDetectedThisFrame;

        public void UpdateView(float modifier)
        {
            // Adjust the view radii based on the modifier
            normalViewRadius = _baseNormalViewRadius * modifier;
            alertedViewRadius = _baseAlertedViewRadius * modifier;
        }

        private void OnDrawGizmosSelected()
        {
            if (_settings == null) InitializeSettings();

            UpdateHorizontalOffset();

            Gizmos.color = color;
            var position = (Vector2)transform.position + offset;
            float radius = _viewRadius != 0 ? _viewRadius : normalViewRadius;
            Gizmos.DrawWireSphere(position, radius);
        }

        /*
         * Updates the horizontal offset when the enemy switches directions
         * Adjust this method if the offset depends on facing direction
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

        // Initialize components and base view radii
        private void InitializeSettings()
        {
            _settings = GetComponent<EnemySettings>();
            _detectionLogic = GetComponent<DetectionLogic>();
            _stateManager = GetComponent<EnemyStateManager>();

            if (_settings == null)
            {
                Debug.LogError("EnemySettings component not found on " + gameObject.name);
            }

            // Store base radii for modifiers
            _baseNormalViewRadius = normalViewRadius;
            _baseAlertedViewRadius = alertedViewRadius;
        }
    }
}
using System;
using System.Collections.Generic;
using _Scripts.Card;
using _Scripts.Enemies.Sniper.State;
using _Scripts.Player;
using UnityEngine;

namespace _Scripts.Enemies.ViewTypes
{
    [RequireComponent(typeof(IEnemyStateManager<SniperStateManager>))]
    public class RayView : MonoBehaviour, IViewType
    {
        [Tooltip(
             "Upper bound for the modifier given to the detection timer when the player is at the far edge of the view")]
        public float maxSweepModifier = 3f;

        [Tooltip(
             "Lower bound for the modifier given to the detection timer when the player is close to the origin of the view")]
        public float minSweepModifier = 1f;

        [Header("Ray View Settings")]
        public float sweepAngle = 90f; // Total sweep angle
        public float defaultSweepSpeed = 10f;// Degrees per second
        public float sweepSpeed;//with modifiers
        public float maxRayDistance = 100f; // We want the ray to basically be infinite but to define the line renderer & gizmos a max is needed, just keep it set high
        public Vector2 offset; // Ray origin offset
        public LayerMask targetLayer;
        public LayerMask environmentLayer;

        private IEnemySettings _settings;
        private IEnemyStateManager<SniperStateManager> _stateManager;
        private bool _playerDetectedThisFrame = false;
        private bool _playerDetectedLastFrame = false;
        
        private float _currentAngle;
        private Vector2 _lastKnownPlayerPosition;
        private Transform _playerTransform;
        private LineRenderer _lineRenderer;
        
        /*
         * State booleans, default behavior is to sweep vertically.
         * When a player is spotted the ray will follow the player's position
         * If the player manages to get away and hide the shot will be fired at their last known position.
         * When reloading or disabled the ray will not be shown and have no behavior.
         */
        private bool _isSweeping = true;
        private bool _isTrackingPlayer = false;
        private bool _isTrackingLastKnownPosition = false;
        private bool _isDisabled = false;
        private bool _lookingAtFalseTrigger = false;

        public event Action<bool, float> PlayerDetected;
        public event Action NoPlayerDetected;
        
        // Ray view is set up specifically for the sniper, which doesn't have a detection mode.
        // This could potentially be used for a quick charge instead, but would likely feel very unfair.
        public bool QuickDetection() => false;

        private void Awake()
        {
            InitializeSettings();
            // Start at the lower bound of the sweep
            _currentAngle = -sweepAngle / 2; 
        }

        private void Update()
        {
            if (_stateManager.IsDisabledState() || _stateManager.IsReloadingState())
            {
                EnterDisabledState();
                return;
            }

            if (_stateManager.IsInvestigatingState())
            {
                LookAtFalseTrigger();
                CastRay();

                // If the player crosses the ray while the sniper is investigating it will switch to tracking the player
                if (_playerDetectedThisFrame)
                    _stateManager.TransitionToState(GetComponent<SniperStateManager>().ChargingState);
                else
                    return;
            }
            
            // The ray is disabled but the enemy is back in a normal state (patrolling, charging, etc)
            if (_isDisabled)
                ExitDisabledState();

            UpdateStateBooleans();

            if (_isSweeping)
                Sweep();
            else if (_isTrackingPlayer)
                TrackPlayer();
            else if (_isTrackingLastKnownPosition)
                TrackLastKnownPosition();


            CastRay();

            _playerDetectedLastFrame = _playerDetectedThisFrame;
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
            /* Intentionally empty */
        }

        #region State Management

        private void UpdateStateBooleans()
        {
            if (_stateManager.IsDisabledState() || _stateManager.IsReloadingState())
            {
                EnterDisabledState();
            }
            else
            {
                // Player was detected this frame, track them
                if (_playerDetectedThisFrame)
                {
                    _isSweeping = false;
                    _isTrackingPlayer = true;
                    _isTrackingLastKnownPosition = false;
                }
                // Player was detected last frame but not this one, track the last known position
                else if (_playerDetectedLastFrame && !_playerDetectedThisFrame)
                {
                    _lastKnownPlayerPosition = _playerTransform.position;
                    // Track the last known position until the shot is fired
                    if (_stateManager.IsChargingState())
                    {
                        _isSweeping = false;
                        _isTrackingPlayer = false;
                        _isTrackingLastKnownPosition = true;
                    }
                    // Shot fired, prepare sweeping state
                    else
                    {
                        _isSweeping = true;
                        _isTrackingPlayer = false;
                        _isTrackingLastKnownPosition = false;
                    }
                }
                // Exit tracking last known position when not in Charging State
                else if (_isTrackingLastKnownPosition && !_stateManager.IsChargingState())
                {
                    _isSweeping = true;
                    _isTrackingLastKnownPosition = false;
                }
            }
        }

        private void EnterDisabledState()
        {
            if (!_isDisabled)
            {
                _isDisabled = true;
                _lineRenderer.enabled = false;
                _isSweeping = false;
                _isTrackingPlayer = false;
                _isTrackingLastKnownPosition = false;
            }
        }

        private void ExitDisabledState()
        {
            _isDisabled = false;
            _lineRenderer.enabled = true;
            _isSweeping = true;
        }

        #endregion

        #region Behavior Methods

        private void Sweep()
        {
            // Update the angle over time to sweep back and forth when a player is not in sight
            _currentAngle += sweepSpeed * Time.deltaTime;

            if (_currentAngle > sweepAngle / 2)
            {
                _currentAngle = sweepAngle / 2;
                sweepSpeed = -Mathf.Abs(sweepSpeed);
            }
            else if (_currentAngle < -sweepAngle / 2)
            {
                _currentAngle = -sweepAngle / 2;
                sweepSpeed = Mathf.Abs(sweepSpeed);
            }
        }

        private void TrackPlayer()
        {
            if (_playerTransform is null)
            {
                // Lost player reference, switch states will handle the transition
                return;
            }

            var position = (Vector2)transform.position;
            var directionToPlayer = ((Vector2)_playerTransform.position - position).normalized;
            var angleToPlayer = Mathf.Atan2(directionToPlayer.y, directionToPlayer.x) * Mathf.Rad2Deg;

            var facingDirectionAngle = _settings.IsFacingRight() ? 0f : 180f;
            var angleDifference = Mathf.DeltaAngle(facingDirectionAngle, angleToPlayer);
            
            // Player is outside of the sweep angle, resume sweeping
            if (Mathf.Abs(angleDifference) > sweepAngle / 2)
            {
                _isSweeping = true;
                _isTrackingPlayer = false;
                _playerTransform = null;
                return;
            }

            _currentAngle = angleDifference;
        }

        private void TrackLastKnownPosition()
        {
            var position = (Vector2)transform.position;
            var directionToLastKnown = (_lastKnownPlayerPosition - position).normalized;
            var angleToLastKnown = Mathf.Atan2(directionToLastKnown.y, directionToLastKnown.x) * Mathf.Rad2Deg;

            var facingDirectionAngle = _settings.IsFacingRight() ? 0f : 180f;
            var angleDifference = Mathf.DeltaAngle(facingDirectionAngle, angleToLastKnown);

            // Last known position is outside of the sweep angle
            // Keep the angle at the boundary
            if (Mathf.Abs(angleDifference) > sweepAngle / 2)
                _currentAngle = Mathf.Sign(angleDifference) * (sweepAngle / 2);
            else
                _currentAngle = angleDifference;
        }

        private void LookAtFalseTrigger()
        {
            var position = (Vector2)transform.position;
            var falseTriggerPosition = CardManager.Instance.lastFalseTriggerPosition;
            var directionToFalseTrigger = (falseTriggerPosition - position).normalized;
            var angleToFalseTrigger = Mathf.Atan2(directionToFalseTrigger.y, directionToFalseTrigger.x) * Mathf.Rad2Deg;

            var facingDirectionAngle = _settings.IsFacingRight() ? 0 : 180f;
            var angleDifference = Mathf.DeltaAngle(facingDirectionAngle, angleToFalseTrigger);
            
            // Check for out of bounds intentionally left out, false trigger will be out of the sight bounds a lot, but
            // it should still be able to be used as a distraction.

            _currentAngle = angleDifference;
        }

        #endregion

        private void CastRay()
        {
            if (_isDisabled)
                return;

            UpdateHorizontalOffset();
            var position = (Vector2)transform.position + offset;

            // Calculate the direction of the ray based on the current angle
            var facingDirection = _settings.IsFacingRight() ? 0f : 180f;
            var angle = facingDirection + _currentAngle;
            var direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

            var hit = Physics2D.Raycast(position, direction, maxRayDistance, targetLayer | environmentLayer);

            if (hit.collider != null)
            {
                // If we hit something, draw the line to that point
                _lineRenderer.SetPosition(0, position);
                _lineRenderer.SetPosition(1, hit.point);

                if (((1 << hit.collider.gameObject.layer) & targetLayer) != 0)
                {
                    // Hit the player
                    var distanceToPlayer = Vector2.Distance(position, hit.point);
                    var modifier = CalculateModifier(distanceToPlayer);
                    OnTargetDetected(modifier, hit.collider.transform);
                }
                else
                {
                    // Did not hit the player
                    OnNoTargetDetected();
                }
            }
            else
            {
                // No collision detected
                _lineRenderer.SetPosition(0, position);
                _lineRenderer.SetPosition(1, position + direction * maxRayDistance);
                OnNoTargetDetected();
            }
        }

        private void OnTargetDetected(float modifier, Transform playerTransform)
        {
            _playerDetectedThisFrame = true;
            PlayerDetected?.Invoke(false, modifier);

            _playerTransform = playerTransform;
            _lastKnownPlayerPosition = playerTransform.position;
        }

        private void OnNoTargetDetected()
        {
            _playerDetectedThisFrame = false;
            NoPlayerDetected?.Invoke();
        }

        public bool IsPlayerDetectedThisFrame() => _playerDetectedThisFrame;

        
        private float CalculateModifier(float distanceToPlayer)
        {
            /*
             * TODO: Figure out if this should be affected by distance to the player or left alone.
             * Other views have detection speed modifiers based on distance so it would make sense to use here as well, but
             * it would be for the charge time instead of the detection speed, which could easily get out of hand and be hard to balance.
             */ 
            return 1f;
        }
        
        public void UpdateView(float modifier)
        {
            // sweepSpeed *= modifier;
        }

        private void OnDrawGizmos()
        {
            if (_settings == null) InitializeSettings();

            UpdateHorizontalOffset();
            var position = (Vector2)transform.position + offset;

            // Draw the sweep arc
            Gizmos.color = Color.yellow;
            var facingDirection = _settings.IsFacingRight() ? 0f : 180f;
            var leftBoundary = new Vector2(Mathf.Cos((facingDirection - sweepAngle / 2) * Mathf.Deg2Rad),
                Mathf.Sin((facingDirection - sweepAngle / 2) * Mathf.Deg2Rad));
            var rightBoundary = new Vector2(Mathf.Cos((facingDirection + sweepAngle / 2) * Mathf.Deg2Rad),
                Mathf.Sin((facingDirection + sweepAngle / 2) * Mathf.Deg2Rad));

            Gizmos.DrawLine(position, position + leftBoundary * maxRayDistance);
            Gizmos.DrawLine(position, position + rightBoundary * maxRayDistance);

            // Draw the current ray
            Gizmos.color = Color.blue;
            var direction = new Vector2(Mathf.Cos((facingDirection + _currentAngle) * Mathf.Deg2Rad),
                Mathf.Sin((facingDirection + _currentAngle) * Mathf.Deg2Rad));
            Gizmos.DrawLine(position, position + direction * maxRayDistance);
        }

        private void UpdateHorizontalOffset()
        {
            if (_settings.IsFacingRight())
            {
                offset.x = -Mathf.Abs(offset.x);
            }
            else
            {
                offset.x = Mathf.Abs(offset.x);
            }
        }

        private void InitializeSettings()
        {
            _settings = GetComponent<IEnemySettings>();
            _stateManager = GetComponent<IEnemyStateManager<SniperStateManager>>();
            _lineRenderer = GetComponent<LineRenderer>();
            if (_settings == null)
            {
                Debug.LogError("EnemySettings component not found on " + gameObject.name);
            }

            _lineRenderer.startWidth = 0.05f;
            _lineRenderer.endWidth = 0.05f;
            _lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // TODO: Maybe change to a custom material?
            _lineRenderer.startColor = Color.red;
            _lineRenderer.endColor = Color.red;
            sweepSpeed = defaultSweepSpeed;
        }

        public void ChangeView()
        {
            //the default sweep speed is 10. We want its maximum to be 30. We need the modifier to go between 0 and 20
            int sinModifier = PlayerVariables.Instance.sinHeld;

            if (sinModifier >= 135)
            {
                sinModifier = 135;
            }
            //transposes the range of 0-135 to the range of 0-20
            float speedModifier = (float)sinModifier / 6.75f;

            sweepSpeed = defaultSweepSpeed + speedModifier;
        }
    }
}
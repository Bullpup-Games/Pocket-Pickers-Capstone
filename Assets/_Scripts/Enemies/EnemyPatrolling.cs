using System.Collections;
using UnityEngine;

namespace _Scripts.Enemies
{
    [RequireComponent(typeof(Rigidbody2D)), RequireComponent(typeof(BoxCollider2D))]
    public class EnemyPatrolling : MonoBehaviour
    {
        [Header("Patrol Settings")]
        public float movementSpeed = 4f;
        [Tooltip("Distance to patrol from the left of the origin")]
        public float leftPatrolDistance = 3f;
        [Tooltip("Distance to patrol from the right of the origin")]
        public float rightPatrolDistance = 3f;
        [Tooltip("Time to wait after each patrol segment")]
        public float waitTimeAtEnds = 2f;

        [Header("Physics Settings")]
        public float maxFallSpeed = 20f;
        public float gravity = 10f;

        [Header("Ground Detection")]
        [Tooltip("Distance for ground check raycast")]
        public float groundCheckDistance = 0.5f;
        public LayerMask groundLayer;

        [Header("Gizmo Settings")]
        public Color patrolPathColor = Color.green;
        public Color groundRayColor = Color.red;

        private Rigidbody2D _rb;
        private EnemySettings _settings;
        private EnemyStateManager _stateManager;
        private Vector2 _originPosition;
        private Vector2 _leftPatrolPoint;
        private Vector2 _rightPatrolPoint;
        private Vector2 _currentTarget;
        private bool _isWaiting;
        private bool _isMovingRight;
        private bool _isGrounded;

        private void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            _settings = GetComponent<EnemySettings>();
            _stateManager = GetComponent<EnemyStateManager>();
            _originPosition = transform.position;

            // Calculate patrol points based on origin and patrol distances
            _leftPatrolPoint = _originPosition + Vector2.left * leftPatrolDistance;
            _rightPatrolPoint = _originPosition + Vector2.right * rightPatrolDistance;
            
            // Set initial direction and target based on facing direction
            if (_settings.isFacingRight)
            {
                _isMovingRight = true;
                _currentTarget = _rightPatrolPoint;
            }
            else
            {
                _isMovingRight = false;
                _currentTarget = _leftPatrolPoint;
                _settings.FlipLocalScale();
            }
        }

        private void Update()
        {
            HandleGroundDetection();
            HandleGravity();

            if ((_stateManager.state is EnemyState.Patrolling or EnemyState.Detecting) && !_isWaiting)
            {
                MoveTowardsTarget();
            }
            else if (_stateManager.state == EnemyState.Returning)
            {
                ReturnToPatrolPosition();
            }
            else if (_isWaiting)
            {
                // During waiting state make sure any horizontal movement is halted but allow gravity
                _rb.velocity = new Vector2(0, _rb.velocity.y);
            }
        }

        private void MoveTowardsTarget()
        {
            var direction = _isMovingRight ? 1f : -1f;

            _rb.velocity = new Vector2(direction * movementSpeed, _rb.velocity.y);

            // Checking if the target position has been reached
            if ((!_isMovingRight || !(transform.position.x >= _currentTarget.x))
                && (_isMovingRight || !(transform.position.x <= _currentTarget.x))) return;
        
            _rb.velocity = new Vector2(0, _rb.velocity.y);

            if (_stateManager.state == EnemyState.Patrolling)
            {
                _isWaiting = true;
                StartCoroutine(WaitAtPoint()); 
            }
            
        }
        
        private void ReturnToPatrolPosition()
        {
            var distanceToOrigin = _originPosition.x - transform.position.x;
            var direction = distanceToOrigin > 0 ? 1f : -1f;

            if ((_settings.isFacingRight && direction < 0) || (!_settings.isFacingRight && direction > 0))
            {
                _settings.FlipLocalScale();
            }

            // Move towards the origin position
            _rb.velocity = new Vector2(direction * movementSpeed, _rb.velocity.y);

            if (!(Mathf.Abs(distanceToOrigin) <= 0.1f)) return;
            _rb.velocity = new Vector2(0, _rb.velocity.y);
            _stateManager.SetState(EnemyState.Patrolling);

            // Reset patrol variables
            // Set initial direction and target based on facing direction
            if (_settings.isFacingRight)
            {
                _isMovingRight = true;
                _currentTarget = _rightPatrolPoint;
            }
            else
            {
                _isMovingRight = false;
                _currentTarget = _leftPatrolPoint;
            }
        }

        // Coroutine to handle the wait time and prepare the variables for the next patrol
        private IEnumerator WaitAtPoint()
        {
            yield return new WaitForSeconds(waitTimeAtEnds);

            // Flip the guard around after waiting the full duration
            SwitchPoints();
        }
    
        private void HandleGroundDetection()
        {
            var hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
            _isGrounded = hit.collider != null;
            // TODO: Add wall detection that triggers a turnaround, currently gets stuck on walls if hit
        }

        private void HandleGravity()
        {
            if (_isGrounded) return;
        
            // Apply gravity to make the guard fall
            var newYVelocity = Mathf.MoveTowards(_rb.velocity.y, -maxFallSpeed, gravity * Time.deltaTime);
            _rb.velocity = new Vector2(_rb.velocity.x, newYVelocity);
        }

        // Turn around and switch patrol points
        private void SwitchPoints()
        {
            _isMovingRight = !_isMovingRight;
            _settings.FlipLocalScale();
            _currentTarget = _isMovingRight ? _rightPatrolPoint : _leftPatrolPoint;
            _isWaiting = false;
        }

        // Gizmos of the patrol distance and ground detection ray visible in the editor
        private void OnDrawGizmos()
        {
            Gizmos.color = patrolPathColor;

            var originPosition = Application.isPlaying ? _originPosition : (Vector2)transform.position;

            // Draw left patrol ray
            var leftPoint = originPosition + Vector2.left * leftPatrolDistance;
            Gizmos.DrawLine(originPosition, leftPoint);
            Gizmos.DrawSphere(leftPoint, 0.1f);

            // Draw right patrol ray
            var rightPoint = originPosition + Vector2.right * rightPatrolDistance;
            Gizmos.DrawLine(originPosition, rightPoint);
            Gizmos.DrawSphere(rightPoint, 0.1f);

            // Draw ground detection ray
            Gizmos.color = groundRayColor;
            var currentPosition = Application.isPlaying ? (Vector2)transform.position : originPosition;
            Gizmos.DrawLine(currentPosition, currentPosition + Vector2.down * groundCheckDistance);
        }
    }
}
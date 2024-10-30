using System;
using _Scripts.Enemies;
using _Scripts.Enemies.State;
using _Scripts.Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Card
{
    public class Card : MonoBehaviour
    {
        [Header("False Trigger Settings")]
        [SerializeField] private float falseTriggerRadius = 4f;
        [SerializeField] private Color gizmoColor = Color.cyan;

        public float speed = 20f; // Speed of the card
        public int totalBounces;  // Total allowed bounces
        public int bounces;       // Current number of bounces

        private Vector2 _direction;   // Current movement direction
        private Vector2 _velocity;    // Current velocity
        private float _startTime;    // Time when the card was instantiated

        private Rigidbody2D _rb;
        private Vector2 _previousPosition; // Position in the previous frame
        public Vector2 lastSafePosition; // Keep track of safe positions to teleport as the card travels

        private const float MinMoveDistance = 0.01f; // Minimum movement distance to prevent sticking

        #region Singleton

        public static Card Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType(typeof(Card)) as Card;

                return _instance;
            }
            set => _instance = value;
        }

        private static Card _instance;

        #endregion

        private void OnEnable()
        {
            SetListeners();
        }

        private void OnDestroy()
        {
            DeleteListeners();
        }

        private void SetListeners()
        {
            if (InputHandler.Instance != null)
            {
                InputHandler.Instance.OnFalseTrigger += ActivateFalseTrigger;
                InputHandler.Instance.OnCancelActiveCard += DestroyCard;
            }

            if (CardManager.Instance != null)
            {
                CardManager.Instance.Teleport += CatchTeleport;
            }
        }

        private void DeleteListeners()
        {
            if (InputHandler.Instance != null)
            {
                InputHandler.Instance.OnFalseTrigger -= ActivateFalseTrigger;
                InputHandler.Instance.OnCancelActiveCard -= DestroyCard;
            }

            if (CardManager.Instance != null)
            {
                CardManager.Instance.Teleport -= CatchTeleport;
            }
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.isKinematic = true;
            bounces = 0;

            // Ignore collisions with the player to avoid pushing on Awake frame
            var playerCollider = PlayerVariables.Instance.gameObject.GetComponent<Collider2D>();
            Physics2D.IgnoreCollision(playerCollider, GetComponent<Collider2D>());

            lastSafePosition = PlayerVariables.Instance.transform.position;
            _startTime = Time.time;
            
            // Calculate initial velocity
            _direction = HandleCardStanceArrow.Instance.currentDirection.normalized;
            CalculateVelocity(_direction);
        }

        public void Launch(Vector2 direction)
        {
            _direction = direction.normalized;
            CalculateVelocity(_direction);
        }

        private void CalculateVelocity(Vector2 direction)
        {
            _velocity = direction * speed;
        }

        private void Update()
        {
            // Destroy the card after its lifetime expires
            if (Time.time - _startTime >= CardManager.Instance.cardLifeTime)
            {
                DestroyCard();
            }
        }

        private void FixedUpdate()
        {
            MoveCard();
            UpdateSafePosition();
        }

        private void MoveCard()
        {
            // Store the previous position
            _previousPosition = transform.position;

            // Calculate movement
            var movement = _velocity * Time.fixedDeltaTime;
            var newPosition = _previousPosition + movement;

            // If movement amount is too small skip to prevent sticking
            if (movement.magnitude < MinMoveDistance)
            {
                movement = _direction * MinMoveDistance;
                newPosition = _previousPosition + movement;
            }

            // Perform a raycast from previousPosition to newPosition to check for collisions in between frames
            var hit = Physics2D.Raycast(
                _previousPosition,
                movement.normalized,
                movement.magnitude,
                LayerMask.GetMask("Environment")
            );

            if (hit.collider != null)
            {
                // Adjust position to point of collision
                newPosition = hit.point;

                // Reflect the direction off the hit normal
                var newDirection = Vector2.Reflect(_direction, hit.normal).normalized;

                _direction = newDirection;
                CalculateVelocity(_direction);

               /*
                    When you hit a corner, it will count as 2 bounces because it bounces off of both corners 
                    at the same time
               */
                 bounces++;

                // Safety check if we entered with max bounces
                if (bounces >= totalBounces)
                {
                    DestroyCard();
                    return;
                }

                // Adjust position slightly along the new direction to prevent immediate re-collision
                newPosition += _direction * MinMoveDistance;

                Debug.Log("Reflected off Environment. New direction: " + _direction);
            }

            // Move the card to the new position
            transform.position = newPosition;
        }

        /*
         * Keeps track of the last known safe position to teleport to each frame the card is alive
         * to prevent teleportation into walls or objects
         */
        private void UpdateSafePosition()
        {
            var playerCollider = PlayerVariables.Instance.Collider2D;
            var colliderSize = playerCollider.bounds.size;
            var colliderOffset = playerCollider.offset;
            
            // get the current collider position with the player offset to cast from when checking for hits
            var colliderPos = (Vector2)transform.position + colliderOffset;
            
            // Check for obstructions that would prevent a teleport in this location
            var hits = Physics2D.OverlapBoxAll(
                colliderPos,
                colliderSize,
                0f,
                LayerMask.GetMask("Environment", "Enemy")
            );
            
            if (hits.Length == 0)
                lastSafePosition = transform.position;
        }

        private void ActivateFalseTrigger()
        {
            if (CardManager.Instance.falseTriggerOnCooldown)
            {
                Debug.Log("False Trigger on cooldown.");
                return;
            }

            var colliders = Physics2D.OverlapCircleAll(transform.position, falseTriggerRadius, LayerMask.GetMask("Enemy"));

            foreach (var col in colliders)
            {
                var enemyStateManager = col.GetComponent<EnemyStateManager>();
                if (enemyStateManager != null)
                {
                    enemyStateManager.TransitionToState(enemyStateManager.StunnedState);
                }
            }

            CardManager.Instance.ActivateFalseTriggerCooldown();
            DestroyCard();
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            // Ignore collisions with the player
            if (col.gameObject.CompareTag("Player"))
            {
                Physics2D.IgnoreCollision(col.collider, GetComponent<Collider2D>());
                return;
            }
            if (col.gameObject.CompareTag("enemy"))
            {
                //todo if the card hits an enemy, incapacitate the enemy and destroy the card
                Physics2D.IgnoreCollision(col.collider, GetComponent<Collider2D>());
                var enemy = col.gameObject.GetComponent<EnemyStateManager>();
                enemy.KillEnemy();
                // enemy.TransitionToState(enemy.DisabledState);
                DestroyCard();
                return;
            }
            if (col.gameObject.CompareTag("permeable"))
            {
                //this tag exists because we want the player and enemies to be stopped
                //by permeable objects, but not the card.
                Physics2D.IgnoreCollision(col.collider, GetComponent<Collider2D>());
                return;
            }
            if (col.gameObject.CompareTag("wall"))
            {
                // Handle collision with walls (if not already handled by the raycast)
               // bounces++;

                //if (bounces >= totalBounces)
                //{
                //    DestroyCard();
                //    return;
                //}

                // // Calculate the combined normal from ALL contact points
                // var combinedNormal = Vector2.zero;
                // var contactCount = col.contactCount;
                //
                // for (var i = 0; i < contactCount; i++)
                // {
                //     combinedNormal += col.GetContact(i).normal;
                // }
                //
                // combinedNormal = combinedNormal.normalized;
                //
                // // Reflect the direction and recalculate velocity
                // var newDirection = Vector2.Reflect(_direction, combinedNormal).normalized;
                //
                // // If the new direction is too similar to the old direction, invert the direction
                // if (Vector2.Dot(newDirection, _direction) > 0.99f)
                // {
                //     newDirection = -_direction;
                //     Debug.Log("Inverted direction due to corner collision.");
                // }
                //
                // _direction = newDirection;
                // CalculateVelocity(_direction);
                //
                // // Adjust position slightly along the new direction to prevent immediate re-collision
                // transform.position += (Vector3)(_direction * MinMoveDistance);

                return;
            }
            if (col.gameObject.CompareTag("EscapeRout"))
            {
                DestroyCard();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("EscapeRout"))
            {
                DestroyCard();
            }
        }

        private void CatchTeleport(Vector2 noop)
        {
            DestroyCard();
        }

        // CALL THIS FUNCTION TO DESTROY CARDS, DON'T START FROM THE CARD MANAGER!
        public void DestroyCard()
        {
            // Notify the CardManager that the card has been destroyed
            if (CardManager.Instance != null)
            {
                CardManager.Instance.OnCardDestroyed();
            }
            Destroy(gameObject);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawWireSphere(transform.position, falseTriggerRadius);
        }
    }
}
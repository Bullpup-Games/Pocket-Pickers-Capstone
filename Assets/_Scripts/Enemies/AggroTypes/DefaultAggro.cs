using System;
using System.Collections;
using _Scripts.Enemies.ViewTypes;
using _Scripts.Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Enemies.AggroTypes
{
    public class DefaultAggro : MonoBehaviour, IAggroType
    {
        [SerializeField] private float movementSpeed = 11f;
        [Tooltip("The amount of time the enemy waits before flipping directions when the player crosses over them")]
        [SerializeField] private float flipTime = 0.2f;
        [Header("Quick Time Event Variables")]
        [SerializeField] private float qteTimeLimit = 4f;
        [SerializeField] private float timeLostPerEncounter = 0.5f;
        [SerializeField] private int counterGoal = 15;
        private bool _hasExecuted = false;
        private bool _isFlipping = false;

        private Vector2 _targetPosition;
        private Vector2 _lastKnownPosition;
        private bool _checkingLastKnownLocation;
        private IViewType[] _viewTypes;
        private EnemyStateManager _enemyStateManager;
        private PlayerStateManager _playerStateManager;
        private EnemySettings _settings;
        private Rigidbody2D _rb;

        public Vector2 LastKnownPosition() => _lastKnownPosition;
        
        private void Awake()
        {
            _viewTypes = GetComponents<IViewType>();
            _enemyStateManager = GetComponent<EnemyStateManager>();
            _playerStateManager = PlayerVariables.Instance.gameObject.GetComponent<PlayerStateManager>();
            _settings = GetComponent<EnemySettings>();
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            // TODO: Check for player distance and call QTE from there
            Movement();
        }

        /*
         * Default guard should move toward the player at a faster speed than the player is able to run
         * If the player breaks line of sight move to their last known position and switch states to Searching
         */
        public void Movement()
        {
            if (_enemyStateManager.state != EnemyState.Aggro) return;
            // if NoPlayerDetected is called call GoToLastKnownLocation and return;
            var noPlayerDetected = true;
            foreach (var viewType in _viewTypes)
            {
                if (viewType.IsPlayerDetectedThisFrame())
                {
                    noPlayerDetected = false;
                    break;
                }
            }
            
            if (noPlayerDetected)
            {
                if (!_checkingLastKnownLocation)
                {
                    var position = PlayerVariables.Instance.transform.position;
                    _lastKnownPosition = position;
                    GoToLastKnownLocation(position);
                }
                return;
            }

            var targetPos = (Vector2)PlayerVariables.Instance.transform.position;
            
            // If the enemy is already facing the target move to it
            if ((_settings.isFacingRight && targetPos.x > transform.position.x)
                || (!_settings.isFacingRight && targetPos.x < transform.position.x))
            {
                MoveTo(targetPos);
            }
            // Otherwise, turn around then move it it
            else if (!_isFlipping)
            {
                StartCoroutine(FlipLocalScale(targetPos));
            }
        }

        private void GoToLastKnownLocation(Vector2 location)
        {
            _checkingLastKnownLocation = true;
            // move to target location 
            MoveTo(location);
            
            // switch to Searching state
            _enemyStateManager.SetState(EnemyState.Searching);
        }

        private void MoveTo(Vector2 location)
        {
            var direction = _settings.isFacingRight ? 1f : -1f;
            var xVelocity = direction * movementSpeed;

            _rb.velocity = new Vector2(xVelocity, _rb.velocity.y);

            // Checking if the target position has been reached
            if ((!_settings.isFacingRight || !(transform.position.x >= location.x))
                && (_settings.isFacingRight || !(transform.position.x <= location.x))) return;
        
            _rb.velocity = new Vector2(0, _rb.velocity.y);
        }
        
        // Flip the entity's sprite by inverting the X scaling
        private IEnumerator FlipLocalScale(Vector2 location)
        {
            _isFlipping = true;
            yield return new WaitForSeconds(flipTime);
            // I don't know why the transformCopy needs to exist but Unity yelled at me when I didn't have it so here it sits..
            var transformCopy = transform;
            var localScale = transformCopy.localScale;
            localScale.x *= -1;
            transformCopy.localScale = localScale;
            
            _isFlipping = false;
            MoveTo(location);
        }

        // Modified grapple coroutine from Don't Move
        private IEnumerator StartQuicktimeEvent()
        {
            Debug.Log("QTE Started");
            _hasExecuted = false;
            // grappleUI.SetActive(true);
            var counter = 0;
            var timeElapsed = 0f;

            var playerRb = PlayerVariables.Instance.gameObject.GetComponent<Rigidbody2D>();

            while (timeElapsed < qteTimeLimit && counter < counterGoal)
            {
                // Stop any movement from the guard or player
                _rb.velocity = new Vector2(0f, 0f);
                playerRb.velocity = new Vector2(0f, 0f);
                if (Input.GetButtonDown("FalseTrigger"))
                {
                    counter++;
                }

                timeElapsed += Time.deltaTime;
                yield return null;
            }

            if (_hasExecuted) yield break;
            // Quick time event succeeded
            if (counter >= counterGoal)
            {
                // grappleUI.SetActive(false);
                // UnlockPositions();
                Debug.Log("QTE Passed");
                _enemyStateManager.SetState(EnemyState.Stunned);
                _playerStateManager.SetState(PlayerState.Idle);
                counterGoal += 2;
                if (qteTimeLimit > 2f)
                    qteTimeLimit -= timeLostPerEncounter;
            }
            // Quick time event failed
            else
            {
                Debug.Log("QTE Failed");
                StopCoroutine(StartQuicktimeEvent());
                // grappleUI.SetActive(false);
                // GameOver.Instance.EndGame();
            }

            _hasExecuted = true;
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (_enemyStateManager.state is EnemyState.Disabled or EnemyState.Stunned) return;
            if (col.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                var playerStateManager = col.gameObject.GetComponent<PlayerStateManager>();
                playerStateManager.SetState(PlayerState.Stunned);
                StartCoroutine(StartQuicktimeEvent());
            }
        }
    }
}
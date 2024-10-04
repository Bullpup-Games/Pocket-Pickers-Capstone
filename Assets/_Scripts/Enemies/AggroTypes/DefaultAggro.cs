using System;
using System.Collections;
using PlayerController;
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
        [SerializeField] private int counterGoal = 5;
        private bool _hasExecuted = false;
        private bool _isFlipping = false;

        private Vector2 _targetPosition;
        private bool _checkingLastKnownLocation;
        private IViewType[] _viewTypes;
        private EnemyStateManager _stateManager;
        private EnemySettings _settings;
        private Rigidbody2D _rb;

        private void Awake()
        {
            _viewTypes = GetComponents<IViewType>();
            _stateManager = GetComponent<EnemyStateManager>();
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
            if (_stateManager.state != EnemyState.Aggro) return;
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
                    GoToLastKnownLocation(PlayerVariables.Instance.transform.position);
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

        /*
         * When within a certain range the guard should grab the player and enter a QTE that gets progressively
         * harder to pass. If failed trigger a level reset
         */
        public void Action()
        {
            return;
            // check distance to player

            // if within range call StartQuickTimeEvent 
            // Handle the rest from there
        }

        private void GoToLastKnownLocation(Vector2 location)
        {
            _checkingLastKnownLocation = true;
            // move to target location 
            MoveTo(location);
            
            // switch to Searching state
            _stateManager.SetState(EnemyState.Searching);
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
            
            MoveTo(location);
            _isFlipping = false;
        }

        // Grapple coroutine from Don't Move - Needs to be integrated
        private IEnumerator StartQuicktimeEvent()
        {
            _hasExecuted = false;
            // grappleUI.SetActive(true);
            var counter = 0;
            var timeElapsed = 0f;

            while (timeElapsed < qteTimeLimit && counter < counterGoal)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    counter++;
                }

                timeElapsed += Time.deltaTime;
                yield return null;
            }

            if (_hasExecuted) yield break;
            if (counter >= counterGoal)
            {
                // grappleUI.SetActive(false);
                // UnlockPositions();
                if (qteTimeLimit > 2f)
                    qteTimeLimit -= timeLostPerEncounter;
            }
            else
            {
                StopCoroutine(StartQuicktimeEvent());
                // grappleUI.SetActive(false);
                // GameOver.Instance.EndGame();
            }

            _hasExecuted = true;
        }
    }
}
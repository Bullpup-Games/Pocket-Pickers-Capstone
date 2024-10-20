using System;
using _Scripts.Card;
using _Scripts.Enemies;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

namespace _Scripts.Player
{
    /// <summary>
    /// Hey!
    /// Tarodev here. I built this controller as there was a severe lack of quality & free 2D controllers out there.
    /// I have a premium version on Patreon, which has every feature you'd expect from a polished controller. Link: https://www.patreon.com/tarodev
    /// You can play and compete for best times here: https://tarodev.itch.io/extended-ultimate-2d-controller
    /// If you hve any questions or would like to brag about your score, come to discord: https://discord.gg/tarodev
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class PlayerMovement : MonoBehaviour, IPlayerController
    {
        private FrameInput _frameInput;
        private Vector2 _frameVelocity;
        private bool _cachedQueryStartInColliders;

        #region Interface

        public Vector2 FrameInput => _frameInput.Move;
        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;

        #endregion
        #region Singleton

        public static PlayerMovement Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType(typeof(PlayerMovement)) as PlayerMovement;

                return _instance;
            }
            set { _instance = value; }
        }

        private static PlayerMovement _instance;

        #endregion

        private float _time;

        private void Awake()
        {
            _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
        }

        private void OnEnable()
        {
            CardManager.Instance.Teleport += TeleportTo;
        }
        private void Update()
        {
            _time += Time.deltaTime;
        }
        
        
        public void GatherInput()
        {
            _frameInput = new FrameInput
            {
                JumpDown = InputHandler.Instance.JumpPressed,
                JumpHeld = InputHandler.Instance.JumpHeld,
                Move = InputHandler.Instance.MovementInput
            };
            
            // Track the facing direction based on the last non-zero horizontal input
            if (_frameInput.Move.x != 0)
            {
                // PlayerVariables.Instance.isFacingRight = _frameInput.Move.x > 0;
                if ((PlayerVariables.Instance.isFacingRight && _frameInput.Move.x < 0)||
                    (!PlayerVariables.Instance.isFacingRight && _frameInput.Move.x > 0))
                {
                    PlayerVariables.Instance.FlipLocalScale();
                }
                
            }
        
            if (PlayerVariables.Instance.Stats.SnapInput)
            {
                _frameInput.Move.x = Mathf.Abs(_frameInput.Move.x) < PlayerVariables.Instance.Stats.HorizontalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.x);
                _frameInput.Move.y = Mathf.Abs(_frameInput.Move.y) < PlayerVariables.Instance.Stats.VerticalDeadZoneThreshold ? 0 : Mathf.Sign(_frameInput.Move.y);
            }
        
            if (_frameInput.JumpDown)
            {
                _jumpToConsume = true;
                _timeJumpWasPressed = _time;
            }
        }

        private void FixedUpdate()
        {
            // CheckCollisions();
            //
            // HandleJump();
            // HandleDirection();
            // HandleGravity();
            //
            // ApplyMovement();
        }

        public void TeleportTo(Vector2 location)
        {
            gameObject.transform.position = location;
            gameObject.transform.rotation = Quaternion.identity;
            //todo set the player's velocity to 0
            
        }

        #region Collisions
        
        private float _frameLeftGrounded = float.MinValue;
        private bool _grounded;

        public bool isGrounded() { return _grounded;}
        public void CheckCollisions()
        {
            Physics2D.queriesStartInColliders = false;

            // Ground and Ceiling
            var origin = (Vector2)PlayerVariables.Instance.Collider2D.transform.position + PlayerVariables.Instance.Collider2D.offset;

            bool groundHit = Physics2D.BoxCast(
                origin,
                PlayerVariables.Instance.Collider2D.size,
                0f,
                Vector2.down,
                PlayerVariables.Instance.Stats.GrounderDistance,
                ~PlayerVariables.Instance.Stats.PlayerLayer
            );

            bool ceilingHit = Physics2D.BoxCast(
                origin,
                PlayerVariables.Instance.Collider2D.size,
                0f,
                Vector2.up,
                PlayerVariables.Instance.Stats.GrounderDistance,
                ~PlayerVariables.Instance.Stats.PlayerLayer
            );

            // Hit a Ceiling
            if (ceilingHit) _frameVelocity.y = Mathf.Min(0, _frameVelocity.y);

            // Landed on the Ground
            if (!_grounded && groundHit)
            {
                _grounded = true;
                _coyoteUsable = true;
                _bufferedJumpUsable = true;
                _endedJumpEarly = false;
                GroundedChanged?.Invoke(true, Mathf.Abs(_frameVelocity.y));
            }
            // Left the Ground
            else if (_grounded && !groundHit)
            {
                _grounded = false;
                _frameLeftGrounded = _time;
                GroundedChanged?.Invoke(false, 0);
            }

            Physics2D.queriesStartInColliders = _cachedQueryStartInColliders;
        }

        #endregion


        #region Jumping

        private bool _jumpToConsume;
        private bool _bufferedJumpUsable;
        private bool _endedJumpEarly;
        private bool _coyoteUsable;
        private float _timeJumpWasPressed;

        public bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + PlayerVariables.Instance.Stats.JumpBuffer;
        public bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + PlayerVariables.Instance.Stats.CoyoteTime;

        public void HandleJump()
        {
            if (!_endedJumpEarly && !_grounded && !_frameInput.JumpHeld && PlayerVariables.Instance.RigidBody2D.velocity.y > 0) _endedJumpEarly = true;

            if (!_jumpToConsume && !HasBufferedJump) return;

            if (_grounded || CanUseCoyote) ExecuteJump();

            _jumpToConsume = false;
        }

        public void ExecuteJump()
        {
            _endedJumpEarly = false;
            _timeJumpWasPressed = 0;
            _bufferedJumpUsable = false;
            _coyoteUsable = false;
            _frameVelocity.y = PlayerVariables.Instance.Stats.JumpPower;
            Jumped?.Invoke();
        }

        #endregion

        #region Horizontal

        public void HandleDirection()
        {
            if (_frameInput.Move.x == 0)
            {
                var deceleration = _grounded ? PlayerVariables.Instance.Stats.GroundDeceleration : PlayerVariables.Instance.Stats.AirDeceleration;
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, 0, deceleration * Time.fixedDeltaTime);
            } 
            // else if (_stateManager.state == PlayerState.Stunned)
            // {
            //     _frameVelocity.x = 0f;
            // }
            else
            {
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, _frameInput.Move.x * PlayerVariables.Instance.Stats.MaxSpeed, PlayerVariables.Instance.Stats.Acceleration * Time.fixedDeltaTime);
            }
        }

        #endregion

        #region Gravity

        public void HandleGravity()
        {
            if (_grounded && _frameVelocity.y <= 0f)
            {
                _frameVelocity.y = PlayerVariables.Instance.Stats.GroundingForce;
            }
            else
            {
                var inAirGravity = PlayerVariables.Instance.Stats.FallAcceleration;
                if (_endedJumpEarly && _frameVelocity.y > 0) inAirGravity *= PlayerVariables.Instance.Stats.JumpEndEarlyGravityModifier;
                _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -PlayerVariables.Instance.Stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
            }
        }

        #endregion


        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("EscapeRout"))
            {
                PlayerVariables.Instance.Escape();
            }
        }

       
        public void ApplyMovement() => PlayerVariables.Instance.RigidBody2D.velocity = _frameVelocity;

        public void HaltHorizontalMomentum()
        {
            _frameInput.Move.x = 0f;
            _frameVelocity.x = 0f;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (PlayerVariables.Instance.Stats == null) Debug.LogWarning("Please assign a ScriptableStats asset to the Player Controller's Stats slot", this);
        }
#endif
    }

    public struct FrameInput
    {
        public bool JumpDown;
        public bool JumpHeld;
        public Vector2 Move;
    }

    public interface IPlayerController
    {
        public event Action<bool, float> GroundedChanged;

        public event Action Jumped;
        public Vector2 FrameInput { get; }
    }
}

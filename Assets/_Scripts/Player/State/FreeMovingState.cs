using System;
using _Scripts.Card;
using UnityEngine;

namespace _Scripts.Player.State
{

    public class FreeMovingState : IPlayerState
    {
        private PlayerStateManager _player;

        
        private BoxCollider2D _col;
        private PlayerStateManager _stateManager;
        
        private Vector2 _frameVelocity;
        private bool _cachedQueryStartInColliders;
        private float _time;

        public event Action<bool, float> GroundedChanged;
        public event Action Jumped;

        public void EnterState(PlayerStateManager player)
        {
            _player = player;
            _cachedQueryStartInColliders = Physics2D.queriesStartInColliders;
        }

        public void UpdateState()
        {
            _time += Time.deltaTime;
        }

        public void FixedUpdateState()
        {
            CheckCollisions();

            HandleJump();
            HandleDirection();
            HandleGravity();

            ApplyMovement();
        }

        public void ExitState() {}

        public void HandleInput()
        {
            PlayerStateManager.Instance.FrameInput = new FrameInput
            {
                JumpDown = _player.JumpPressed,
                JumpHeld = _player.JumpHeld,
                Move = _player.MovementInput
            };
            
            // Debug.Log(PlayerStateManager.Instance.FrameInput.Move);

            // Track the facing direction based on the last non-zero horizontal input
            if (PlayerStateManager.Instance.FrameInput.Move.x != 0)
            {
                // PlayerVariables.Instance.isFacingRight = _frameInput.Move.x > 0;
                if ((PlayerVariables.Instance.isFacingRight && PlayerStateManager.Instance.FrameInput.Move.x < 0) ||
                    (!PlayerVariables.Instance.isFacingRight && PlayerStateManager.Instance.FrameInput.Move.x > 0))
                {
                    PlayerVariables.Instance.FlipLocalScale();
                }

                // _stateManager.SetState(PlayerState.Moving);
            }
            else
            {
                // _stateManager.SetState(PlayerState.Idle);
            }

            if (PlayerVariables.Instance.Stats.SnapInput)
            {
                PlayerStateManager.Instance.FrameInput.Move.x = Mathf.Abs(PlayerStateManager.Instance.FrameInput.Move.x) < PlayerVariables.Instance.Stats.HorizontalDeadZoneThreshold
                    ? 0
                    : Mathf.Sign(PlayerStateManager.Instance.FrameInput.Move.x);
                PlayerStateManager.Instance.FrameInput.Move.y = Mathf.Abs(PlayerStateManager.Instance.FrameInput.Move.y) < PlayerVariables.Instance.Stats.VerticalDeadZoneThreshold
                    ? 0
                    : Mathf.Sign(PlayerStateManager.Instance.FrameInput.Move.y);
            }

            if (PlayerStateManager.Instance.FrameInput.JumpDown)
            {
                _jumpToConsume = true;
                _timeJumpWasPressed = _time;
            }
        }

        public void OnCollisionEnter2D(Collision2D col)
        {
        }
        
          #region Collisions

        private float _frameLeftGrounded = float.MinValue;
        private bool _grounded;

        public bool isGrounded()
        {
            return _grounded;
        }

        private void CheckCollisions()
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

        private bool HasBufferedJump => _bufferedJumpUsable && _time < _timeJumpWasPressed + PlayerVariables.Instance.Stats.JumpBuffer;
        private bool CanUseCoyote => _coyoteUsable && !_grounded && _time < _frameLeftGrounded + PlayerVariables.Instance.Stats.CoyoteTime;

        private void HandleJump()
        {
            if (!_endedJumpEarly && !_grounded && !PlayerStateManager.Instance.FrameInput.JumpHeld && PlayerVariables.Instance.RigidBody2D.velocity.y > 0) _endedJumpEarly = true;

            if (!_jumpToConsume && !HasBufferedJump) return;

            if (_grounded || CanUseCoyote) ExecuteJump();

            _jumpToConsume = false;
        }

        private void ExecuteJump()
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

        private void HandleDirection()
        {
            if (PlayerStateManager.Instance.FrameInput.Move.x == 0)
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
                _frameVelocity.x = Mathf.MoveTowards(_frameVelocity.x, PlayerStateManager.Instance.FrameInput.Move.x * PlayerVariables.Instance.Stats.MaxSpeed,
                    PlayerVariables.Instance.Stats.Acceleration * Time.fixedDeltaTime);
            }
        }

        #endregion

        #region Gravity

        private void HandleGravity()
        {
            if (_grounded && _frameVelocity.y <= 0f)
            {
                _frameVelocity.y = PlayerVariables.Instance.Stats.GroundingForce;
            }
            else
            {
                var inAirGravity = PlayerVariables.Instance.Stats.FallAcceleration;
                if (_endedJumpEarly && _frameVelocity.y > 0) inAirGravity *= PlayerVariables.Instance.Stats.JumpEndEarlyGravityModifier;
                _frameVelocity.y = Mathf.MoveTowards(_frameVelocity.y, -PlayerVariables.Instance.Stats.MaxFallSpeed,
                    inAirGravity * Time.fixedDeltaTime);
            }
        }

        #endregion

        private void ApplyMovement() => PlayerVariables.Instance.RigidBody2D.velocity = _frameVelocity;
    }
}
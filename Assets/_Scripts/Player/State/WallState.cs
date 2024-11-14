using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Player.State
{
    public class WallState : IPlayerState
    {
        private Coroutine _slideToLedgePosCoroutine;
        private bool _onLedge;
        public void EnterState()
        {
            PlayerAnimator.Instance.wallSlide();
            PlayerMovement.Instance.LerpVerticalMomentum();
            _onLedge = false;
        }

        public void UpdateState()
        {
            PlayerMovement.Instance.GatherInput();
            
            if (PlayerMovement.Instance.JumpDownFrameInput)
            {
                PlayerMovement.Instance.HandleWallJump();
                PlayerAnimator.Instance.endSlide();
                PlayerStateManager.Instance.TransitionToState(PlayerStateManager.Instance.FreeMovingState);
                return;
            }

            if (!_onLedge)
            {
                PlayerAnimator.Instance.endHang();
                PlayerMovement.Instance.WallSlideMovement();
            }
               
            
            PlayerMovement.Instance.HandleWallJump();
            CheckWallAndGroundConditions();
            CheckInputIsPresent();
            LedgeCheck();
        }

        public void FixedUpdateState()
        {
            PlayerMovement.Instance.ApplyMovement();
        }

        public void ExitState()
        {
            if (_slideToLedgePosCoroutine != null)
            {
                PlayerStateManager.Instance.StopCoroutine(_slideToLedgePosCoroutine);
                _slideToLedgePosCoroutine = null;
            }

            _onLedge = false;

            // Set the last wall hang time to prevent immediate re-entry into WallState
            PlayerStateManager.Instance.SetLastWallHangTime(Time.time);
            PlayerAnimator.Instance.endSlide();
            PlayerAnimator.Instance.endHang();
        }

        /*
         * To stay on a wall the player should be pushing their left stick in the direction of the wall.
         * When they release it they should exit wall state and begin a free fall.
         */
        private static void CheckInputIsPresent()
        {
            if (PlayerVariables.Instance.isFacingRight && PlayerMovement.Instance.FrameInput.x > 0) return;
            if (!PlayerVariables.Instance.isFacingRight && PlayerMovement.Instance.FrameInput.x < 0) return;
            PlayerStateManager.Instance.TransitionToState(PlayerStateManager.Instance.FreeMovingState);
        }

        private static void CheckWallAndGroundConditions()
        {
            if (!PlayerMovement.Instance.IsGrounded() && PlayerMovement.Instance.IsWalled()) return;
            PlayerStateManager.Instance.TransitionToState(PlayerStateManager.Instance.FreeMovingState);
        }

        /*
         * Checks if the top of the player's collider is touching the wall.
         * If it is not then the player is on the slide of a ledge and needs to fall until the ray hits the edge of the wall
         * then hang there
         */
        private void LedgeCheck()
        {
            var rayOrigin = (Vector2)PlayerVariables.Instance.Collider2D.bounds.center + Vector2.up * PlayerVariables.Instance.Collider2D.bounds.extents.y;

            var direction = PlayerVariables.Instance.isFacingRight ? Vector2.right : Vector2.left;

            var hit = Physics2D.Raycast(rayOrigin, direction, 0.5f, PlayerMovement.Instance.wallLayer);

            // If a hit was detected the player is NOT on a ledge
            if (hit.collider != null)
            {
                // Release ledge hold with downward joystick input
                if (PlayerMovement.Instance.FrameInput.y < 0)
                {
                    _onLedge = false;
                    
                }
                
                return;
            }
            PlayerAnimator.Instance.ledgeHang();
            _onLedge = true;
            
            if (_slideToLedgePosCoroutine != null) return;
            
            PlayerMovement.Instance.HaltVerticalMomentum(); 
            _slideToLedgePosCoroutine = PlayerStateManager.Instance.StartCoroutine(LerpToLedgeHang());
        }

        /*
         * After _onLedge is set to true the player needs to be pushed down so that they are at equal height with the
         * ledge they are hanging on
         */
        private IEnumerator LerpToLedgeHang()
        {
            while (true)
            {
                var rayOrigin = (Vector2)PlayerVariables.Instance.Collider2D.bounds.center +
                                Vector2.up * PlayerVariables.Instance.Collider2D.bounds.extents.y;

                var direction = PlayerVariables.Instance.isFacingRight ? Vector2.right : Vector2.left;

                var hit = Physics2D.Raycast(rayOrigin, direction, 0.5f, PlayerMovement.Instance.wallLayer);

                if (hit.collider != null)
                {
                    // The ray hit the wall; exit the loop
                    break;
                }

                PlayerMovement.Instance.WallSlideMovement();

                // Break the coroutine on downward joystick input
                if (PlayerMovement.Instance.FrameInput.y < 0)
                {
                    yield break;
                }

                if (PlayerMovement.Instance.IsGrounded() || !PlayerMovement.Instance.IsWalled())
                {
                    yield break;
                }

                yield return null;
            }

            // After exiting the loop ensure vertical momentum is halted
            PlayerMovement.Instance.HaltVerticalMomentum();
        }
    }
}
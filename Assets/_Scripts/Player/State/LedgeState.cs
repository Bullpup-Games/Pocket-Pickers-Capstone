using System.Collections;
using _Scripts.Card;
using UnityEngine;

namespace _Scripts.Player.State
{
    public class LedgeState : IPlayerState
    {
        private Coroutine _slideToLedgePosCoroutine;
        public void EnterState()
        {
            PlayerMovement.Instance.HaltHorizontalMomentum();
            
            PlayerMovement.Instance.HaltVerticalMomentum();

            CardManager.Instance.Teleport += teleported;
            
            _slideToLedgePosCoroutine = PlayerStateManager.Instance.StartCoroutine(LerpToLedgeHangPosition());
            
            PlayerAnimator.Instance.ledgeHang();
        }

        public void UpdateState()
        {
            if (_slideToLedgePosCoroutine is null)
                PlayerMovement.Instance.HaltVerticalMomentum();
            
            // Release ledge hold with downward joystick input
            if (PlayerMovement.Instance.FrameInput.y < 0)
                PlayerStateManager.Instance.TransitionToState(PlayerStateManager.Instance.FreeMovingState);
            
            CheckInputIsPresent();
            if (!PlayerMovement.Instance.IsWalled())
            {
                PlayerStateManager.Instance.TransitionToState(PlayerStateManager.Instance.FreeMovingState);
            }
        }

        public void FixedUpdateState()
        {
            // Apply gravity
            PlayerMovement.Instance.ApplyMovement();
        }
        public void ExitState()
        {
            Debug.Log("Trying to exit ledge state");
            if (_slideToLedgePosCoroutine is null) return;
            PlayerStateManager.Instance.StopCoroutine(_slideToLedgePosCoroutine);
            _slideToLedgePosCoroutine = null;
            
            PlayerStateManager.Instance.setLastLedgeHangTime(Time.time);
            
            PlayerAnimator.Instance.endHang();
        }
        
        
        private void CheckInputIsPresent()
        {
            if (PlayerVariables.Instance.isFacingRight && PlayerMovement.Instance.FrameInput.x > 0) return;
            if (!PlayerVariables.Instance.isFacingRight && PlayerMovement.Instance.FrameInput.x < 0) return;
            PlayerStateManager.Instance.TransitionToState(PlayerStateManager.Instance.FreeMovingState);
        }

        public void teleported(Vector2 catchEvent)
        {
            PlayerStateManager.Instance.TransitionToState(PlayerStateManager.Instance.FreeMovingState);
        }
        
        private IEnumerator LerpToLedgeHangPosition()
        {
            // PlayerAnimator.Instance.ledgeHang();
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
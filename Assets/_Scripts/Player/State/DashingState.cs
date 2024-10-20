using System.Collections;
using UnityEngine;

namespace _Scripts.Player.State
{
    public class DashingState : IPlayerState
    {
        private Coroutine _dashCoroutine;
        private Vector2 _dashDirection;
        public void EnterState()
        {
            // Ensure we have the latest input
            PlayerMovement.Instance.GatherInput();

            // Capture the dash direction from the movement input
            var inputDirection = PlayerMovement.Instance.FrameInput;

            // If the input direction magnitude is small, default to facing direction
            if (inputDirection.sqrMagnitude < 0.01f)
            {
                // Default to facing right or left based on facing direction
                inputDirection = PlayerVariables.Instance.isFacingRight ? Vector2.right : Vector2.left;
            }
            else
            {
                // Normalize the input direction
                inputDirection.Normalize();
            }

            // Store the dash direction
            _dashDirection = inputDirection;

            // Set the dash direction in PlayerMovement
            PlayerMovement.Instance.DashDirection = _dashDirection;

            // Start the dash coroutine
            _dashCoroutine = PlayerStateManager.Instance.StartCoroutine(DashCoroutine());
        }

        public void UpdateState()
        {
            
        }

        public void FixedUpdateState()
        {
            PlayerMovement.Instance.CheckCollisions();
            // PlayerMovement.Instance.HandleGravity();
            PlayerMovement.Instance.ApplyDashMovement();
        }

        public void ExitState()
        {
            if (_dashCoroutine == null) return;
            PlayerStateManager.Instance.StopCoroutine(_dashCoroutine);
            _dashCoroutine = null;
        }

        private IEnumerator DashCoroutine()
        {
            yield return new WaitForSeconds(PlayerVariables.Instance.Stats.DashDuration);
            PlayerStateManager.Instance.TransitionToState(PlayerStateManager.Instance.FreeMovingState);
        }
    }
}
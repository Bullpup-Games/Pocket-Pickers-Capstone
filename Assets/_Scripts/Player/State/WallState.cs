using UnityEngine;

namespace _Scripts.Player.State
{
    public class WallState : IPlayerState
    {
        public void EnterState()
        {
            PlayerMovement.Instance.LerpVerticalMomentum();
        }

        public void UpdateState()
        {
            PlayerMovement.Instance.GatherInput();
            PlayerMovement.Instance.WallSlideMovement();
            
            CheckWallAndGroundConditions();
            CheckInputIsPresent();
        }

        public void FixedUpdateState()
        {
            PlayerMovement.Instance.ApplyMovement();
        }

        public void ExitState()
        {
            
        }

        /*
         * To stay on a wall the player should be pushing their left stick in the direction of the wall.
         * When they release it they should exit wall state and begin a free fall.
         */
        private void CheckInputIsPresent()
        {
            if (PlayerVariables.Instance.isFacingRight && PlayerMovement.Instance.FrameInput.x > 0) return;
            if (!PlayerVariables.Instance.isFacingRight && PlayerMovement.Instance.FrameInput.x < 0) return;
            PlayerStateManager.Instance.TransitionToState(PlayerStateManager.Instance.FreeMovingState);
        }

        private void CheckWallAndGroundConditions()
        {
            if (!PlayerMovement.Instance.IsGrounded() && PlayerMovement.Instance.IsWalled()) return;
            PlayerStateManager.Instance.TransitionToState(PlayerStateManager.Instance.FreeMovingState);
        }
    }
}
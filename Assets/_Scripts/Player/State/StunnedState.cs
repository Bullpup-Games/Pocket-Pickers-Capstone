using UnityEngine;

namespace _Scripts.Player.State
{
    public class StunnedState : IPlayerState
    {
        private float _stunDuration = 2f;
        private float _stunTimer;

        public void EnterState()
        {
            PlayerMovement.Instance.HaltHorizontalMomentum();
        }

        public void UpdateState()
        {
            PlayerMovement.Instance.HandleGravity();
        }

        public void FixedUpdateState()
        {
            // Apply gravity
            PlayerMovement.Instance.ApplyMovement();
        }
        public void ExitState()
        {
        }
    }
}
using UnityEngine;

namespace _Scripts.Player.State
{
    public class StunnedState : IPlayerState
    {
        private PlayerStateManager _player;
        public void EnterState(PlayerStateManager player)
        {
            _player = player;
            Debug.Log("entered stun state");
        }

        public void UpdateState()
        {
            
        }

        public void FixedUpdateState()
        {
            
        }

        public void ExitState()
        {
            
        }

        public void HandleInput()
        {
            
        }

        public void OnCollisionEnter2D(Collision2D col)
        {
            
        }  
    }
}
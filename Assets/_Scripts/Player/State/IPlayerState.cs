using UnityEngine;

namespace _Scripts.Player.State
{
    public interface IPlayerState
    {
        void EnterState(PlayerStateManager player);
        void UpdateState();
        void FixedUpdateState();
        void ExitState();
        void HandleInput();
        void OnCollisionEnter2D(Collision2D col);
    }
}
using _Scripts.Player;
using Unity.VisualScripting;
using UnityEngine;

namespace _Scripts.Enemies.State
{
    public class DisabledState : IEnemyState
    {
        private EnemyStateManager _enemy;

        public void EnterState(EnemyStateManager enemy)
        {
            _enemy = enemy;
            _enemy.StopMoving();
            _enemy.Rigidbody2D.isKinematic = true;
            _enemy.Collider2D.enabled = false;
            Physics2D.IgnoreCollision(PlayerVariables.Instance.Collider2D, _enemy.Collider2D);
        }

        public void UpdateState()
        {
            // TODO: Play disabled animation
            _enemy.StopMoving();
            _enemy.Settings.HandleGroundDetection();
            _enemy.Settings.HandleGravity();
            if (_enemy.Settings.IsGrounded())
            {
                _enemy.StopFalling();
            }
            
            Physics2D.IgnoreCollision(PlayerVariables.Instance.Collider2D, _enemy.Collider2D);
        }

        public void ExitState() {}

        public void OnCollisionEnter2D(Collision2D col)
        {
            Physics2D.IgnoreCollision(PlayerVariables.Instance.Collider2D, _enemy.Collider2D);
        }
        
        public void OnCollisionStay2D(Collision2D col) {}
    }
}
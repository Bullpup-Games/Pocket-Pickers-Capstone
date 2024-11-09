using _Scripts.Player;
using UnityEngine;

namespace _Scripts.Enemies.Guard.State
{
    public class GuardDisabledState : IEnemyState<GuardStateManager>
    {
        private GuardStateManager _enemy;

        public void EnterState(GuardStateManager enemy)
        {
            _enemy = enemy;
            _enemy.StopMoving();
            _enemy.Rigidbody2D.isKinematic = true;
            Physics2D.IgnoreCollision(PlayerVariables.Instance.Collider2D, _enemy.Collider2D);
            _enemy.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 90f)); // TODO: Change eventually
            _enemy.Settings.removeListeners();
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
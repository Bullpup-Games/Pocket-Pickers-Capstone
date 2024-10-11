using System.Collections;
using _Scripts.Player;
using UnityEngine;

namespace _Scripts.Enemies.State
{
    public class StunnedState : IEnemyState
    {
        private EnemyStateManager _enemy;
        private Coroutine _stunCoroutine;
        private Collider2D _col;
        public void EnterState(EnemyStateManager enemy)
        {
            _enemy = enemy;
            _enemy.StopMoving();
            _enemy.Rigidbody2D.isKinematic = true;
            _enemy.Collider2D.enabled = false;
            _stunCoroutine = _enemy.StartCoroutine(StunDuration());
        }

        public void UpdateState()
        {
            // TODO: play stunned animation
            _enemy.StopMoving();
            _enemy.Settings.HandleGroundDetection();
            _enemy.Settings.HandleGravity();
            if (_enemy.Settings.IsGrounded())
            {
                _enemy.StopFalling();
            }
            
            Physics2D.IgnoreCollision(PlayerVariables.Instance.Collider2D, _enemy.Collider2D);
        }

        public void ExitState()
        {
            _enemy.Rigidbody2D.isKinematic = false;
            _enemy.Collider2D.enabled = true;
            if (_stunCoroutine != null)
                _enemy.StopCoroutine(_stunCoroutine);
        }

        public void OnCollisionEnter2D(Collision2D col)
        {
            Physics2D.IgnoreCollision(PlayerVariables.Instance.Collider2D, _enemy.Collider2D);
        }
        
        public void OnCollisionStay2D(Collision2D col) {}

        private IEnumerator StunDuration()
        {
            yield return new WaitForSeconds(_enemy.Settings.stunLength);

            // Transition back to Searching state
            _enemy.TransitionToState(_enemy.SearchingState);
        }
    }
}
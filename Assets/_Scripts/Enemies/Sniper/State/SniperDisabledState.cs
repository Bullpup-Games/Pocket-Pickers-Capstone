using System.Collections;
using _Scripts.Player;
using UnityEngine;

namespace _Scripts.Enemies.Sniper.State
{
    public class SniperDisabledState : IEnemyState<SniperStateManager>
    {
        private SniperStateManager _enemy;
        private Coroutine _timeout;

        public void EnterState(SniperStateManager enemy)
        {
            _enemy = enemy;
            Physics2D.IgnoreCollision(PlayerVariables.Instance.Collider2D, _enemy.Collider2D);
            // _enemy.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 90f)); // TODO: Change eventually
            
            _timeout = _enemy.StartCoroutine(Timeout());
        }

        public void UpdateState() {}

        public void ExitState()
        {
            if (_timeout is not null)
            {
                _enemy.StopCoroutine(_timeout);
                _timeout = null;
            }
            
            Physics2D.IgnoreCollision(PlayerVariables.Instance.Collider2D, _enemy.Collider2D, false);
        }

        public void OnCollisionEnter2D(Collision2D col) {}

        public void OnCollisionStay2D(Collision2D col) {}
        
        private IEnumerator Timeout()
        {
            yield return new WaitForSeconds(_enemy.Settings.disabledTimeout);
            _enemy.TransitionToState(_enemy.PatrollingState);
        }
    }
}
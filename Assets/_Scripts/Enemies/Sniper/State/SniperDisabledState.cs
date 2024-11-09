using _Scripts.Player;
using UnityEngine;

namespace _Scripts.Enemies.Sniper.State
{
    public class SniperDisabledState : IEnemyState<SniperStateManager>
    {
        private SniperStateManager _enemy;
        public void EnterState(SniperStateManager enemy)
        {
            _enemy = enemy;
            Physics2D.IgnoreCollision(PlayerVariables.Instance.Collider2D, _enemy.Collider2D);
            _enemy.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 90f)); // TODO: Change eventually
        }

        public void UpdateState() {}

        public void ExitState() {}

        public void OnCollisionEnter2D(Collision2D col) {}

        public void OnCollisionStay2D(Collision2D col) {}
    }
}
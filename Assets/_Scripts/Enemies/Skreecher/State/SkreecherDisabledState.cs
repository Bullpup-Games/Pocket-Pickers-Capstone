using _Scripts.Player;
using UnityEngine;

namespace _Scripts.Enemies.Skreecher.State
{
    public class SkreecherDisabledState : IEnemyState<SkreecherStateManager>
    {
        private SkreecherStateManager _enemy;
        public void EnterState(SkreecherStateManager enemy)
        {
            _enemy = enemy;
            Physics2D.IgnoreCollision(_enemy.Collider2D, PlayerVariables.Instance.Collider2D);
            _enemy.Collider2D.enabled = false;
        }

        public void UpdateState() {}

        public void ExitState() {}

        public void OnCollisionEnter2D(Collision2D col)
        {
            if (col.gameObject.layer == LayerMask.GetMask("Card"))
                Physics2D.IgnoreCollision(_enemy.Collider2D, col.collider);
        }

        public void OnCollisionStay2D(Collision2D col) {}  
    
    }
}
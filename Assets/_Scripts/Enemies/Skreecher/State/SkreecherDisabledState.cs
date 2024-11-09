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
            // Physics2D.IgnoreCollision(_enemy.Collider2D, PlayerVariables.Instance.Collider2D);
            // _enemy.Collider2D.enabled = false;
            
            // TODO: Eventually I think a teleport or poof effect on the skreecher would look good as the disabled anim?
            // Leaving it hanging on the ceiling doesn't seem right, neither does letting it fall. TP away seems like the best solution.
            // Maybe once you disable a skreecher it just teleports to a new position instead of disappearing?
            _enemy.gameObject.SetActive(false);
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
using UnityEngine;

namespace _Scripts.Enemies.Sniper.State
{
    public class SniperDisabledState : IEnemyState<SniperStateManager>
    {
        public void EnterState(SniperStateManager enemy) {}

        public void UpdateState() {}

        public void ExitState() {}

        public void OnCollisionEnter2D(Collision2D col) {}

        public void OnCollisionStay2D(Collision2D col) {}
    }
}
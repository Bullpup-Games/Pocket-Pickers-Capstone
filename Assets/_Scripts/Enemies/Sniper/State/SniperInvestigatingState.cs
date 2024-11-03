using System.Collections;
using UnityEngine;

namespace _Scripts.Enemies.Sniper.State
{
    public class SniperInvestigatingState : IEnemyState<SniperStateManager>
    {
        private SniperStateManager _enemy;
        private Coroutine _falseTriggerInvestigationCoroutine;
        public void EnterState(SniperStateManager enemy)
        {
            _enemy = enemy;
            _falseTriggerInvestigationCoroutine = _enemy.StartCoroutine(LookAtFalseTriggerPosition());
        }

        public void UpdateState() {}

        public void ExitState()
        {
            if (_falseTriggerInvestigationCoroutine is null) return;
            _enemy.StopCoroutine(_falseTriggerInvestigationCoroutine);
            _falseTriggerInvestigationCoroutine = null;
        }

        public void OnCollisionEnter2D(Collision2D col) {}

        public void OnCollisionStay2D(Collision2D col) {}

        private IEnumerator LookAtFalseTriggerPosition()
        {
            yield return new WaitForSeconds(_enemy.Settings.investigationTime);
            _enemy.TransitionToState(_enemy.PatrollingState);
        }
    }
}
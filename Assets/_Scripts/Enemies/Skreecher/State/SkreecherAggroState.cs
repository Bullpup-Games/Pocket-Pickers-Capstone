using System.Collections;
using System.Collections.Generic;
using _Scripts.Enemies.Guard.State;
using _Scripts.Enemies.Sniper.State;
using UnityEngine;

namespace _Scripts.Enemies.Skreecher.State
{
    public class SkreecherAggroState : IEnemyState<SkreecherStateManager>
    {
        private SkreecherStateManager _enemy;
        private Coroutine _screechCoroutine;
        private List<Collider2D> _enemiesInScreechRange;
        public void EnterState(SkreecherStateManager enemy)
        {
            _enemy = enemy;
            _screechCoroutine = _enemy.StartCoroutine(PerformScreech());
            Debug.Log("Enter Aggro");
            _enemiesInScreechRange = _enemy.FindAllEnemiesInRange();
            Debug.Log("# of Enemies in Range:" + _enemiesInScreechRange.Count);

            foreach (var col in _enemiesInScreechRange)
            {
                var guardStateManager = col.GetComponent<IEnemyStateManager<GuardStateManager>>();
                if (guardStateManager is not null)
                {
                    Debug.Log("GUARD STATE MANAGER FOUND");
                    guardStateManager.AlertFromSkreecher();
                }
                var sniperStateManager = col.GetComponent<IEnemyStateManager<SniperStateManager>>();
                if (sniperStateManager is not null)
                {
                    sniperStateManager.AlertFromSkreecher();
                }
            }
        }

        public void UpdateState() {}

        public void ExitState()
        {
            if (_screechCoroutine is null) return;
            _enemy.StopCoroutine(_screechCoroutine);
            _screechCoroutine = null;
        }

        public void OnCollisionEnter2D(Collision2D col) {}

        public void OnCollisionStay2D(Collision2D col) {}

        private IEnumerator PerformScreech()
        {
            // TODO: Play animation & sound
            yield return new WaitForSeconds(_enemy.Settings.screechTime);
            _enemy.TransitionToState(_enemy.DetectingState);
        }
    }
}
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts.Enemies.Sniper.State
{
    public class SniperChargingState : IEnemyState<SniperStateManager>
    {
        private SniperStateManager _enemy;
        private Coroutine _chargeCoroutine;
        public void EnterState(SniperStateManager enemy)
        {
            Debug.Log("Enter Charging State");
            _enemy = enemy;
            _chargeCoroutine = _enemy.StartCoroutine(ChargeShot());
        }

        public void UpdateState() {}

        public void ExitState()
        {
            if (_chargeCoroutine is not null)
            {
                _enemy.StopCoroutine(_chargeCoroutine);
                _chargeCoroutine = null;
            }
        }

        public void OnCollisionEnter2D(Collision2D col) {}

        public void OnCollisionStay2D(Collision2D col) {}

        private IEnumerator ChargeShot() 
        {
            yield return new WaitForSeconds(_enemy.Settings.chargeTime);
            FireShot();
        }

        private void FireShot()
        {
            Debug.Log("Shot fired.");
            if (_enemy.IsPlayerDetected())
            {
                // TODO: Eventually this will need to call a full cleanup of the level. For now just restart the scene
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            else
            {
                Debug.Log("Switching to reload");
                _enemy.TransitionToState(_enemy.ReloadingState); 
            }
        }
    }
}
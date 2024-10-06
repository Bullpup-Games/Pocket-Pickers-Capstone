using System.Collections;
using UnityEngine;

namespace _Scripts.Enemies
{
    public class EnemySearching : MonoBehaviour
    {
        [SerializeField] private float totalSearchTime = 16f;
        [SerializeField] private float searchIntervalTime = 4f;
        private EnemyStateManager _stateManager;
        private EnemySettings _settings;
        private bool _isSearching;
        private float _elapsedTime;

        private void Awake()
        {
            _stateManager = GetComponent<EnemyStateManager>();
            _settings = GetComponent<EnemySettings>();
        }

        private void Update()
        {
            if (_stateManager.state != EnemyState.Searching && _isSearching)
            {
                StopCoroutine(SearchRoutine());
                _elapsedTime -= _elapsedTime;
                return;
            }

            if (_stateManager.state != EnemyState.Searching || _isSearching) return;
            _elapsedTime -= _elapsedTime;
            StartCoroutine(SearchRoutine());
        }

        private IEnumerator SearchRoutine()
        {
            _isSearching = true;
            yield return new WaitForSeconds(searchIntervalTime);

            while (_elapsedTime < totalSearchTime && _stateManager.state == EnemyState.Searching)
            {
                // Flip the enemy to look in the other direction
                _settings.FlipLocalScale();

                // Wait for the search interval
                var interval = Mathf.Min(searchIntervalTime, totalSearchTime - _elapsedTime);
                yield return new WaitForSeconds(interval);

                _elapsedTime += interval;
            }

            // Check if the enemy is still in the Searching state before changing to Returning
            // Otherwise, they are aggro or stunned most likely, in which case just return
            if (_stateManager.state == EnemyState.Searching)
            {
                _stateManager.SetState(EnemyState.Returning);
            }

            _isSearching = false;
            yield return null;
        }
    }
}
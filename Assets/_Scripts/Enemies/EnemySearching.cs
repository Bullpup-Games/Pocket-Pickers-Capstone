using System;
using System.Collections;
using UnityEngine;

namespace _Scripts.Enemies
{
    public class EnemySearching : MonoBehaviour
    {
        [SerializeField] private float totalSearchTime = 11f;
        [SerializeField] private float searchIntervalTime = 1.5f;
        private EnemyStateManager _stateManager;
        private EnemySettings _settings;
        private bool isLooking;

        private void Awake()
        {
            _stateManager = GetComponent<EnemyStateManager>();
            _settings = GetComponent<EnemySettings>();
        }

        private void Update()
        {
            if (_stateManager.state != EnemyState.Searching) return;
            
            
        }

        private IEnumerator IsLooking()
        {
            isLooking = true;
            yield return new WaitForSeconds(searchIntervalTime);
            isLooking = false;
        }
        
    }
}
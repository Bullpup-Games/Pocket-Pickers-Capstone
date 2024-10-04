using System;
using UnityEngine;

namespace _Scripts.Enemies
{
    public class DetectionLogic : MonoBehaviour
    {
        private float _detectionTimer = 0f;
        
        private EnemySettings _settings;
        private EnemyStateManager _stateManager;
        private Rigidbody2D _rb;
        private IViewType _view;
        private void Awake()
        {
            _settings = GetComponent<EnemySettings>();
            _stateManager = GetComponent<EnemyStateManager>();
            _rb = GetComponent<Rigidbody2D>();
            _view = GetComponent<IViewType>();
        }

        private void Update()
        {
            _view.SetView();
            DetectEnemy();
        }

        // Handle the detection timer, taking the enemy from detecting to aggro if limit is reached
        private void DetectEnemy()
        {
            if (_stateManager.state != EnemyState.Detecting)
            {
                _detectionTimer = 0f;
                return;
            }

            _detectionTimer += Time.deltaTime * _settings.DetectionModifier;

            if (!(_detectionTimer >= _settings.baseDetectionTime)) return;
            
            // Switch to the agro state after filling detection meter
            _stateManager.SetState(EnemyState.Aggro);
            Debug.Log("Enemy is now aggro!");
        }
    }
}

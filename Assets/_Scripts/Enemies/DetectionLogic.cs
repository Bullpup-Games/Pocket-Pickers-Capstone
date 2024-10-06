using System;
using _Scripts.Enemies.ViewTypes;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

namespace _Scripts.Enemies
{
    public class DetectionLogic : MonoBehaviour
    {
        private float _detectionTimer = 0f;
        
        private EnemySettings _settings;
        private EnemyStateManager _stateManager;
        private Rigidbody2D _rb;
        private IViewType[] _viewTypes;
        
        private void Awake()
        {
            _settings = GetComponent<EnemySettings>();
            _stateManager = GetComponent<EnemyStateManager>();
            _rb = GetComponent<Rigidbody2D>();
            _viewTypes = GetComponents<IViewType>();

        }

        private void Update()
        {
            foreach (var viewType in _viewTypes)
            {
                viewType.SetView();
            }
            
            if (_stateManager.state != EnemyState.Detecting)
            {
                _detectionTimer = 0f;
            }
        }

        private void OnEnable()
        {
            foreach (var viewType in _viewTypes)
            {
                viewType.PlayerDetected += HandleDetectionTimer;
            } 
        }

        // Handle the detection timer, taking the enemy from detecting to aggro if limit is reached
        private void HandleDetectionTimer(bool quickDetect)
        {
            _detectionTimer += Time.deltaTime * _settings.DetectionModifier;

            if (!(_detectionTimer >= Mathf.Abs(_settings.baseDetectionTime)) && !quickDetect) return;
            if (!(_detectionTimer >= Mathf.Abs(_settings.baseDetectionTime / 4f)) && quickDetect) return;
            
            // Switch to the agro state after filling detection meter
            _stateManager.SetState(EnemyState.Aggro);
            // Debug.Log("Enemy is now aggro!");
        }
    }
}

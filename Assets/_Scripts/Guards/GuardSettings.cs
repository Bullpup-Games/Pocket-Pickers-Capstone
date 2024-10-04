using System;
using UnityEngine;

namespace _Scripts.Guards
{
    public class GuardSettings : MonoBehaviour
    {
        public bool isFacingRight = true;

        private float _detectionSpeed = 1.0f;
        public event Action<float> OnDetectionSpeedChanged;
        public float DetectionSpeed
        {
            get => _detectionSpeed;
            set
            {
                if (Math.Abs(_detectionSpeed - value) > 0)
                {
                    _detectionSpeed = value;
                    OnDetectionSpeedChanged?.Invoke(_detectionSpeed); // Trigger event when detectionSpeed is modified
                }
            }
        }

        private float _viewModifier = 1.0f;
        public event Action<float> OnViewModifierChanged;
        public float ViewModifier
        {
            get => _viewModifier;
            set
            {
                if (!(Math.Abs(_viewModifier - value) > 0)) return;
                _viewModifier = value;
                OnViewModifierChanged?.Invoke(_viewModifier);  // Trigger event when viewModifier is modified
            }
        }

        private const float Tolerance = 0.1f;
        private void Update()
        {
            // If the player's local state is 1 they're facing right, -1 left
            isFacingRight = !(Math.Abs(gameObject.transform.localScale.x - 1) > Tolerance);
        }

        private void Start()
        {
            // Log functions for testing, can be removed whenever
            OnDetectionSpeedChanged += LogDetectionSpeedChange;
            OnViewModifierChanged += LogViewModifierChange;
        }

        private void LogDetectionSpeedChange(float newSpeed)
        {
            Debug.Log($"Detection speed changed to {newSpeed}");
        }

        private void LogViewModifierChange(float newModifier)
        {
            Debug.Log($"View modifier changed to {newModifier}");
        }
    }
}

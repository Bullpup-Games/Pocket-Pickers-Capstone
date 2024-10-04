using System;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.XR;

namespace _Scripts.Guards
{
    public class GuardSettings : MonoBehaviour
    {
        public bool isFacingRight = true;


        private void Update()
        {
            // If the player's local state is 1 they're facing right, -1 left
            isFacingRight = !(Math.Abs(gameObject.transform.localScale.x - 1) > 0.1f);
        }

        #region SinModifiers
        private float _detectionSpeed = 1.0f;
        private float _viewModifier = 1.0f;
        public event Action<float> OnDetectionSpeedChanged;
        public event Action<float> OnViewModifierChanged;
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
        #endregion
    }
}

using System;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.XR;

namespace _Scripts.Enemies
{
    public class EnemySettings : MonoBehaviour
    {
        public bool isFacingRight = true;
        [Tooltip("Amount of time (in seconds) it takes to fully detect the player and enter aggro")]
        public float baseDetectionTime = 4.0f;
        public float closeDetectionTime = 1.0f;
        
        private EnemyStateManager _stateManager;
        private Rigidbody2D _rb;
        private BoxCollider2D _col;

        private void Awake()
        {
            _stateManager = GetComponent<EnemyStateManager>();
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<BoxCollider2D>();
        }

        private void Update()
        {
            // If the player's local state is 1 they're facing right, -1 left
            isFacingRight = !(Math.Abs(gameObject.transform.localScale.x - 1) > 0.1f);
            
            // If the enemy is disabled they should just stop at their current position and be immovable for now
            if (_stateManager.state == EnemyState.Disabled)
            {
                // TODO: Maybe change this eventually?
                _rb.isKinematic = true;
                _col.isTrigger = true;
            } 
        }
        
        // Flip the entity's sprite by inverting the X scaling
        public void FlipLocalScale()
        {
            // I don't know why the transformCopy needs to exist but Unity yelled at me when I didn't have it so here it sits..
            var transformCopy = transform;
            var localScale = transformCopy.localScale;
            localScale.x *= -1;
            transformCopy.localScale = localScale;
        }

        #region SinModifiers
        private float _detectionModifier = 1.0f; // Speed modifier for detecting the player
        private float _viewModifier = 1.0f; // View width, radius, length, etc modifier for detecting the player
        public event Action<float> OnDetectionSpeedChanged;
        public event Action<float> OnViewModifierChanged;
        public float DetectionModifier
        {
            get => _detectionModifier;
            set
            {
                if (!(Math.Abs(_detectionModifier - value) > 0)) return;
                _detectionModifier = value;
                OnDetectionSpeedChanged?.Invoke(_detectionModifier); // Trigger event when detectionSpeed is modified
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

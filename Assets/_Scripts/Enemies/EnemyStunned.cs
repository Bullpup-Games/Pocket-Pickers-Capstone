using System;
using System.Collections;
using UnityEngine;

namespace _Scripts.Enemies
{
    public class EnemyStunned : MonoBehaviour
    {
        [Tooltip("The amount of time (in seconds) that the stun should last"), Range(0.1f, 15f)]
        public float stunLength = 3.0f;
        private EnemyStateManager _stateManager;
        private Rigidbody2D _rb;
        private bool _isInStunCoroutine = false;
        private void Awake()
        {
            _stateManager = GetComponent<EnemyStateManager>();
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            // Halt x velocity while stunned 
            if (_stateManager.state == EnemyState.Stunned)
            {
                _rb.velocity = new Vector2(0f, _rb.velocity.y);
                // TODO: Maybe stop player collisions while stunned, we'll see after testing
            }
            
            if (_stateManager.state == EnemyState.Stunned && !_isInStunCoroutine)
            {
                StartCoroutine(HandleStun());
            }
        }

        private IEnumerator HandleStun()
        {
            _isInStunCoroutine = true;
            yield return new WaitForSeconds(stunLength);
            
            _isInStunCoroutine = false;
            
            // If the enemy was hit by the card during the stun timer they should not start searching again
            if (_stateManager.state != EnemyState.Disabled)
            {
                _stateManager.SetState(EnemyState.Searching);
            }
            
            yield return null;
        }
    }
}
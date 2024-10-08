using System;
using System.Collections;
using _Scripts.Enemies.ViewTypes;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Enemies
{
    public class EnemyStateManager : MonoBehaviour
    {
        public EnemyState state;
        private IViewType[] _viewTypes;
        private Rigidbody2D _rb;
        private bool _waiting;
        public void SetState(EnemyState newState)
        {
            this.state = newState;
        }
        
        private void Awake()
        {
            _viewTypes = GetComponents<IViewType>();
            _rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            foreach (var viewType in _viewTypes)
            {
                viewType.PlayerDetected += HandlePlayerSighting;
                viewType.NoPlayerDetected += HandleNoPlayerSighting;
            }
        }

        private void OnDisable()
        {
            foreach (var viewType in _viewTypes)
            {
                viewType.PlayerDetected -= HandlePlayerSighting;
                viewType.NoPlayerDetected -= HandleNoPlayerSighting;
            }
        }

        private void Update()
        {
            if (state == EnemyState.Disabled)
            {
                _rb.velocity = Vector2.zero;
            }
        }

        private void HandlePlayerSighting(bool quickDetect, float distanceModifier) // params not needed here, needed elsewhere as part of event
        {
            var spotted = false;
            
            foreach (var viewType in _viewTypes)
            {
                if (viewType.IsPlayerDetectedThisFrame() == true)
                {
                    spotted = true;
                }
            }

            if (!spotted) return;
            
            switch (state)
            {
                case EnemyState.Patrolling:
                    state = EnemyState.Detecting;
                    return;
                case EnemyState.Detecting:
                    return;
                case EnemyState.Aggro:
                    return;
                case EnemyState.Searching:
                    state = EnemyState.Aggro;
                    return;
                case EnemyState.Stunned:
                    return;
                case EnemyState.Returning:
                    state = EnemyState.Aggro;
                    return;
            }
        }

        private void HandleNoPlayerSighting()
        {
            foreach (var viewType in _viewTypes)
            {
                if (viewType.IsPlayerDetectedThisFrame() == true)
                {
                    return;
                }
            }
            
            // Debug.Log("No Player Sighted");
            switch (state)
            {
                case EnemyState.Patrolling:
                    return;
                case EnemyState.Detecting:
                    if (!_waiting) StartCoroutine(WaitBeforeSwitchingBackToPatrol());
                    return;
                case EnemyState.Aggro:
                    return;
                case EnemyState.Searching:
                    return;
                case EnemyState.Stunned:
                    return;
            }
        }

        /*
         * Called if the enemy is detecting the player and they step out of view.
         * Instead of immediately returning to their patrol state the guard should keep looking in direction
         * of the player's light sighting for a short time before returning.
         */
        private IEnumerator WaitBeforeSwitchingBackToPatrol()
        {
            _waiting = true;
            yield return new WaitForSeconds(2);

            if (state == EnemyState.Detecting)
            {
                state = EnemyState.Patrolling;
            }

            _waiting = false;
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (col.gameObject.layer == LayerMask.NameToLayer("Card"))
            {
                state = EnemyState.Disabled;
                // TODO: Maybe destroy card? 
            }

            // If the player touches an alive and un-stunned guard it should aggro them immediately 
            if (col.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                if (state is EnemyState.Disabled or EnemyState.Stunned) return;
                state = EnemyState.Aggro;
            }
        }
    }
}
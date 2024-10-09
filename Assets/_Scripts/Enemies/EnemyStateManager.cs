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
        private Collider2D _col;

        public void SetState(EnemyState newState)
        {
            this.state = newState;
        }
        
        private void Awake()
        {
            _viewTypes = GetComponents<IViewType>();
            _rb = GetComponent<Rigidbody2D>();
            _col = GetComponent<Collider2D>();
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
                _rb.velocity = new Vector2(0f, 0f);
                // If the enemy is disabled they should just stop at their current position and be immovable for now
                _rb.isKinematic = true;
                _col.isTrigger = true;
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
                if (viewType.IsPlayerDetectedThisFrame())
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


        private void OnCollisionEnter2D(Collision2D col)
        {
            if (col.gameObject.layer == LayerMask.NameToLayer("Card"))
            {

            }

            // If the player touches an alive and un-stunned guard it should aggro them immediately 
            if (col.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                if (state is EnemyState.Disabled or EnemyState.Stunned) return;
                Debug.Log("EnemyStateManager collision");
                state = EnemyState.Aggro;
            }
        }

        public void KillEnemy()
        {
            state = EnemyState.Disabled;
        }
    }
}
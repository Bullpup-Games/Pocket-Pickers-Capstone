using System;
using _Scripts.Enemies.ViewTypes;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Enemies
{
    public class EnemyStateManager : MonoBehaviour
    {
        public EnemyState state;
        private IViewType[] _viewTypes;
        public void SetState(EnemyState newState)
        {
            this.state = newState;
        }
        
        private void Awake()
        {
            _viewTypes = GetComponents<IViewType>();
        }

        private void OnEnable()
        {
            foreach (var viewType in _viewTypes)
            {
                viewType.PlayerDetected += HandlePlayerSighting;
                // viewType.NoPlayerDetected += HandleNoPlayerSighting;
            }
        }

        private void OnDisable()
        {
            foreach (var viewType in _viewTypes)
            {
                viewType.PlayerDetected -= HandlePlayerSighting;
                // viewType.NoPlayerDetected -= HandleNoPlayerSighting;
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

        // TODO: Determine if this and the associated NoPlayerSighted event is still needed for anything 
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
                    // state = EnemyState.Patrolling;
                    return;
                case EnemyState.Aggro:
                    return;
                case EnemyState.Searching:
                    return;
                case EnemyState.Stunned:
                    return;
            }
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
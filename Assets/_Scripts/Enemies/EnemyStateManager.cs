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
        // private IViewType _lineView;
        public void SetState(EnemyState state)
        {
            this.state = state;
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

        private void HandlePlayerSighting()
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
                    state = EnemyState.Patrolling;
                    return;
                case EnemyState.Aggro:
                    state = EnemyState.Searching;
                    return;
                case EnemyState.Searching:
                    return;
                case EnemyState.Stunned:
                    return;
            }
        }
    }
}
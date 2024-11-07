using System;
using _Scripts.Enemies.ViewTypes;
using _Scripts.Player;
using UnityEngine;

namespace _Scripts.Enemies.Sniper.State
{
    public class SniperStateManager : MonoBehaviour, IEnemyStateManager<SniperStateManager>
    {
        public IEnemyState<SniperStateManager> PatrollingState { get; private set; }
        public IEnemyState<SniperStateManager> ChargingState { get; private set; }
        public IEnemyState<SniperStateManager> ReloadingState { get; private set; }
        public IEnemyState<SniperStateManager> InvestigatingState { get; private set; }
        public IEnemyState<SniperStateManager> DisabledState { get; private set; }
        public IEnemyState<SniperStateManager> CurrentState { get; private set; }
        public IEnemyState<SniperStateManager> PreviousState { get; private set; }
        [SerializeField] private SniperState enumState;

        [HideInInspector] public SniperSettings Settings;
        [HideInInspector] public Rigidbody2D Rigidbody2D;
        [HideInInspector] public Collider2D Collider2D;
        [HideInInspector] public IViewType[] ViewTypes;

        [HideInInspector] public LayerMask environmentLayer;
        [HideInInspector] public LayerMask enemyLayer;
        [HideInInspector] public LayerMask playerLayer;
        
        [Header("Sin Values")]
        public int sinPenalty;

        public bool alertedFromSkreecher;
        private void Awake()
        {
            Debug.Log("SniperStateManager Awake");
            Settings = GetComponent<SniperSettings>();
            Rigidbody2D = GetComponent<Rigidbody2D>();
            Collider2D = GetComponent<Collider2D>();
            ViewTypes = GetComponents<IViewType>();
            
            environmentLayer = LayerMask.GetMask("Environment");
            playerLayer = LayerMask.GetMask("Enemy");
            playerLayer = LayerMask.GetMask("Player");

            PatrollingState = new SniperPatrollingState();
            ChargingState = new SniperChargingState();
            ReloadingState = new SniperReloadingState();
            InvestigatingState = new SniperInvestigatingState();
            DisabledState = new SniperDisabledState();

            // Set the initial state
            CurrentState = PatrollingState;
            CurrentState.EnterState(this);
        }
        
        private void Update()
        {
            // Update the current state via its UpdateState function
            CurrentState.UpdateState();
            
            if (IsPatrollingState())
            {
                enumState = SniperState.Patrolling;
            }
            else if (IsChargingState())
            {
                enumState = SniperState.Charging;
            }
            else if (IsReloadingState())
            {
                enumState = SniperState.Reloading;
            }
            else if (IsInvestigatingState())
            {
                enumState = SniperState.Investigating;
            }
            else if (IsDisabledState())
            {
                enumState = SniperState.Disabled;
            }
        }
        public void TransitionToState(IEnemyState<SniperStateManager> newState)
        {
            // if (CurrentState == newState)
            //     return;
            
            // Disable state trap
            if (CurrentState == DisabledState)
            {
                Debug.Log("Tried to exit from DisabledState");
                return;
            }

            PreviousState = CurrentState;
            CurrentState.ExitState();
            CurrentState = newState;
            CurrentState.EnterState(this); 
        }
        
        public void KillEnemy()
        {
            PlayerVariables.Instance.CommitSin(sinPenalty);
            TransitionToState(this.DisabledState);
        }
        
        public void AlertFromSkreecher()
        {
            if (CurrentState == DisabledState) return;
            alertedFromSkreecher = true;
            TransitionToState(ChargingState);
        }
        
        public bool IsPlayerDetected()
        {
            foreach (var viewType in ViewTypes)
            {
                if (viewType.IsPlayerDetectedThisFrame())
                {
                    return true;
                }
            }
            return false;
        }
        
        public bool IsPlayerDetectedWithQuickDetect()
        {
            foreach (var viewType in ViewTypes)
            {
                if (viewType.IsPlayerDetectedThisFrame())
                {
                    if (viewType.QuickDetection())
                    {
                        return true;
                    }
                }
            }
            return false;
        } 
        
        #region State Getters

        public bool IsPatrollingState()
        {
            return CurrentState == PatrollingState;
        }
        public bool IsDetectingState()
        {
            return false;
        }
        public bool IsAlertedState()
        {
            return false;
        }
        public bool IsInvestigatingState()
        {
            return CurrentState == InvestigatingState;
        }
        public bool IsDisabledState()
        {
            return CurrentState == DisabledState;
        }
        
        // For alternate enemy types just return false

        // Guard-Specific
        public bool IsAggroState() // Guard & Bat
        {
            return false;
        }
        public bool IsStunnedState()
        {
            return false;
        }
        
        // Sniper Specific
        public bool IsChargingState()
        {
            return CurrentState == ChargingState;
        }
        public bool IsReloadingState()
        {
            return CurrentState == ReloadingState;
        }
        
        #endregion
    }
}
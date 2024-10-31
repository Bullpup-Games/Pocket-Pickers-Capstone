using _Scripts.Enemies.ViewTypes;
using _Scripts.Player;
using UnityEngine;

namespace _Scripts.Enemies.Sniper.State
{
    public class SniperStateManager : MonoBehaviour, IEnemyStateManager<SniperStateManager>
    {
        public IEnemyState<SniperStateManager> PatrollingState { get; private set; }
        public IEnemyState<SniperStateManager> DetectingState { get; private set; }
        public IEnemyState<SniperStateManager> ChargingState { get; private set; }
        public IEnemyState<SniperStateManager> ReloadingState { get; private set; }
        public IEnemyState<SniperStateManager> ReturningState { get; private set; }
        public IEnemyState<SniperStateManager> StunnedState { get; private set; }
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
        private void Awake()
        {
            Settings = GetComponent<SniperSettings>();
            Rigidbody2D = GetComponent<Rigidbody2D>();
            Collider2D = GetComponent<Collider2D>();
            ViewTypes = GetComponents<IViewType>();
            
            environmentLayer = LayerMask.GetMask("Environment");
            playerLayer = LayerMask.GetMask("Enemy");
            playerLayer = LayerMask.GetMask("Player");

            PatrollingState = new SniperPatrollingState();
            DetectingState = new SniperDetectingState();
            ChargingState = new SniperChargingState();
            ReloadingState = new SniperReloadingState();

            // Set the initial state
            CurrentState = PatrollingState;
            CurrentState.EnterState(this);
        }
        public void TransitionToState(IEnemyState<SniperStateManager> newState)
        {
            
        }
        
        public void KillEnemy()
        {
            PlayerVariables.Instance.CommitSin(sinPenalty);
            TransitionToState(this.DisabledState);
        }
        
        #region State Getters

        public bool IsPatrollingState()
        {
            return false;
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
            return false;
        }
        public bool IsDisabledState()
        {
            return false;
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
            return false;
        }
        public bool IsReloadingState()
        {
            return false;
        }
        
        #endregion
    }
}
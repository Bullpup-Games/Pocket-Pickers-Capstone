using System;
using _Scripts.Card;
using UnityEngine;

namespace _Scripts.Player.State
{
    public class PlayerStateManager : MonoBehaviour
    {
        // States
        public IPlayerState FreeMovingState { get; private set; }
        public IPlayerState DashingState { get; private set; }
        public IPlayerState WallState { get; private set; }
        public IPlayerState StunnedState { get; private set; }
        public IPlayerState CurrentState { get; private set; }
        public IPlayerState PreviousState { get; private set; }

        [SerializeField] private PlayerState enumState;

        #region Singleton

        public static PlayerStateManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType(typeof(PlayerStateManager)) as PlayerStateManager;

                return _instance;
            }
            set { _instance = value; }
        }

        private static PlayerStateManager _instance;

        #endregion

        private void Awake()
        {
            // Initialize States
            FreeMovingState = new FreeMovingState();
            DashingState = new DashingState();
            WallState = new WallState();
            StunnedState = new StunnedState();

            // Set Initial State
            TransitionToState(FreeMovingState);
        }

        private void Update()
        {
            if (CurrentState == FreeMovingState)
                enumState = PlayerState.FreeMoving;
            if (CurrentState == StunnedState)
                enumState = PlayerState.Stunned;
            if (CurrentState == DashingState)
                enumState = PlayerState.Dashing;
            if (CurrentState == WallState)
                enumState = PlayerState.Wall;
            
            CurrentState.UpdateState();
        }

        private void FixedUpdate()
        {
            CurrentState.FixedUpdateState();
        }

        public void TransitionToState(IPlayerState newState)
        {
            // State blocks
            if (CurrentState == StunnedState && newState == DashingState) return;
            if (CurrentState == StunnedState && newState == WallState) return;
            
            if (CurrentState != null)
                CurrentState.ExitState();

            PreviousState = CurrentState;
            CurrentState = newState;
            CurrentState.EnterState();
        }

        #region Dash Transition
        private float _lastDashTime;
        private void OnEnable()
        {
            InputHandler.Instance.OnDash += OnDashAction;
        }
        private void OnDisable()
        {
            InputHandler.Instance.OnDash -= OnDashAction;
        }
        private void OnDashAction()
        {
            // Cooldown check
            if (_lastDashTime + PlayerVariables.Instance.Stats.DashCooldown > PlayerVariables.Instance.Time)
            {
                Debug.Log("Dash cooldown");
                return;
            };
            
            _lastDashTime = PlayerVariables.Instance.Time;
            TransitionToState(DashingState);
        }
        #endregion

        public bool IsStunnedState()
        {
            return CurrentState is StunnedState;
        }
    }
}
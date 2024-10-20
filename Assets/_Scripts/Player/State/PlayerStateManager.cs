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
            CurrentState.UpdateState();
        }

        private void FixedUpdate()
        {
            CurrentState.FixedUpdateState();
        }

        public void TransitionToState(IPlayerState newState)
        {
            if (CurrentState != null)
                CurrentState.ExitState();

            PreviousState = CurrentState;
            CurrentState = newState;
            CurrentState.EnterState();
        }

        public bool IsStunnedState()
        {
            return CurrentState is StunnedState;
        }
    }
}
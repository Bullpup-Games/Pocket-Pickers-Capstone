using System;
using _Scripts.Card;
using UnityEngine;
using UnityEngine.XR;

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
        }

        private void Start()
        {
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
            if (CurrentState == newState) return;
            if (CurrentState == StunnedState && newState == DashingState) return;
            if (CurrentState == StunnedState && newState == WallState) return;
            
            if (CurrentState != null)
                CurrentState.ExitState();

            PreviousState = CurrentState;
            CurrentState = newState;
            CurrentState.EnterState();
        }
        
        private void OnEnable()
        {
            if (InputHandler.Instance == null) return;
            InputHandler.Instance.OnDash += OnDashAction;
            PlayerMovement.Instance.Walled += HandleWallStateTransition;
        }
        private void OnDisable()
        {
            if (InputHandler.Instance == null) return;
            InputHandler.Instance.OnDash -= OnDashAction;
            PlayerMovement.Instance.Walled -= HandleWallStateTransition;
        }

        #region Dash Transition
        private float _lastDashTime;
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
        
        #region Wall Sliding

        public float lastWallHangTime;

        private void HandleWallStateTransition(bool isWalled)
        {
            if (CurrentState == WallState)
            {
                lastWallHangTime = Time.time;
                return;
            }

            if (!isWalled)
            {
                return;
            }

            if (PlayerMovement.Instance.JumpHeldFrameInput && PlayerMovement.Instance.CurrentFrameVelocity.y > 0f)
            {
                return;
            }

            if (lastWallHangTime + PlayerVariables.Instance.Stats.WallHangCooldown > Time.time)
            {
                // Debug.Log("Wall hang cooldown");
                return;
            }

            // If the player is touching a wall, is not grounded, is moving downwards, and is moving the left stick in the direction of the wall they hit...
            if (!PlayerMovement.Instance.IsGrounded() &&
                ((PlayerVariables.Instance.isFacingRight && PlayerMovement.Instance.FrameInput.x > 0) ||
                 (!PlayerVariables.Instance.isFacingRight && PlayerMovement.Instance.FrameInput.x < 0)))
            {
                TransitionToState(WallState);
            }
        }
        
        public void SetLastWallHangTime(float time)
        {
            lastWallHangTime = time;
        }
        #endregion

        public bool IsStunnedState()
        {
            return CurrentState is StunnedState;
        }

        public bool IsWallState()
        {
            return CurrentState is WallState;
        }
    }
}
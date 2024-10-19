using System;
using _Scripts.Card;
using UnityEngine;

namespace _Scripts.Player.State
{
    public struct FrameInput
    {
        public bool JumpDown;
        public bool JumpHeld;
        public Vector2 Move;
    }
    public class PlayerStateManager : MonoBehaviour
    {
        public IPlayerState FreeMovingState { get; private set; }
        public IPlayerState DashingState { get; private set; }
        public IPlayerState WallState { get; private set; }
        public IPlayerState StunnedState { get; private set; }
        public IPlayerState CurrentState { get; private set; }
        public IPlayerState PreviousState { get; private set; }
        
        public FrameInput FrameInput;

        public Vector2 MovementInput => InputHandler.Instance.MovementInput;
        public bool JumpPressed => InputHandler.Instance.JumpPressed;
        public bool JumpHeld => InputHandler.Instance.JumpHeld;
        
        public PlayerState enumState;
        
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
            FreeMovingState = new FreeMovingState();
            DashingState = new DashingState();
            WallState = new WallState();
            StunnedState = new StunnedState();
        }

        private void Start()
        {
            TransitionToState(FreeMovingState);
        }

        private void Update()
        {
            CurrentState.HandleInput();
            CurrentState.UpdateState();
        }

        private void FixedUpdate()
        {
            CurrentState.FixedUpdateState();
        }

        public void TransitionToState(IPlayerState state)
        {
            if (CurrentState != null)
                CurrentState.ExitState();

            CurrentState = state;
            CurrentState.EnterState(this);
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            CurrentState.OnCollisionEnter2D(col);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("EscapeRout"))
            {
                PlayerVariables.Instance.escape();
            }
        }

        // public void SetState(PlayerState newState)
        // {
        //     this.enumState = newState;
        // } 
        private void OnEnable()
        {
            //TODO make it subscribe to Card's Teleport event
            CardManager.Instance.Teleport += TeleportTo;
        } 
        private void TeleportTo(Vector2 location)
        {
            var transform1 = transform;
            transform1.position = location;
            transform1.rotation = Quaternion.identity;
            //todo set the player's velocity to 0
        }

        #region State Getters

        public bool IsStunnedState()
        {
            return CurrentState is StunnedState;
        }

        #endregion
    }
}
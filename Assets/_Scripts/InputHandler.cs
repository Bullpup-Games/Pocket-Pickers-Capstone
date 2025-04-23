using System;
using System.Linq;
using _Scripts.Camera;
using _Scripts.Card;
using _Scripts.Player;
using _Scripts.Player.State;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts
{
    public class InputHandler : MonoBehaviour
    {
        public Vector2 MovementInput { get; private set; }
        public Vector3 LookInput { get; private set; }
        public bool JumpPressed { get; private set; }
        public bool JumpHeld { get; private set; }

        public enum InputDeviceType { KeyboardMouse, Gamepad }
        public InputDeviceType CurrentInputDevice { get; private set; } = InputDeviceType.KeyboardMouse;


        #region Singleton

        public static InputHandler Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType(typeof(InputHandler)) as InputHandler;

                return _instance;
            }
            set { _instance = value; }
        }

        private static InputHandler _instance;

        #endregion

        private PlayerInputActions _inputActions;

        private void OnEnable()
        {
            if (_inputActions == null)
            {
                _inputActions = new PlayerInputActions();
            }

            _inputActions.Player.Enable();
            _inputActions.UI.Enable();

            // Detect control scheme changes
            _inputActions.asset.controlSchemes.ToList().ForEach(scheme =>
            {
                Debug.Log($"Found control scheme: {scheme.name}");
            });

            // Subscribe to input events
            _inputActions.Player.Aim.performed += OnLookPerformed;
            _inputActions.Player.Aim.canceled += OnLookCanceled;

            _inputActions.Player.Throw.performed += OnThrowPerformed;
            _inputActions.Player.CancelCardThrow.performed += OnCancelCardThrow;
            _inputActions.Player.FalseTrigger.performed += OnFalseTriggerPerformed;
            
            _inputActions.Player.Move.performed += OnMovePerformed;
            _inputActions.Player.Move.canceled += OnMoveCanceled;
            _inputActions.Player.Jump.performed += OnJumpPerformed;
            _inputActions.Player.Jump.canceled += OnJumpCanceled;

            _inputActions.Player.Dash.performed += OnDashPerformed;
            _inputActions.Player.Crouch.performed += OnCrouchPerformed;
            
            _inputActions.UI.PauseEvent.performed += OnPausePerformed;

            // if (PlayerVariables.Instance is not null)
            //     PlayerVariables.Instance.PlayerInput.onControlsChanged += OnControlsChanged;

        }

        private void OnDisable()
        {
            _inputActions.Player.Aim.performed -= OnLookPerformed;
            _inputActions.Player.Aim.canceled -= OnLookCanceled;

            _inputActions.Player.Throw.performed -= OnThrowPerformed;
            _inputActions.Player.CancelCardThrow.performed -= OnCancelCardThrow;
            _inputActions.Player.FalseTrigger.performed -= OnFalseTriggerPerformed;
            
            _inputActions.Player.Move.performed -= OnMovePerformed;
            _inputActions.Player.Move.canceled -= OnMoveCanceled;
            _inputActions.Player.Jump.performed -= OnJumpPerformed;
            _inputActions.Player.Jump.canceled -= OnJumpCanceled;

            _inputActions.Player.Dash.performed -= OnDashPerformed;
            _inputActions.Player.Crouch.performed -= OnCrouchPerformed;
            
            _inputActions.UI.PauseEvent.performed -= OnPausePerformed;
            // if (PlayerVariables.Instance is not null)
            //     PlayerVariables.Instance.PlayerInput.onControlsChanged -= OnControlsChanged;

            _inputActions.Player.Disable();
            _inputActions.UI.Disable();
        }

        private void Update()
        {
            if (GameManager.Instance is null || GameManager.Instance.isDead)
                return;

            if (PlayerStateManager.Instance.IsStunnedState() || CardManager.Instance.IsCardInScene())
                return;

            Vector2 mouseScreen = Mouse.current.position.ReadValue();
            Vector3 worldMouse = UnityEngine.Camera.main.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, 0));
            int centerX = Screen.width / 2;
            int centerY = Screen.height / 2;

            Vector3 centerPos = UnityEngine.Camera.main.ScreenToWorldPoint(new Vector3(centerX, centerY, 0));
            // Vector3 playerPos = PlayerVariables.Instance.transform.position;


            Vector2 aimDir = worldMouse - centerPos;

            if (aimDir.magnitude > 0.1f)
                CardStanceDirectionalInput?.Invoke(aimDir.normalized);
            else
                CardStanceDirectionalInput?.Invoke(Vector2.zero);

            // switch (CurrentInputDevice)
            // {
            //     case InputDeviceType.Gamepad:
            //         LookInput = _inputActions.Player.Aim.ReadValue<Vector2>();
            //         if (LookInput.magnitude > 0.1f)
            //             CardStanceDirectionalInput?.Invoke(LookInput.normalized);
            //         else
            //             CardStanceDirectionalInput?.Invoke(Vector2.zero);
            //         break;

            //     case InputDeviceType.KeyboardMouse:
            //         break;
            // }

            if (JumpPressed)
                JumpPressed = false;
        }

        // Event for updating direction while in card stance
        public event Action<Vector2> CardStanceDirectionalInput;

        // Event for handling card throw
        public event Action OnCardThrow;

        private Vector2 _lookInput;

        // public Vector2 LookInput() => _lookInput;

        private void OnLookPerformed(InputAction.CallbackContext context)
        {
            if (GameManager.Instance is null)
                return;
            if (GameManager.Instance.isDead)
                return;

            if (PlayerStateManager.Instance.IsStunnedState())
                return;
            if (CardManager.Instance.IsCardInScene()) 
                return;

            _lookInput = context.ReadValue<Vector2>();

            if (_lookInput.magnitude > 0.1f)
            {
                Vector2 inputDirection = _lookInput.normalized;
                CardStanceDirectionalInput?.Invoke(inputDirection);
            }
            else
            {
                CardStanceDirectionalInput?.Invoke(Vector2.zero);
            }
        }

        private void OnLookCanceled(InputAction.CallbackContext context)
        {
            CardStanceDirectionalInput?.Invoke(Vector2.zero);
        }

        private void OnThrowPerformed(InputAction.CallbackContext context)
        {
            // if (PlayerStateManager.Instance.IsStunnedState()) return;
            // Debug.Log("Throw Input");
            if (GameManager.Instance is null) 
                return;
            if (!GameManager.Instance.isDead)
                OnCardThrow?.Invoke();
        }
        
        public event Action OnFalseTrigger;
        private void OnFalseTriggerPerformed(InputAction.CallbackContext context)
        
        {
                /*
                 * The False trigger input is used to escape stuns. Even if it wasn't, it would be a clever way
                 * of escaping one regardless if the player already has an active card out and near the enemy.
                 * So, allow FalseTrigger input even if the player is stunned
                 */
                // Debug.Log("False Trigger Input");
                OnFalseTrigger?.Invoke();
        }

        public event Action OnCancelActiveCard;
        private void OnCancelCardThrow(InputAction.CallbackContext context)
        {
            // Debug.Log("Cancel throw");
            OnCancelActiveCard?.Invoke();
        }

        public event Action OnDash;

        private void OnDashPerformed(InputAction.CallbackContext context)
        {
            OnDash?.Invoke();
        }

        public event Action OnPausePressed;

        private void OnPausePerformed(InputAction.CallbackContext context)
        {
            Debug.Log("Pause Pressed");
            OnPausePressed?.Invoke();
        }
        
        private void OnMovePerformed(InputAction.CallbackContext context)
        {
            MovementInput = context.ReadValue<Vector2>();
        }

        private void OnMoveCanceled(InputAction.CallbackContext context)
        {
            MovementInput = Vector2.zero;
        }
        public event Action OnJumpPressed;
        
        private void OnJumpPerformed(InputAction.CallbackContext context)
        {
            // JumpPressed = true;
            JumpHeld = true;
            OnJumpPressed?.Invoke();
        }

        private void OnJumpCanceled(InputAction.CallbackContext context)
        {
            JumpHeld = false;
        }

        public Action OnCrouch;

        private void OnCrouchPerformed(InputAction.CallbackContext context)
        {
            OnCrouch?.Invoke();
        }

        private void OnControlsChanged(PlayerInput input)
        {
            CurrentInputDevice = input.currentControlScheme switch
            {
                "KeyboardMouse" => InputDeviceType.Gamepad,
                _ => InputDeviceType.KeyboardMouse,
            };

            Debug.Log($"Switched to {CurrentInputDevice}");
        }
    }
}
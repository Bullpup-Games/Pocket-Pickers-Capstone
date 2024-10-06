using System;
using UnityEngine;

namespace _Scripts
{
    /**
 * Input manager axis to DuelSense inputs
- 0 - Square
- 1 - X
- 2 - Circle
- 3 - Triangle
- 4 - Left Bumper
- 5 - Right Bumper
- 6 - Left Trigger
- 7 - Right Trigger
- 8 - Share Button
- 9 - Menu Button
- 10 - Left Stick Down
- 11 - Right Stick Down
- 12 - On / Off Button
- 13 - DuelSense GamePad
 */
    public class InputHandler : MonoBehaviour
    {
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

        private void Update()
        {
            HandleCardStanceInput();
            HandleCardThrowInput();
        }

        public event Action OnEnterCardStance; // Event for entering card stance
        public event Action OnExitCardStance;  // Event for exiting card stance

        private bool _wasInCardStance = false; // Check to see if card stance was already entered last in the previous frame

        private void HandleCardStanceInput()
        {
            var triggerValue = Input.GetAxis("CardStance");
            bool isInCardStance = Mathf.Abs(triggerValue) > 0.1f;

            if (isInCardStance && !_wasInCardStance)
            {
                // Trigger pressed
                OnEnterCardStance?.Invoke();
                Debug.Log("Entered Card Stance");
            }
            else if (!isInCardStance && _wasInCardStance)
            {
                // Trigger released
                OnExitCardStance?.Invoke();
                Debug.Log("Exited Card Stance");
            }

            _wasInCardStance = isInCardStance;
        }

        public event Action OnCardThrow;

        private void HandleCardThrowInput()
        {
            // if (!PlayerVariables.Instance.inCardStance)
            // {
            //     // TODO: Instead of blocking the input if the player isn't in card stance send a quick throw action
            //     return;
            // }

            if (Input.GetButtonDown("CardThrow"))
            {
                OnCardThrow?.Invoke();
            }
        }
    }
}

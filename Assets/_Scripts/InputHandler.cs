using System;
using UnityEngine;

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
    public static InputHandler Instance { get; private set; }

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Update()
    {
        HandleCardStanceInput();
    }

    public event Action OnEnterCardStance; // Event for entering card stance
    public event Action OnExitCardStance;  // Event for exiting card stance

    private bool _wasInCardStance = false; // Check to see if card stance was already entered last in the previous frame

    private void HandleCardStanceInput()
    {
        var triggerValue = Input.GetAxis("RightTrigger");
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
}

using System.Collections;
using System.Collections.Generic;
using PlayerController;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public PlayerMovementController playerMovementController;
    public InputHandler inputHandler;

    private void OnEnable()
    {
        inputHandler.OnEnterCardStance += HandleEnterCardStance;
        inputHandler.OnExitCardStance += HandleExitCardStance;
    }

    private void OnDisable()
    {
        inputHandler.OnEnterCardStance -= HandleEnterCardStance;
        inputHandler.OnExitCardStance -= HandleExitCardStance;
    }

    private void HandleEnterCardStance()
    {
        playerMovementController.EnterCardStance();
    }

    private void HandleExitCardStance()
    {
        playerMovementController.ExitCardStance();
    }
}

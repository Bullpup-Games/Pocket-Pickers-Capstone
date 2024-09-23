using System.Collections;
using System.Collections.Generic;
using PlayerController;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public InputHandler inputHandler;
    public PlayerMovementController playerMovementController;
    public HandleCardStanceArrow cardStanceArrow;

    private void OnEnable()
    {
        // enter card stance
        inputHandler.OnEnterCardStance += HandleEnterCardStance;
        inputHandler.OnExitCardStance += HandleExitCardStance;
        
        // enable card stance directional arrow
        inputHandler.OnEnterCardStance += HandleShowDirectionalArrow;
        inputHandler.OnExitCardStance += HandleHideDirectionalArrow;
        
    }

    private void OnDisable()
    {
        // exit Card Stance
        inputHandler.OnEnterCardStance -= HandleEnterCardStance;
        inputHandler.OnExitCardStance -= HandleExitCardStance;
        
        // disable card stance directional arrow
        inputHandler.OnEnterCardStance -= HandleShowDirectionalArrow;
        inputHandler.OnExitCardStance -= HandleHideDirectionalArrow;
    }

    private void HandleEnterCardStance()
    {
        playerMovementController.EnterCardStance();
    }

    private void HandleExitCardStance()
    {
        playerMovementController.ExitCardStance();
    }

    private void HandleShowDirectionalArrow()
    {
       cardStanceArrow.InstantiateDirectionalArrow(); 
    }

    private void HandleHideDirectionalArrow()
    {
        cardStanceArrow.DestroyDirectionalArrow();
    }
}

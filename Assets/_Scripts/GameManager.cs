using Card;
using PlayerController;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public InputHandler inputHandler;
    public PlayerMovementController playerMovementController;
    public HandleCardStanceArrow cardStanceArrow;
    
    public static GameManager Instance { get; private set; }

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
        }
    }

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


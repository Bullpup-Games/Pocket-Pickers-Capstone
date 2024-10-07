using System;
using _Scripts.Player;
using UnityEngine;

namespace _Scripts.Card
{

    /**
    * Card Manager is responsible for instantiating, destroying, and keeping track on the current active card
     * It also tracks subscribes to the EventHandler for information about about card related inputs
     * and sends them out to the necessary scripts.
    */
    
    /*
     * The plan:
     * x Make card a singleton
     * x When you recieve the card throw event:
     * x Check if inStance is true
     * x if true, throw card
     * x else, call teleport function
     * x teleport will check if instance of card is null
     * x if it is not null, call event Teleport
     * x If it is null do nothing
     */
    public class CardManager : MonoBehaviour
    {
        public InputHandler inputHandler;
        public PlayerMovementController playerMovementController;
        public HandleCardStanceArrow cardStanceArrow;
        private PlayerStateManager _playerStateManager;

        public Transform player;
        public GameObject cardPrefab;
        private GameObject _instantiatedCard;
        
        [Header("Card and Direction Arrow Offsets")]
        public float horizontalOffset = 2.0f;
        public float verticalOffset = 0.3f;

        private float _lastCardThrowTime;  // Track the last time a card was thrown
        
        [Header("Card Behavior Variables")] 
        public float cooldown = 0.5f;
        public float cardSpeed = 20.0f;
        public float cardLifeTime = 10.0f;
        
        
        public event Action <Vector2> Teleport ;
        
        #region Singleton

        public static CardManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType(typeof(CardManager)) as CardManager;

                return _instance;
            }
            set
            {
                _instance = value;
            }
        }
        private static CardManager _instance;
        #endregion

        private void OnEnable()
        {
            // enter card stance
            inputHandler.OnEnterCardStance += HandleEnterCardStance;
            inputHandler.OnExitCardStance += HandleExitCardStance;
        
            // enable card stance directional arrow
            inputHandler.OnEnterCardStance += HandleShowDirectionalArrow;
            inputHandler.OnExitCardStance += HandleHideDirectionalArrow;
            
            // Subscribe to the card throw event
            inputHandler.OnCardThrow += HandleCardAction;
        }

        private void OnDisable()
        {
            // exit Card Stance
            inputHandler.OnEnterCardStance -= HandleEnterCardStance;
            inputHandler.OnExitCardStance -= HandleExitCardStance;
        
            // disable card stance directional arrow
            inputHandler.OnEnterCardStance -= HandleShowDirectionalArrow;
            inputHandler.OnExitCardStance -= HandleHideDirectionalArrow;
            
            // Unsubscribe from the card throw event
            inputHandler.OnCardThrow -= HandleCardAction;
        }
        
        //decide if you should throw card or teleport
        //if you are in the card stance you should throw a card.
        //if not, you should test for teleportation
        private void HandleCardAction()
        {
            if (PlayerVariables.Instance.stateManager.state == PlayerState.CardStance &&  _instantiatedCard == null)
            {
                HandleCardThrow();
            }
            else
            {
                HandleTeleportation();
            }
        }

        /**
        * Called when the player triggers a card throw, responsible for instantiating a new card
        */
        private void HandleCardThrow()
        {
            // Check if the cooldown has passed
            if (Time.time - _lastCardThrowTime <= cooldown)
            {
                Debug.Log("Card throw is on cooldown.");
                return;
            }
            
            // If a card is active, destroy the existing card
            Card cardScript;
            if (_instantiatedCard != null)
            {
                cardScript = _instantiatedCard.GetComponent<Card>();
                if (cardScript != null)
                {
                    cardScript.DestroyCard();
                }
                else
                {
                    Debug.LogError("Card script not found on active card.");
                    Destroy(_instantiatedCard);  // Fallback: destroy the card if no script is found
                }
                _instantiatedCard = null;  // Reset the active card reference
            }

            // Get the launch direction from the HandleCardStanceArrow
            var currentDirection = HandleCardStanceArrow.Instance.currentDirection;
            var angleRad = Mathf.Atan2(currentDirection.y, currentDirection.x);
            // Calculate the arrow's position relative to the player
            var offset = new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad) + verticalOffset, 0) * horizontalOffset;
            var cardSpawnLocation = player.position + offset;

            // Instantiate the card at the player's position
            _instantiatedCard = Instantiate(cardPrefab, cardSpawnLocation, Quaternion.identity);

            // Get the Card script component from the instantiated card to call its Launch function
            cardScript = _instantiatedCard.GetComponent<Card>();

            // Pass the launch direction to the card
            if (cardScript != null)
            {
                cardScript.Launch(currentDirection);
            }
            else
            {
                Debug.LogError("Card script not found on Card.");
            }

            _lastCardThrowTime = Time.time;
        }

        //if the instance of card is not null, trigger an event that holds the card's
        //transform location. If it is null, do nothing.
        private void HandleTeleportation()
        {
            if (Card.Instance != null)
            {
                Teleport.Invoke(Card.Instance.transform.position);
            }
        }

        /**
     * Called by the Card script when the card is destroyed or its lifecycle ends
     */
        public void OnCardDestroyed()
        {
            _instantiatedCard = null;
            Destroy(_instantiatedCard);
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
}
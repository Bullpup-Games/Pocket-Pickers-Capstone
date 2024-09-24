using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Card
{

    /**
 * Card Manager is responsible for instantiating, destroying, and keeping track on the current active card
 */
    public class CardManager : MonoBehaviour
    {
        public static CardManager Instance { get; private set; }
        public InputHandler inputHandler;

        public Transform player;
        public GameObject cardPrefab;
        private GameObject _instantiatedCard;

        [Header("Card and Direction Arrow Offsets")]
        public float horizontalOffset = 2.0f;  // Distance from the player to position the arrow
        public float verticalOffset = 0.3f;

        private float _lastCardThrowTime;  // Track the last time a card was thrown
        
        [Header("Card Behavior Variables")] 
        public float cooldown = 0.5f;
        public float cardSpeed = 20.0f;
        public float cardLifeTime = 10.0f;

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
            // Subscribe to the card throw event
            inputHandler.OnCardThrow += HandleCardThrow;
        }

        private void OnDisable()
        {
            inputHandler.OnCardThrow -= HandleCardThrow;
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

        /**
     * Called by the Card script when the card is destroyed or its lifecycle ends
     */
        public void OnCardDestroyed()
        {
            _instantiatedCard = null;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Card
{
    public class Card : MonoBehaviour
    {
        private Vector2 _launchDirection; // Direction in which the card is launched
        private float _startTime;

        private Rigidbody2D _rigidbody;
        public InputHandler inputHandler;
        public static Card Instance {  get; private set; }

        /*
         *The plan:
         * x make a public event to say that there is a teleportation
         * x make a function that engages when the card throw button is pressed
         * x That function should invoke the teleport event
         * x That event  should communicate the transform location of the card as a vector2
         * the function should then destroy the gameobject
         *
         */
        //public event Action<Vector2> Teleport;

        /*
        When this object first exists, get everything set up.
        When it gets destroyed, tear everything down.
        It is a prefab, so some things we can't set in the editor
        I am putting a lot of things in seperate functions, and then
        calling those functions, just to make it more readable.
        For example, I'm calling setListeners and deleteListeners,
        and those functions are seperate
        */
        private void OnEnable()
        {
            inputHandler = GameObject.Find("InputHandler").GetComponent<InputHandler>();
            setListeners();
        }

        private void OnDestroy()
        {
            deleteListeners();
        }
        
        private void setListeners()
        {
            inputHandler.OnEnterCardStance += DestroyCard;
            CardManager.Instance.Teleport += CatchTeleport;
        }

        //todo sometimes when the card gets deleted, it doesn't have the inputHandler item
        private void deleteListeners()
        {
            inputHandler.OnEnterCardStance -= DestroyCard;
            CardManager.Instance.Teleport -= CatchTeleport;
        }

        private void Awake()
        {
            
            //singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
            
            
            _rigidbody = GetComponent<Rigidbody2D>();
            _startTime = Time.time;
        }

        public void Launch(Vector2 direction)
        {
            _launchDirection = direction.normalized;

            // Calculate initial velocity
            var velocity = _launchDirection * CardManager.Instance.cardSpeed;

            // Apply velocity to the Rigidbody2D
            _rigidbody.velocity = velocity;
        }

        private void Update()
        {
            // Destroy the card after its lifetime expires
            if (Time.time - _startTime >= CardManager.Instance.cardLifeTime)
            {
                DestroyCard();
            }

            // if (Input.GetButtonDown("CardThrow"))
            // {
            //     Debug.Log($"Activating teleporation using {transform.position.x}, {transform.position.y}");
            //     Teleport?.Invoke(new Vector2(transform.position.x, transform.position.y));
            //     DestroyCard();
            // }
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            // Ignore collisions with the player
            if (col.gameObject.CompareTag("Player"))
            {
                Physics2D.IgnoreCollision(col.collider, GetComponent<Collider2D>());
                return;
            } 
            // TODO: Add a check for walls and count bounces
            
            Debug.Log("Card collision");
        }

        //the teleport event needs to be caught by a function that takes in a vector2
        //all this function does is take in a vector 2 so that it can catch the Teleport
        //event, and then run DestroyCard.
        private void CatchTeleport(Vector2 noop)
        {
            Debug.Log("caught teleport");
            DestroyCard();
        }

        public void DestroyCard()
        {
            // Notify the CardManager that the card has been destroyed
            CardManager.Instance.OnCardDestroyed();
            Destroy(gameObject);
        }

        
    }
}
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

        private void OnEnable()
        {
            inputHandler = GameObject.Find("InputHandler").GetComponent<InputHandler>();
            inputHandler.OnEnterCardStance += DestroyCard;
        }

        private void OnDestroy()
        {
            inputHandler.OnEnterCardStance -= DestroyCard;
        }

        private void Awake()
        {
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

        public void DestroyCard()
        {
            // Notify the CardManager that the card has been destroyed
            CardManager.Instance.OnCardDestroyed();
            Destroy(gameObject);
        }
    }
}
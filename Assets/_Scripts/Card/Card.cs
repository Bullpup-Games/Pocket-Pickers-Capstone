using _Scripts.Player;
using UnityEngine;

namespace _Scripts.Card
{
    public class Card : MonoBehaviour
    {
        
        private float _startTime;

        //private Rigidbody2D _rigidbody;
        public InputHandler inputHandler;

        private Vector2 direction; // Direction in which the card is launched
        public float speed = 15;
        private Vector2 velocity;

        //total bounces is how many ricochets are allowed. Bounces is 
        //how many ricochets have happened
        public int totalBounces;
        public int bounces;
       // public Vector2 directon;
       
       #region Singleton

       public static Card Instance
       {
           get
           {
               if (_instance == null)
                   _instance = FindObjectOfType(typeof(Card)) as Card;

               return _instance;
           }
           set
           {
               _instance = value;
           }
       }
       private static Card _instance;
       #endregion

       /*
       The plan:
       x Add a constant public speed, and a direction vector
       x add a variable for total possible ricochets and a second variable
       for number of ricochets that have happened 
       Make collision detection with tag recognition for four scenarios
           x 1. It hit a wall
               If we have done all our ricochets
                   we will switch to a falling state
               Otherwise
                   We will bounce off the wall and keep our speed (calculate
                   the normal of the wall, do an angle reflection calculation,
                   set that as the new direction, normalize that, and multiply by
                   speed to set new velocity)
           x 2. It hits the player
               Card goes through player, nothing happens
           3. Card hits a grate/bars
               Card goes through the bars, but the player cant go through the bars
           4. Card hits an enemy
               Card disapears, enemy is incapacitated, add sin
       */
        

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
            //we start with 0 bounces, each time we bounce off a wall we incriment it
            bounces = 0;
            
            
            //if we don't have this, a card throw up or down will push the player around
            var col = PlayerVariables.Instance.gameObject.GetComponent<BoxCollider2D>();
            Physics2D.IgnoreCollision(col, GetComponent<Collider2D>());
            
           
            this.direction = HandleCardStanceArrow.Instance.currentDirection;
            _startTime = Time.time;
        }

        public void Launch(Vector2 direction)
        {
            this.direction = direction.normalized;
            calculateVelocity(this.direction);

            
        }

        private void calculateVelocity(Vector2 direction) {
            var velocity = direction * this.speed;
            this.velocity = velocity;
        }

        private void Update()
        {
            // Destroy the card after its lifetime expires
            if (Time.time - _startTime >= CardManager.Instance.cardLifeTime)
            {
                DestroyCard();
            }

            moveCard();
           
        }

        private void moveCard()
        {
            Vector3 newPosition = ((Vector2)transform.position) + (velocity * Time.deltaTime);
            this.transform.position = newPosition;
        }

       
        private void OnCollisionEnter2D(Collision2D col)
        {
            
            Debug.Log("Card collision");
            // Ignore collisions with the player
            if (col.gameObject.CompareTag("Player"))
            {
                Physics2D.IgnoreCollision(col.collider, GetComponent<Collider2D>());
                return;
            }

            if (col.gameObject.CompareTag("enemy"))
            {
                //todo if the card hits an enemy, incapacitate the enemy and destroy the card
                Physics2D.IgnoreCollision(col.collider, GetComponent<Collider2D>());
                return;
            }
            // TODO: Add a check for walls and count bounces
            if (col.gameObject.CompareTag("wall"))
            {
                bounces++;
                
                //todo deflect the card and change its direction
                if (bounces >= totalBounces)
                {
                    Debug.Log("no more ricochets");
                    //todo eventually we will not destroy the card, we will change states
                    DestroyCard();
                }
                
                //changes the direction of the card, and sets it to move in that direction
                Vector3 wallNormal = col.GetContact(0).normal;
                direction = Vector2.Reflect(direction, wallNormal);
                calculateVelocity(direction);
                
                //Physics2D.IgnoreCollision(col.collider, GetComponent<Collider2D>());
                return;
                
            }

            if (col.gameObject.CompareTag("permeable"))
            {
                //this tag exists because we want the player and enemies to be stopped
                //by permeable objects, but not the card.
                Physics2D.IgnoreCollision(col.collider, GetComponent<Collider2D>());
                return;
            }
            
            //Debug.Log("Card collision");
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
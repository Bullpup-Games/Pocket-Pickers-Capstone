using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Enemies.AggroTypes
{
    public class DefaultAggro : MonoBehaviour, IAggroType
    {
        [SerializeField] private float movementSpeed = 8f;
        [Header("Quick Time Event Variables")]
        [SerializeField] private float qteTimeLimit = 4f;
        [SerializeField] private float timeLostPerEncounter = 0.5f;
        [SerializeField] private int counterGoal = 5;
        private bool _hasExecuted = false;

        private Vector2 _targetPosition;
        
        /*
         * Default guard should move toward the player at a faster speed than the player is able to run
         * If the player breaks line of sight move to their last known position and switch states to Searching
         */
        public void Movement()
        {
            // update the player pos every time ConeView calls player detected
            // if NoPlayerDetected is called call GoToLastKnownLocation and return;
            
            // move to target location
        }

        /*
         * When within a certain range the guard should grab the player and enter a QTE that gets progressively
         * harder to pass. If failed trigger a level reset
         */
        public void Action()
        {
           // check distance to player
           
           // if within range call StartQuickTimeEvent 
           // Handle the rest from there
        }

        private void GoToLastKnownLocation(Vector2 location)
        {
            // move to target location 
            
            // switch to Searching state
        }

        // Grapple script from Don't Move - Needs to be integrated
        private IEnumerator StartQuicktimeEvent()
        {
            _hasExecuted = false;
            // grappleUI.SetActive(true);
            var counter = 0;
            var timeElapsed = 0f;

            while (timeElapsed < qteTimeLimit && counter < counterGoal)
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    counter++;
                }

                timeElapsed += Time.deltaTime;
                yield return null;
            }

            if (!_hasExecuted)
            {

                if (counter >= counterGoal)
                {
                    // grappleUI.SetActive(false);
                    // UnlockPositions();
                    if (qteTimeLimit > 2f)
                        qteTimeLimit -= timeLostPerEncounter;
                }
                else
                {
                    StopCoroutine(StartQuicktimeEvent());
                    // grappleUI.SetActive(false);
                    // GameOver.Instance.EndGame();
                }

                _hasExecuted = true;
            } 
        }
    }
}
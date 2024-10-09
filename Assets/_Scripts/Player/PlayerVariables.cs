using UnityEngine;
using UnityEngine.Timeline;

namespace _Scripts.Player
{
    public class PlayerVariables : MonoBehaviour
    {
        public bool isFacingRight = true;   // Start facing right by default
        // public bool inCardStance;
        [HideInInspector]public PlayerStateManager stateManager;
        
        //sin variables
        public int sinHeld;//how much you have picked up
        public int sinAccrued;//how much sin you have commited in game
        public int sinThreshold;//how much sinAccrued you have to reach in order to release a new sin

        #region Singleton

        public static PlayerVariables Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType(typeof(PlayerVariables)) as PlayerVariables;

                return _instance;
            }
            set
            {
                _instance = value;
            }
        }
        private static PlayerVariables _instance;
        #endregion

        private void Awake()
        {
            stateManager = GetComponent<PlayerStateManager>();
        }

        public void collectSin(int weight)
        {
            Debug.Log("Collected sin of weight " + weight);
            sinHeld += weight;
            Debug.Log("Total sin collected: " + sinHeld);
        }
    }
}

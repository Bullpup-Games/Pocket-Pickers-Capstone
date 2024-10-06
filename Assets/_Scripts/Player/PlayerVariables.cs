using UnityEngine;
using UnityEngine.Timeline;

namespace _Scripts.Player
{
    public class PlayerVariables : MonoBehaviour
    {
        public bool isFacingRight = true;   // Start facing right by default
        // public bool inCardStance;
        [HideInInspector]public PlayerStateManager stateManager;

        public static PlayerVariables Instance { get; private set; }

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

            stateManager = GetComponent<PlayerStateManager>();
        }
    }
}

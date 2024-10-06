using UnityEngine;
using UnityEngine.Timeline;

namespace _Scripts.Player
{
    public class PlayerVariables : MonoBehaviour
    {
        public bool isFacingRight = true;   // Start facing right by default
        // public bool inCardStance;
        [HideInInspector]public PlayerStateManager stateManager;

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
    }
}

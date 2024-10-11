using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace _Scripts.Player
{
    public class PlayerVariables : MonoBehaviour
    {
        public bool isFacingRight = true;   // Start facing right by default
        // public bool inCardStance;
        [HideInInspector] public PlayerStateManager stateManager;
        [HideInInspector] public Collider2D Collider2D;

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
        
        public void FlipLocalScale()
        {
            var localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;
        }

        private void Awake()
        {
            stateManager = GetComponent<PlayerStateManager>();
            Collider2D = GetComponent<Collider2D>();
        }

        private void Update()
        {
            isFacingRight = transform.localScale.x > 0; 
        }
    }
}

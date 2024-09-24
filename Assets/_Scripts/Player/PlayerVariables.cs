using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerController
{
    public class PlayerVariables : MonoBehaviour
    {
        public bool isFacingRight = true;   // Start facing right by default
        public bool inCardStance;

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
        }
    }
}

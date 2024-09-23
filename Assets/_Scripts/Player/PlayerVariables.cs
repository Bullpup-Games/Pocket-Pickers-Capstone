using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerController
{
    public class PlayerVariables : MonoBehaviour
    {
        public static PlayerVariables Instance;
        public bool isFacingRight = true;   // Start facing right by default
        public bool inCardStance;

        private void Awake()
        {
            if (Instance != null) return;
            Instance = this;
        }
    }
}

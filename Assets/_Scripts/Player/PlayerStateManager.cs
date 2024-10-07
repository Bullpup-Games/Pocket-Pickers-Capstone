using System;
using Unity.VisualScripting;
using UnityEngine;

namespace _Scripts.Player
{
    public class PlayerStateManager : MonoBehaviour
    {
        public PlayerState state;

        public void SetState(PlayerState newState)
        {
            this.state = newState;
        } 
    }
}
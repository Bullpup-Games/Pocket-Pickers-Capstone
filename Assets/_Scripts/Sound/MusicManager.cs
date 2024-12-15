using System;
using UnityEngine;

namespace _Scripts.Sound
{
    public class MusicManager : MonoBehaviour
    {
        // Just keep the object around so it can play music throughout all scenes
        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}

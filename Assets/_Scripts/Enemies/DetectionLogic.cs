using System;
using UnityEngine;

namespace _Scripts.Enemies
{
    public class DetectionLogic : MonoBehaviour
    {
        [Tooltip("True if the guard sees the player but is not yet aggro (building aggression)")]
        public bool isSuspicious;
        [Tooltip("True if this enemy is currently aggro toward the player and in pursuit")]
        public bool isTrackingEnemy;
        [Tooltip("True if a guard has lost track on an enemy they were chasing, and is searching is for them again")]
        public bool isSearchingLastKnownArea;

        private EnemySettings _settings;
        private IViewType _view;
        private void Awake()
        {
            _settings = GetComponent<EnemySettings>();
            _view = GetComponent<IViewType>();
        }

        private void Update()
        {
            _view.SetView();
        }
    }
    

}

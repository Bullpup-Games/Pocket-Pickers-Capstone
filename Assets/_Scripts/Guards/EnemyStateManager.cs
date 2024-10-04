using UnityEngine;
using UnityEngine.Serialization;

namespace _Scripts.Guards
{
    public class EnemyStateManager : MonoBehaviour
    {
        public EnemyState state;
        private IViewType _view;
        
        private void Awake()
        {
            _view = GetComponent<IViewType>();
        }

        private void OnEnable()
        {
            _view.PlayerDetected += HandlePlayerDetected;
        }

        private void OnDisable()
        {
            _view.PlayerDetected -= HandlePlayerDetected;
        }

        private void HandlePlayerDetected()
        {
            state = EnemyState.Detecting;
        }
    }
}
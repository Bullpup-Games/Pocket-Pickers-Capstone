using UnityEngine;

namespace _Scripts
{
    public class GameManager : MonoBehaviour
    {
        #region Singleton

        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType(typeof(GameManager)) as GameManager;

                return _instance;
            }
            set { _instance = value; }
        }

        private static GameManager _instance;

        #endregion
    }
}


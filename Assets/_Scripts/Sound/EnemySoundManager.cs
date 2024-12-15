using UnityEngine;

namespace _Scripts.Sound
{
    public class EnemySoundManager : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip sniperShotClip;
        
        #region Singleton

        public static EnemySoundManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType(typeof(EnemySoundManager)) as EnemySoundManager;

                return _instance;
            }
            set { _instance = value; }
        }

        private static EnemySoundManager _instance;

        #endregion

        public void PlaySniperShotClip()
        {
            if (sniperShotClip is null) return;

            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(sniperShotClip);
        }
    }
}
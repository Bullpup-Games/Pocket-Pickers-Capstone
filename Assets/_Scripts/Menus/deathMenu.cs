using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts.Menus
{
    public class deathMenu : MonoBehaviour
    {
        // Restarts current level
        public void Retry()
        {
            GameManager.Instance.isDead = false;
            GameManager.Instance.deathPanel.SetActive(false);

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // Loads main menu from death screen
        public void MainMenu()
        {
            GameManager.Instance.isDead = false;
            GameManager.Instance.deathPanel.SetActive(false);

            SceneManager.LoadScene("MainMenuPlayTest2");
        }
    }
}
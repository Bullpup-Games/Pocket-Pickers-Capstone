using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartDemo()
    {
        LevelLoader.Instance.LoadLevel("Map");
        // SceneManager.LoadScene("Map");
    }

    public void QuitToDesktop()
    {
        Application.Quit();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void StartDemo()
    {
        Cursor.visible = false;
        LevelLoader.Instance.LoadLevel(LevelLoader.Instance.map);
        // SceneManager.LoadScene("Map");
    }

    public void QuitToDesktop()
    {
        Application.Quit();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; //needed to change scenes 

public class MainMenu : MonoBehaviour
{
    //play button method to load next lvl:
    public void PlayGame()
    {
        //load next lvl:
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1); //next lvl is one after curr lvl in queue
    }

    public void QuitGame()
    {
        //quit out of game:
        Application.Quit();

        //quit out of editor (will need to remove before building):
        //UnityEditor.EditorApplication.isPlaying = false;
    }
}

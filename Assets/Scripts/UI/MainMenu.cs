using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; //needed to change scenes 

//namespace SethScripts.UI
//{
    public class MainMenu : MonoBehaviour
    {
        //make sure cursor is unlocked
        private void Start()
        {
            //unlock cursor and make visible:
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

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

//}


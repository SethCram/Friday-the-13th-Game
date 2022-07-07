using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; //to load scenes
using Photon.Pun; //needed to access photon network

public class EscapePanel : MonoBehaviour
{
    //***init in inspector*****:
    public PausedUI pauseUI;

    #region Button Methods

    //called by resume button:
    public void ResumeButton()
    {
        //tell to close escape menu:
        pauseUI.CloseEscapeMenu();
    }

    /// <summary>
    /// Call game manager's main menu btn method
    /// </summary>
    public void MainMenuButtonWrapper()
    {
        GameManager.Instance.MainMenuButton();
    }

    #endregion Button Methods
}

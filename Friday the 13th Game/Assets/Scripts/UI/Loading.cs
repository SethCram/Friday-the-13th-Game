﻿using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Loading : MonoBehaviour
{
    //for loading
    public GameObject loadingScreen;
    public Slider slider;
    public TMP_Text progressTxt;

    private void Start()
    {
        //start w/ loading screen invisible
        loadingScreen.SetActive(false);
    }

    /// <summary>
    /// Load scene asynchly to allow tracking of load progress
    /// </summary>
    /// <returns></returns>
    public IEnumerator LoadLevelAsynch()
    {
        //set photon network to sync the loading of the next scene
        PhotonNetwork.AutomaticallySyncScene = true;

        //activate loading screen
        loadingScreen.SetActive(true);

        // Temporary disable processing of futher network messages
        PhotonNetwork.IsMessageQueueRunning = false;

        //wait a frame
        yield return null;

        //if master client
        if( PhotonNetwork.IsMasterClient)
        {
            //have multiplayer server asynchly load next scene in build settings:
            PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().buildIndex + 1);
        }

        //check if level loaded yet
        while (PhotonNetwork.LevelLoadingProgress < 1)
        {
            //log progress (breaks it)
            //Debug.Log(PhotonNetwork.LevelLoadingProgress);

            //set to 100% even if only 90% thru clamping
            float progress = Mathf.Clamp01(PhotonNetwork.LevelLoadingProgress / 0.9f);
            slider.value = progress;

            //slider.value = PhotonNetwork.LevelLoadingProgress;


            //if loading is still zero
            if (PhotonNetwork.LevelLoadingProgress == 0)
            {
                slider.fillRect.gameObject.SetActive(false);
            }
            //loading is non zero
            else
            {
                slider.fillRect.gameObject.SetActive(true);
            }


            //set progress percentage
            int progressPercentage = (int)(progress * 100f);

            /*
            //if proggress is 89
            if( progressPercentage == 89 )
            {
                //change progress to 99
                progressPercentage = 99;
            }
            */

            progressTxt.text = progressPercentage.ToString() + "%";

            //wait a frame
            yield return null;
        }

        //turn msging queue back on w/ scene loaded:
        PhotonNetwork.IsMessageQueueRunning = true;
    }
}

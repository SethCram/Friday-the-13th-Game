using Photon.Pun;
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
        //activate loading screen
        loadingScreen.SetActive(true);
        //wait a frame
        yield return null;

        //have multiplayer server asynchly load next scene in build settings:
        PhotonNetwork.LoadLevel(SceneManager.GetActiveScene().buildIndex + 1);

        //check if level loaded yet
        while (PhotonNetwork.LevelLoadingProgress < 1)
        {
            //log progress (breaks it)
            //Debug.Log(PhotonNetwork.LevelLoadingProgress);

            //set to 100% even if only 90% thru clamping
            //float progress = Mathf.Clamp01(PhotonNetwork.LevelLoadingProgress / 0.9f);
            //slider.value = progress;

            slider.value = PhotonNetwork.LevelLoadingProgress;


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
            int progressPercentage = (int)(PhotonNetwork.LevelLoadingProgress * 100f);
            progressTxt.text = progressPercentage.ToString() + "%";

            //wait a frame
            yield return null;
        }
    }
}

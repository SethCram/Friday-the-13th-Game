using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using Photon.Pun;
using System;
//using SethScripts.UI;

namespace Tests
{
    public class TestSceneLoading: MonoBehaviour
    {

        [UnityTest]
        public IEnumerator TestCallingMainMenuToLobby()
        {
            //wait till lobby loaded
            yield return TestLoadingMainMenuToLobby();

            Debug.Log("After coroutine called.");

            Assert.Inconclusive("Requires manual intervention to pass.");
        }

        /// <summary>
        /// Load from the main menu into the lobby using play button function.
        /// By default: verifies we're in lobby scene + Photon Network connected.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator TestLoadingMainMenuToLobby()
        {
            int framesWaited = 0;

            //lobbySceneLoadedAndItsMethodsExed = false;

            //load main menu:
            SceneManager.LoadScene(GameManager.MAIN_MENU_SCENE_NAME);

            //wait for main menu to load
            while (GameManager.Instance.currentScene != GameManager.CurrentScene.MAIN_MENU)
            {
                yield return null;

                framesWaited++;
            }
            Debug.Log($"{framesWaited} frames waited for main menu to load.");
            framesWaited = 0;

            //try and find main menu in scene
            MainMenu mainManu = FindObjectOfType<MainMenu>();

            //while can't find main menu
            while (mainManu == null)
            {
                yield return null;

                framesWaited++;

                //try and find it again
                mainManu = FindObjectOfType<MainMenu>();
            }
            Debug.Log($"{framesWaited} frames waited to find main menu.");
            framesWaited = 0;

            //simulate user clicking play
            mainManu.PlayGame();

            //loading scene also gone thru here

            //wait for lobby to load
            while (GameManager.Instance.currentScene != GameManager.CurrentScene.LOBBY)
            {
                yield return null;
                framesWaited++;
            }
            Debug.Log($"{framesWaited} frames waited for lobby to load.");

            //verify we're in the lobby scene
            VerifyCurrentScene(desiredSceneName: GameManager.LOBBY_SCENE_NAME);

            //verify connnected to Photon network
            VerifyNetworkConnected();
        }

        /// <summary>
        /// Verify we're connected to the Photon Network.
        /// </summary>
        public void VerifyNetworkConnected()
        {
            //pass test if connected to photon network
            Assert.True(PhotonNetwork.IsConnected, "Not connected to the Photon Network.");
            //success msg
            Debug.Log("Connected to the Photon Network.");
        }
    
        /// <summary>
        /// Verify the current and desired scene names are the same.
        /// </summary>
        /// <param name="desiredSceneName"></param>
        public void VerifyCurrentScene(string desiredSceneName)
        {
            string currSceneName = SceneManager.GetActiveScene().name;

            //pass test if current scene matches desired scene
            Assert.True
                (
                    //return true if curr scene name is lobby scene name
                    GameManager.Instance.SameString_IgnoreCase
                    (
                        currSceneName,
                        desiredSceneName
                    ),
                    $"Current scene is {currSceneName} not {desiredSceneName}."
                );
            //success msg
            Debug.Log($"Current scene is {desiredSceneName}.");
        }
    }
}

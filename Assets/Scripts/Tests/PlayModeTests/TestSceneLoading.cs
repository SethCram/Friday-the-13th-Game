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
        int MAX_FRAMES_WAIT = 5000;

        #region Unity Tests

        //[UnityTest]
        //public IEnumerator TestCalling_MainMenuToLobby()
        //{
        //    //wait till lobby loaded
        //    yield return TestLoading_MainMenuToLobby();

        //    Debug.Log($"<color=orange>After coroutine called.</color>");

        //    Assert.Inconclusive("Requires manual intervention to pass.");
        //}

        /// <summary>
        /// Load into the lobby using play button function.
        /// Verifies we're in lobby scene + Photon Network connected.
        /// Assumes not currently in main menu scene, so loads us into it at the start.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator TestLoading_ToLobby()
        {
            //int framesWaited = 0;

            //lobbySceneLoadedAndItsMethodsExed = false;

            //load main menu:
            SceneManager.LoadScene(GameManager.MAIN_MENU_SCENE_NAME);

            //wait for main menu to load
            yield return WaitTillSceneLoadedUsingEnums(GameManager.CurrentScene.MAIN_MENU);

            //wait till obj of type main menu found in scene
            yield return WaitTillTypeFound<MainMenu>();

            //find main menu in scene
            MainMenu mainManu = FindObjectOfType<MainMenu>();

            //simulate user clicking play
            mainManu.PlayGame();

            //loading scene also gone thru here

            //wait for scene to load
            yield return WaitTillSceneLoadedUsingEnums(GameManager.CurrentScene.LOBBY);

            //verify we're in the lobby scene
            VerifyCurrentSceneByStr(desiredSceneName: GameManager.LOBBY_SCENE_NAME);

            //verify connnected to Photon network
            VerifyNetworkConnected();

            Debug.Log($"<color=orange>Main menu to Lobby loaded successfully.</color>");
        }

        /// <summary>
        /// Test loading to game lobby using two coroutines.
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator TestLoading_ToGameLobby()
        {
            //wait till lobby loaded
            yield return TestLoading_ToLobby();

            //wait till game lobby loaded
            yield return Load_LobbyToGameLobby();

            Debug.Log($"<color=orange>Main menu to Game Lobby loaded successfully.</color>");
        }

        [UnityTest]
        public IEnumerator TestLoading_ToGame()
        {
            //wait till loaded into game lobby
            yield return TestLoading_ToGameLobby();

            //wait till loaded into game
            yield return Load_GameLobbyToGame();
        }

        #endregion Unity Tests

        #region Heavy Lifting

        /// <summary>
        /// Load lobby -> game lobby.
        /// Verifies started in lobby scene, game lobby loaded into, player count incrs to 1.
        /// </summary>
        /// <returns></returns>
        private IEnumerator Load_LobbyToGameLobby()
        {
            //verify we're in lobby scene
            VerifyCurrentSceneByStr(desiredSceneName: GameManager.LOBBY_SCENE_NAME);

            //wait till can find create + join rooms script in scene
            yield return WaitTillTypeFound<CreateAndJoinRooms>();

            //find create+join room script in scene
            CreateAndJoinRooms createAndJoinRooms = FindObjectOfType<CreateAndJoinRooms>();

            //simulate user entering room name
            createAndJoinRooms.createInput.text = "generic room name";

            //simulate user pressing Create btn
            createAndJoinRooms.CreateRoom();

            //wait for scene to load
            yield return WaitTillSceneLoadedUsingEnums(GameManager.CurrentScene.GAME_LOBBY);

            //verify we're in the game lobby scene
            VerifyCurrentSceneByStr(desiredSceneName: GameManager.GAME_LOBBY_SCENE_NAME);

            //verify player count incr'd to one
            VerifyPlayerCount(expectedPlayerCount: 1);

            Debug.Log($"<color=orange>Lobby to Game Lobby loaded successfully.</color>");

            yield return new WaitForSeconds(1);
        }

        /// <summary>
        /// Load game lobby -> game scene.
        /// Verifies start in game lobby and game scene loaded.
        /// </summary>
        /// <returns></returns>
        private IEnumerator Load_GameLobbyToGame()
        {
            //verify curr scene is game lobby
            VerifyCurrentSceneByStr(desiredSceneName: GameManager.GAME_LOBBY_SCENE_NAME);

            //simulate user vote 
            //Event.KeyboardEvent("v");

            //wait till overlay UI found in scene
            yield return WaitTillTypeFound<OverlayUI>();

            //activate method used when vote to start given
            OverlayUI overlayUI = FindObjectOfType<OverlayUI>();
            overlayUI.VoteToggleAndIncrStartVotes();

            //wait for scene to load
            yield return WaitTillSceneLoadedUsingEnums(GameManager.CurrentScene.GAME);

            //verify curr scene is game
            VerifyCurrentSceneByStr(desiredSceneName: GameManager.GAME_SCENE_NAME);
        }

        #endregion Heavy Lifting

        #region Wait Methods

        /// <summary>
        /// Wait till desired scene loaded using enums. 
        /// Verify max frames wait not exceeded.
        /// </summary>
        /// <param name="desiredScene">Desired scene enum from GameManager script.</param>
        /// <returns></returns>
        private IEnumerator WaitTillSceneLoadedUsingEnums(GameManager.CurrentScene desiredScene)
        {
            int framesWaited = 0;

            //wait for scene to load
            while (GameManager.Instance.currentScene != desiredScene)
            {
                //wait a frame
                yield return null;

                //incr bc frame waited
                framesWaited++;

                VerifyMaxFramesNotExceeded(framesWaited);
            }
            Debug.Log($"<color=orange>{framesWaited} frames waited for {desiredScene} to load.</color>");
        }

        /// <summary>
        /// Waits till the object with the given type's found in the current scene.
        /// Verify max frames wait not exceeded.
        /// </summary>
        /// <typeparam name="T">
        /// Type looking for in the current scene. 
        /// Constrained to be a UnityEngine Object.
        /// </typeparam>
        /// <returns></returns>
        private IEnumerator WaitTillTypeFound<T>() where T : UnityEngine.Object
        {
            int framesWaited = 0;

            //try and find main menu in scene
            var foundObject = FindObjectOfType<T>();

            //while can't find main menu
            while (foundObject == null)
            {
                yield return null;

                framesWaited++;

                VerifyMaxFramesNotExceeded(framesWaited);

                //try and find it again
                foundObject = FindObjectOfType<T>();
            }
            Debug.Log($"<color=orange>{framesWaited} frames waited to find main menu.</color>");
        }

        #endregion Wait Methods

        #region Verification Methods

        /// <summary>
        /// Verify we're connected to the Photon Network.
        /// </summary>
        public void VerifyNetworkConnected()
        {
            //pass test if connected to photon network
            Assert.True(PhotonNetwork.IsConnected, "Not connected to the Photon Network.");
            //success msg
            Debug.Log($"<color=orange>Connected to the Photon Network.</color>");
        }
    
        /// <summary>
        /// Verify the current and desired string scene names are the same.
        /// </summary>
        /// <param name="desiredSceneName"></param>
        public void VerifyCurrentSceneByStr(string desiredSceneName)
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
            Debug.Log($"<color=orange>Current scene is {desiredSceneName}.</color>");
        }

        /// <summary>
        /// Verify player count comparing expected to actual 
        ///  using Photon player count in curr room.
        /// </summary>
        /// <param name="expectedPlayerCount">Expected number of players in curr room.</param>
        public void VerifyPlayerCount(int expectedPlayerCount)
        {
            //cache actual player count
            int actualPlayerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            
            //cache feedback msg bc success + failure same
            string feedbackMsg = $"<color=orange>Expected player count = {expectedPlayerCount}, " + 
                $"Actual player count = {actualPlayerCount}</color>";

            //verify expected + actual are equal
            Assert.AreEqual
                (
                    expected: expectedPlayerCount, 
                    actual: actualPlayerCount, 
                    feedbackMsg
                );
            //success msg
            Debug.Log(feedbackMsg);
        }

        public void VerifyMaxFramesNotExceeded(int framesWaited)
        {
            //verify current frames waited less than maximum wait lim
            Assert.Less
                (
                    framesWaited, 
                    MAX_FRAMES_WAIT, 
                    $"Frames waited exceeds the maximum {MAX_FRAMES_WAIT} allowed. " +
                        $"Please increase the max or look into the problem."
                );
        }

        #endregion Verification Methods

        /// <summary>
        /// Teardown after each test through disconnecting from Photon.
        /// </summary>
        [TearDown]
        public void Loading_Teardown()
        {
            //unload the finishing scene
            //SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());

            //disconnect from Photon
            PhotonNetwork.Disconnect();

            Debug.Log("<color=orange>Disconnect from Photon.</color>");
        }
    }
}

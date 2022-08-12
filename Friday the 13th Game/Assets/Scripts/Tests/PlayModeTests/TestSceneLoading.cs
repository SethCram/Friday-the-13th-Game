using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
//using SethScripts.UI;

namespace Tests
{
    public class TestSceneLoading: MonoBehaviour
    {
        // A Test behaves as an ordinary method
        [Test]
        public void TestDyingSimplePasses()
        {
            // Use the Assert class to test conditions
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator TestLoadingMainMenuToLobby()
        {
            int framesWaited = 0;

            //load main menu if no longer connected:
            SceneManager.LoadScene(GameManager.MAIN_MENU_SCENE_NAME);

            //wait for main menu to load
            while(GameManager.Instance.currentScene != GameManager.CurrentScene.MAIN_MENU)
            {
                yield return null;

                framesWaited++;
            }
            Debug.Log($"{framesWaited} frames waited for main menu to load.");
            framesWaited = 0;

            //try and find main menu in scene
            MainMenu mainManu = FindObjectOfType<MainMenu>();

            //while can't find main menu
            while(mainManu == null)
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

            //wait for lobby to load
            while (GameManager.Instance.currentScene != GameManager.CurrentScene.LOBBY)
            {
                yield return null;
                framesWaited++;
            }
            Debug.Log($"{framesWaited} frames waited for lobby to load.");

            // Use the Assert class to test conditions.
            Assert.True(true, "Connected to Lobby.");
        }
    }
}

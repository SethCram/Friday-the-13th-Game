using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
//using SethScripts.UI;

namespace Tests
{
    public class TestDying: MonoBehaviour
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
        public IEnumerator TestDyingWithEnumeratorPasses()
        {
            //load main menu if no longer connected:
            SceneManager.LoadScene(0);

            //wait for main menu to load
            yield return new WaitForSeconds(1);

            //can't find other classes bc of assembly issues
            //FindObjectOfType<MainMenu>()

            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}

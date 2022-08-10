using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameIntro : MonoBehaviour
{
    public GameObject gameIntroPanel;
    public GameObject jasonTextGameObj;
    public GameObject counselorTextGameObj;

    private bool localReady = false;

    // Start is called before the first frame update
    void Start()
    {
        //unlock cursor + make visible
        GameManager.Instance.UnlockCursor();

        //start w/ intro panel deactive
        GameManager.Instance.HideGameIntroPanel();

        StartCoroutine(ShowIntroWhenCustomPropsSet());
    }

    /// <summary>
    /// Show intro panel when the custom props are filled.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShowIntroWhenCustomPropsSet()
    {
        //while custom props not set + on network
        while( !GameManager.Instance.GetWhetherCustomPropsSet() 
            && PhotonNetwork.IsConnected )
        {
            Debug.Log("Custom props not set yet so waited a frame.");

            //wait a frame
            yield return null;
        }

        //if network is connected
        if( PhotonNetwork.IsConnected )
        {
            //cache local player
            Player localPlayer = PhotonNetwork.LocalPlayer;

            //if our player is jason
            if (GameManager.Instance.PlayerIsJason(localPlayer))
            {
                ShowJasonText_HideCounselorText();
            }
            //if our player is counselor
            else if (GameManager.Instance.PlayerIsCounselor(localPlayer))
            {
                ShowCounselorText_HideJasonText();
            }
            //if local player isn't counselor or jason
            else
            {
                Debug.LogError("Custom props not set yet, so can't change intro text.");
            }
        }
        //if network not connected
        else
        {
            //if local player is jason
            if(GameManager.Instance.localPlayerIsJason)
            {
                ShowJasonText_HideCounselorText();
            }
            //if local player isnt jason
            else
            {
                //assume local player is counselor
                ShowCounselorText_HideJasonText();
            }

        }

        //start game w/ intro active
        GameManager.Instance.ShowGameIntroPanel();
    }

    /// <summary>
    /// Local player is jason so activate jason txt + disable counselor txt.
    /// </summary>
    public void ShowJasonText_HideCounselorText()
    {
        jasonTextGameObj.SetActive(true);

        counselorTextGameObj.SetActive(false);
    }

    /// <summary>
    /// Local player is counselor so activate counselor txt + disable jason txt.
    /// </summary>
    public void ShowCounselorText_HideJasonText()
    {
        jasonTextGameObj.SetActive(false);

        counselorTextGameObj.SetActive(true);
    }

    /// <summary>
    /// Method called by ready button to incr players done reading intro.
    /// </summary>
    public void ReadyButton()
    {
        //if not already ready locally
        if( !localReady )
        {
            Debug.Log("Done reading intro button pressed.");

            //if networked
            if (PhotonNetwork.IsConnected)
            {
                //incr # of players game ready on network
                GameManager.Instance.photonView.RPC("RPC_IncrPlayersSpawnReady", RpcTarget.AllBuffered);
            }
            //if local
            else
            {
                //call method locally
                GameManager.Instance.RPC_IncrPlayersSpawnReady();
            }

            //locally ready now
            localReady = false;
        }
    }
}

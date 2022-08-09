using Photon.Pun;
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
        //start w/ intro panel deactive
        gameIntroPanel.SetActive(false);

        StartCoroutine(ShowIntroWhenCustomPropsSet());
    }

    /// <summary>
    /// Show intro panel when the custom props are filled.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShowIntroWhenCustomPropsSet()
    {
        //while custom props not set + on network
        while( !GameManager.Instance.customPropsSet 
            && PhotonNetwork.IsConnected )
        {
            Debug.Log("Custom props not set yet so waited a frame.");

            //wait a frame
            yield return null;
        }

        //if network is connected
        if( PhotonNetwork.IsConnected )
        {
            //if local player is jason
            if (GameManager.Instance.PlayerIsJason(PhotonNetwork.LocalPlayer))
            {
                LocalPlayerIsJason();
            }
            //if local player is counselor
            else if (GameManager.Instance.PlayerIsCounselor(PhotonNetwork.LocalPlayer))
            {
                LocalPlayerIsCounselor();
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
                LocalPlayerIsJason();
            }
            //if local player isnt jason
            else
            {
                //assume local player is counselor
                LocalPlayerIsCounselor();
            }

        }

        //start game w/ intro active
        gameIntroPanel.SetActive(true);
    }

    /// <summary>
    /// Local player is jason so activate jason txt + disable counselor txt.
    /// </summary>
    public void LocalPlayerIsJason()
    {
        jasonTextGameObj.SetActive(true);

        counselorTextGameObj.SetActive(false);
    }

    /// <summary>
    /// Local player is counselor so activate counselor txt + disable jason txt.
    /// </summary>
    public void LocalPlayerIsCounselor()
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

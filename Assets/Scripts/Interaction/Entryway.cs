using Photon.Pun;
using UnityEngine;

/// <summary>
/// subclass of Interactable and superclass of any entryway
/// </summary>
public class Entryway : Interactable
{
    //pickups in container
    public GameObject[] pickups;

	private const string audioClipNameOpen = "Open";
	private const string audioClipNameClose = "Close";

	private AudioClip audioClipOpen;
	private AudioClip audioClipClose;

	public override void Start()
	{
		//cache sounds dict open sound
		Sound soundsDictOpenSound = AudioManager.instance.soundsDict[audioClipNameOpen];

		//set open+close clips
		audioClipOpen = soundsDictOpenSound.source.clip;
		audioClipClose = AudioManager.instance.soundsDict[audioClipNameClose].source.clip;

		//fill audio src copied over from audio manager
		addedAudioSrc = gameObject.AddComponent<AudioSource>(soundsDictOpenSound.source);

		base.Start();
	}


	/// <summary>
	/// Play entryway closing/opening sound and invert the state of the entryway
	/// </summary>
	/// <param name="playerInteracting">Player that triggered interaction.</param>
	public override void Interact(Transform playerInteracting)
	{
        //if audio clip playing
        if (addedAudioSrc.isPlaying)
        {
            //stop it from playing
            addedAudioSrc.Stop();

            Debug.Log("Audio had to be stopped to change clip.");
        }

        //if entryway open and closing
        if (isOpen)
        {
            addedAudioSrc.clip = audioClipClose;
        }
        //if entrway closing and opening
        else
        {
            addedAudioSrc.clip = audioClipOpen;
        }

        //play audio
        addedAudioSrc.Play();

        base.Interact(playerInteracting); //calls 'Interactable' Interact() method

		//should invert state of entryway
		if (PhotonNetwork.IsConnected)
		{
			//over network
			//photonView.RPC("RPC_InvertPickups", RpcTarget.AllBufferedViaServer, !isOpen);
			photonView.RPC("RPC_InvertPickups", RpcTarget.AllBufferedViaServer);

		}
		else
		{
			//local
			RPC_InvertPickups();
		}

		//addedAudioSrc.Play();

	}

	[PunRPC]
	public void RPC_InvertPickups()
    {
		//if no pickups
		if( pickups == null || pickups.Length == 0)
        {
			//dont invert anything
			return;
        }

		//for each pickup
		foreach (GameObject pickup in pickups)
		{
			//make sure pickup hasnt been removed
			if( pickup != null)
            {
				//invert its state
				pickup.SetActive(!pickup.activeSelf);
			}
		}
	}
}

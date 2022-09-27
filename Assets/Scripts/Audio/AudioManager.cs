/*
* Filename: AudioManager.cs
* Developer: Chadwick Goodall
* Purpose: This file contains the code for the audio manager singleton
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/*
* Summary: The AudioManager class which manages SFX
*
* Member Variables:
* sounds - a list of sound effects
* instance - the singleton
*/
public class AudioManager : MonoBehaviour
{
    //public Sound[] sounds;
    [SerializeField] private Sound[] soundsArr;
    public Dictionary<string, Sound> soundsDict;

    //sound clip names for dict calling
    public const string punchSoundClipName = "Punch";
    public const string openAudioClipName = "Open";
    public const string closeAudioClipName = "Close";
    public const string equipAudioClipName = "Equip";
    public const string unequipAudioClipName = "Unequip";
    public const string pickupAudioClipName = "Pickup";
    public const string dropAudioClipName= "Drop";
    public const string hurtAudioClipName = "Hurt";
    public const string dieAudioClipName = "Die";
    public const string gameOverAudioClipName = "Game Over";

    public AudioMixerGroup outputAudioMixerGroup;

    public static AudioManager instance;

    /*
    * Summary: Initialize an AudioManager singleton
    *
    * Parameters:
    * none
    *
    * Returns:
    * none
    */
    void Awake()
    {
        // Singleton code for AudioManager
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        //don't need persistence across scenes
        //DontDestroyOnLoad(gameObject);

        //init new dict
        soundsDict = new Dictionary<string, Sound>();

        foreach (Sound s in soundsArr)
        {
            //could only add audio src if "music" is in name (but then audio src would need creating instead of copying)
            //if(s.name.Contains("music", StringComparison.OrdinalIgnoreCase))
            //{

            //}

            //create an audio src attached to audio manager for ever sound (incase need to play from audio manager)
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
            s.source.spatialBlend = s.spatialBlend;
            s.source.maxDistance = s.maxHearingDistance;
            s.source.outputAudioMixerGroup = outputAudioMixerGroup;

            //don't ever play on awake
            s.source.playOnAwake = false;

            //dynamically construct sounds dict on startup
            soundsDict[s.name] = s;
        }
    }

    /// <summary>
    /// On startup, play music if main menu or lobby. If main menu, also set volume to saved volume.
    /// </summary>
    private void Start()
    {
        bool onMainMenu = GameManager.Instance.currentScene == GameManager.CurrentScene.MAIN_MENU;

        //if on beginning menus
       if ( GameManager.Instance.currentScene == GameManager.CurrentScene.LOBBY || 
            onMainMenu )
        {
            Play("Menu Music");
        }

       //if on main menu
       if( onMainMenu)
        {
            //set options menu initial val
            OptionsMenu optionsMenu = FindObjectOfType<OptionsMenu>(includeInactive: true);
            optionsMenu.SetVolume(optionsMenu.volumeSlider.value);
        }

    }

    /*
    * Summary: Play a sound clip
    *
    * Parameters:
    * name - name of the sound clip
    *
    * Returns:
    * none
    */
    public void Play (string name)
    {
        //Sound s = Array.Find(sounds, sound => sound.name == name);

        Sound s = soundsDict[name];

        // don't do anything if the sound doesn't exist
        if (s == null)
        {
            Debug.LogWarning($"Couldn't find {s.name} sound to Play.");

            return;
        }
        s.source.Play();
    }

    public void StopPlaying(string sound)
    {
        //Sound s = Array.Find(sounds, item => item.name == sound);

        Sound s = soundsDict[sound];

        if (s == null)
        {
            Debug.LogWarning("Sound: " + name + " not found!");
            return;
        }

        //s.source.volume = s.volume * (1f + UnityEngine.Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
        //s.source.pitch = s.pitch * (1f + UnityEngine.Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));

        s.source.Stop();
    }
}

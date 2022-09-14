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
    public Sound[] sounds;

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

        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    private void Start()
    {
        //if on beginning menus
       if( GameManager.Instance.currentScene == GameManager.CurrentScene.LOBBY || 
            GameManager.Instance.currentScene == GameManager.CurrentScene.MAIN_MENU )
        {
            Play("Menu Music");
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
        Sound s = Array.Find(sounds, sound => sound.name == name);
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
        Sound s = Array.Find(sounds, item => item.name == sound);
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

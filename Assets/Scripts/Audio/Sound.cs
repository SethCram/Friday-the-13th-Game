/*
* Filename: Sound.cs
* Developer: Chadwick Goodall
* Purpose: This file contains the class for a sound object
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/*
* Summary: The Sound class which stores all of the data for a sound clip
*
* Member Variables:
* AudioClip - the sound clip to play
* volume - the volume of the SFX
* pitch - the pitch of the SFX
* loop - whether the sound will loop
* source - the audio source that plays the sound
*/
[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1.0f)]
    public float volume = 1f;
    [Range(.1f,3f)]
    public float pitch = 1f;

    public bool loop = false;

    [HideInInspector]
    public AudioSource source;
}

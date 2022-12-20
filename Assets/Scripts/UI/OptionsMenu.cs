using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio; //needed to access 'AudioMixer' type
using UnityEngine.UI; //needed to add 'Dropdown' options thru script (also needed to access any UI objs)
using TMPro; //need for 'TMP_Dropdown' since I used a text mesh pro dropdown

public class OptionsMenu : MonoBehaviour
{
    #region vars

    public AudioMixer audioMixer; //to set volume

    public TMP_Dropdown resolutionDropdown; //to add resolution dropdown options

    private Resolution[] resolutions; //to store screen resolutions

    //to init vals using player prefs:
    public Slider volumeSlider;
    public TMP_Dropdown qualityDropdown; 

    //player pref names:
    private const string resName = "resolutionIndex";
    private const string volName = "volumeFloat";
    private const string qualityName = "qualityIndex";

    public Toggle fullscreenToggle;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        //set options menu deactive 
        gameObject.SetActive(false);

       //set toggle to current state of game
        fullscreenToggle.isOn = Screen.fullScreen;

        //Debug.Log("Options menu awake called");

        //if not first time setting volume:
        if (PlayerPrefs.GetFloat(volName, -1) != -1)
        {
            //set slider to saved volume:
            volumeSlider.value = PlayerPrefs.GetFloat(volName); //hopefully this also updates audiomixer?
        }

        //if not first time setting quality:
        if (PlayerPrefs.GetInt(qualityName, -1) != -1)
        {
            //set saved quality to curr one:
            qualityDropdown.value = PlayerPrefs.GetInt(qualityName);
        }
    }

    private void Start()
    {
        //store all screen resolutions of curr device:
        resolutions = Screen.resolutions;

        //clear out default options on dropdown:
        resolutionDropdown.ClearOptions();

        int currResolutionIndex = 0; //used to keep track of correct resolution index

        //transfer 'resolutions' array into a list of strings:
        List<string> options = new List<string>(); // need to init list before filling it
        for (int i = 0; i < resolutions.Length; i++)
        {
            //option = width x height:
            string optionString = resolutions[i].width + " x " + resolutions[i].height;

            //compare curr resolution option w/ the screen play window resolution (cant compare directly so compare height and width)(dont use 'Screen.currentResolution' bc it gets whole monitor resolution):
            if(resolutions[i].width == Screen.width && 
                resolutions[i].height == Screen.height)
            {
                currResolutionIndex = i; 
            }

            //add converted string of the resolution into the options list:
            options.Add(optionString);
        }

        //add options to dropdown as a list of strings:
        resolutionDropdown.AddOptions(options);

        //if first time setting resolution:
        if (PlayerPrefs.GetInt(resName, -1) == -1)
        {
            print("Set brand new res.");

            //set correct screen resolution as default:
            resolutionDropdown.value = currResolutionIndex;

            //set resolution for further sessions as current resolution:
            PlayerPrefs.SetInt(resName, currResolutionIndex);
        }
        else
        {
            //set resolution as stored:
            resolutionDropdown.value = PlayerPrefs.GetInt(resName);
        }

        //refresh it to update screen:
        resolutionDropdown.RefreshShownValue();
    }

    #endregion

    #region Option Setting Methods

    //set resolution using drop down menu:
    public void SetResolution(int resolutionIndex)
    {
        //convert given resolution into its width and height using the index:
        Resolution res;
        res = resolutions[resolutionIndex];

        Screen.SetResolution(res.width, res.height, Screen.fullScreen); //keeps whether currently fullscreen or not using 'Screen.fullscreen'

        //set resolution for further sessions as current resolution:
        PlayerPrefs.SetInt(resName, resolutionIndex);
    }

    //set volume with its arg passed in by volume slider:
    public void SetVolume(float desiredVolume) //unity feeds in curr value of slider as 'volume' here when this funct set 'on val change'
    {
        //Debug.Log("volume desired: " + desiredVolume);

        float setVolume;

        //if volume small enough
        if( desiredVolume < 0.05 )
        {
            //set desired vol to a threshold below human hearing
            setVolume = -75;
        }
        //if volume isn't tiny
        else
        {
            //use Math class bc the audioMixer volume isn't linear
            setVolume = Mathf.Log10(desiredVolume) * 20;
        }

        //set volume
        audioMixer.SetFloat("volume", setVolume); 

        Debug.Log("Volume set as: " + setVolume + " db");

        //save desired volume bc slider set directly
        PlayerPrefs.SetFloat(volName, desiredVolume);
    }

    //set GFX with its index passed in by graphics dropdown:
    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);

        Debug.Log("Curr Quality Level: " + QualitySettings.GetQualityLevel());

        //save quality index:
        PlayerPrefs.SetInt(qualityName, qualityIndex);
    }

    //set screen as fullscreen based on whether toggled on: 
    public void SetFullscreen(bool wantFullscreen)
    {
        Screen.fullScreen = wantFullscreen;
    }

    #endregion
}

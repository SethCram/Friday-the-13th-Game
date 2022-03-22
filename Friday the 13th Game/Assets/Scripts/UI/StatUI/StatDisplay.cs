using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StatDisplay : MonoBehaviour
{
    public TMP_Text statName; //stat name dropped from obj

    public Slider slider; //used by 'GetSetStats' script to stop slider from going too high if not any pnts left

    public TMP_Text statValue; //txt stat val dragged in from obj

    //set stat val from slider:
    public void GetFromSlider(float sliderVal)
    {
        //cast slider float to int:
        int intSliderVal = (int)sliderVal;

        //cast stat val to txt:
        statValue.text = "" + intSliderVal;
    }
}

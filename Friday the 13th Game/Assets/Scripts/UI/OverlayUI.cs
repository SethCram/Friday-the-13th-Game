using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class OverlayUI : MonoBehaviour
{
    public TMP_Text interactTxt;
    public GameObject minimap;

    //for slider
    public Slider healthSlider;
    public TMP_Text healthRatio;
    public GameObject fillAmt;

    private void Awake()
    {
        //start w/ interact txt off
        interactTxt.gameObject.SetActive(false);

        //start w/ minimap off bc driven by if perception in range
        minimap.SetActive(false);

        //start w/ health slider off
        healthSlider.gameObject.SetActive(false);
    }

    //update health slider using HP changed event
    public void UpdateHealthSlider(int maxHP, int currHP)
    {
        print("Update HP slider");

        //if no HP left
        if( currHP <= 0)
        {
            //delete fill amt
            fillAmt.SetActive(false);
        }

        //update filled amt (float casts needed)
        healthSlider.value = (float)currHP / (float)maxHP;

        //update ratio text
        healthRatio.text = currHP + "/" + maxHP;
    }
}

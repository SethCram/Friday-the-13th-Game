using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq; //needed for '.Concat()'
using System.Globalization;
using System;

public class GetSetStats : MonoBehaviour
{
    //set in ui initer:
    public GameObject pausedUICanvas;
    public StatApplication applyStats;

    //total number text:
    public TMP_Text numTotalText;      //(init in inespector)
    private int maxPnts;
    private int newMaxPnts;

    //panel gameobjs:    (init in inespector)
    public GameObject leftPanel;
    public GameObject rightPanel;

    //left and right array stat val scripts:
    private StatDisplay[] leftStatArray;
    private StatDisplay[] rightStatArray;
    private StatDisplay[] combinedStatArray;

    //use this to set actual player stats:
    public int[] statVals;

    public PlayerStats playerStats; //used to set Stat names, and fill player stat vals  (init w/ created?)

    //arr of not yet implemented stats: (filled in inspector)
    public string[] notImplementedStats;

    //pause game before any 'Start()' called (pausing time scale wont stop start from bein called tho, only update)
    private void Awake()
    {
        //pause game:
        Time.timeScale = 0; //doesnt work correctly
    }

    // Start is called before the first frame update
    void Start()
    {
        //init total max pnts of this character:
        maxPnts = int.Parse(numTotalText.text);

        //fill left array:
        leftStatArray = leftPanel.GetComponentsInChildren<StatDisplay>();

        //fill right array:
        rightStatArray = rightPanel.GetComponentsInChildren<StatDisplay>();

        //combine left and right array to make combined stat array:
        combinedStatArray = leftStatArray.Concat(rightStatArray).ToArray();    //so we dont have to loop thru 2 dif arrays

        //set stat names from player stats 'name' field:
        for (int i = 0; i < combinedStatArray.Length; i++)
        {
            //set stat names from player stats 'name' field
            combinedStatArray[i].statName.text = playerStats.allStats[i].name;

            //if stat not yet implemented, should be highlighted in red
            foreach (string uselessStat in notImplementedStats)
            {
                //if stat is a useless one
                // perform case-insensitive string comparison, turning symbols + whitespace into nothing: (merges chars)
                if (String.Compare(playerStats.allStats[i].name, uselessStat, CultureInfo.CurrentCulture,
                    CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols) == 0)
                {
                    //turn display name red
                    combinedStatArray[i].statName.color = Color.red;
                }
            }

        }
    }

    // Update is called once per frame
    void Update()
    {
        //init new max pnts to max pnt total at start of every frame:
        newMaxPnts = maxPnts;

        //loop thru both left and right panel stat arrays:
        foreach (StatDisplay statDisplay in combinedStatArray)
        {
            //set curr stat val as curr stat val txt:
            int currStatVal = int.Parse(statDisplay.statValue.text);

            //subtract stat value txt from max pnts:
            newMaxPnts -= currStatVal;

            if (newMaxPnts < 0)
            {
                //difference tween new maxpnt and zero: 
                int difference = 0 - newMaxPnts;
                int newStatVal;

                //default max pnts to 0:
                newMaxPnts = 0;

                //set new stat val:
                newStatVal = currStatVal - difference;

                //set txt's new stat val:
                statDisplay.statValue.text = "" + newStatVal;

                //set slider's new val:
                statDisplay.slider.value = newStatVal;
            }

            //set total txt to max pnts:
            numTotalText.text = "" + newMaxPnts;
        }
    }

    //called w/ press done button:
    public void DoneButton()
    {
        //set stat vals to each stat's base value for the game:
        for (int i = 0; i < combinedStatArray.Length; i++)
        {
            Stat currStat = playerStats.allStats[i];

            currStat.baseValue = int.Parse(combinedStatArray[i].statValue.text);

            //if curr stat not 0, signify this stat is being changed and call aprop methods:
            if(currStat.baseValue != 0)
            {
                applyStats.onStatChangedCallback.Invoke(currStat);
            }
        }

        //enable time:
        Time.timeScale = 1;

        //lock cursor and make invisible:
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //enable paused UI:
        pausedUICanvas.SetActive(true);

        //destroy our stats setting UI:
        Destroy(gameObject);
    }
}

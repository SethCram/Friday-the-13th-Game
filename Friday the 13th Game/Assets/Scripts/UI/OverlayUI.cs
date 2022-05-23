using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OverlayUI : MonoBehaviour
{
    public TMP_Text interactTxt;

    private void Awake()
    {
        //start w/ interact txt off
        interactTxt.gameObject.SetActive(false);
    }
}

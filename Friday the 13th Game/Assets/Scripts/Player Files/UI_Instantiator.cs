﻿using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

//require all scripts required to correctly implement the Pause UI:
[RequireComponent(typeof(Inventory))]
[RequireComponent(typeof(PlayerButtons))]
[RequireComponent(typeof(EquipmentManager))]
[RequireComponent(typeof(PlayerManager))]
[RequireComponent(typeof(Cam_Instantiator))]
public class UI_Instantiator : MonoBehaviour
{
    //for multiplayer:
    public PhotonView photonView;

    //to instantiate a pause UI per person:
    public GameObject pausedUICanvasPrefab;
    private PausedUI pausedUIScript;
    private PausedUICallbacks pausedUICallbacks;

    //to instantiate a stats config menu per person:
    public GameObject statsUIPrefab;
    private GetSetStats statSetter;

    //to instantiate an overlay canvas per person:
    public GameObject overlayUICanvasPrefab;

    //for player icon
    public GameObject playerIconObj;
    public Sprite playerIcon;

    public PlayerManager playerManager;
    public StatApplication statApplication;

    // awake called before start, and sometimes we need comps in start:
    void Awake()
    {
        //if this obj isnt mine and we connected to the photon network:
        if (!(photonView.IsMine) && PhotonNetwork.IsConnected)
        {
            //dont execute any spawning
            return;
        }

        //init pause canvas UI for this character:

        //init script:
        pausedUIScript = pausedUICanvasPrefab.GetComponent<PausedUI>();
        pausedUIScript.playerManager = playerManager; //GetComponent<PlayerManager>();
        //pausedUIScript.playerButtons = GetComponent<PlayerButtons>();

        //init callback script:
        pausedUICallbacks = pausedUICanvasPrefab.GetComponent<PausedUICallbacks>();
        pausedUICallbacks.playerInventory = GetComponent<Inventory>();
        pausedUICallbacks.equipManager = GetComponent<EquipmentManager>();
        pausedUICallbacks.charStats = GetComponent<CharacterStats>();
        pausedUICallbacks.statApply = statApplication; //GetComponent<StatApplication>();

        //start paused canvas as inactive:
        pausedUICanvasPrefab.SetActive(false);
        
        //instantiate player's pause UI:
        GameObject pausedUICopy = Instantiate(pausedUICanvasPrefab);


        //init player stats and paused UI for 'GetSetStats' script:
        statSetter = statsUIPrefab.GetComponent<GetSetStats>();
        statSetter.pausedUICanvas = pausedUICopy;
        statSetter.playerStats = GetComponent<PlayerStats>();
        statSetter.applyStats = statApplication; //GetComponent<StatApplication>();
        statSetter.playerManager = playerManager;

        //start stats UI canvas as active:
        statsUIPrefab.SetActive(true);

        //instantiate player's stats UI:
        Instantiate(statsUIPrefab);

        //OVERLAY

        //start overlay UI canvas as active:
        overlayUICanvasPrefab.SetActive(true);

        //create player's overlay canvas:
        GameObject overlayCopy = Instantiate(overlayUICanvasPrefab);
        OverlayUI overlayUICopy = overlayCopy.GetComponent<OverlayUI>();
        overlayUICopy.playerManager = playerManager;

        //fill player manager's overlay UI field
        playerManager.overlayUI = overlayUICopy; //overlayCopy.GetComponent<OverlayUI>();

        //fill minimap UI obj using the mask attached to the minimap
        playerManager.minimapUI = overlayUICopy.minimap; //overlayCopy.GetComponentInChildren<Mask>(includeInactive:true).gameObject;

        //fill player icon in
        playerIconObj.GetComponent<SpriteRenderer>().sprite = playerIcon;

        //fill character stats overlay UI
        GetComponent<CharacterStats>().overlayUI = overlayUICopy;

        //fill stat application overlay UI
        statApplication.overlayUI = overlayUICopy;
    }
}

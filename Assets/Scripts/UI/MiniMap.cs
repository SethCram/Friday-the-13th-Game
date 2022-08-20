using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMap : MonoBehaviour
{
    public Transform player;
    public float yOffset = 10;

    /*
    public Light[] SoftLights = new Light[1];
    public Light[] HardLights;
    void Start()
    {
        if (gameObject.GetComponent<Camera>() == null) { Debug.Log("No Camera Found"); }

        SoftLights[0] = GameObject.FindGameObjectWithTag("Light").GetComponent<Light>();
    }

    void OnPreRender()
    {
        foreach (Light l in SoftLights) { l.shadows = LightShadows.None; }
        foreach (Light l in HardLights) { l.shadows = LightShadows.None; }
    }

    void OnPostRender()
    {
        foreach (Light l in SoftLights) { l.shadows = LightShadows.Soft; }
        foreach (Light l in HardLights) { l.shadows = LightShadows.Hard; }
    }
    */

    private void LateUpdate()
    {
        //set new pos to player's pos + desired y offset
        Vector3 newPos = player.position + Vector3.up * yOffset;

        //apply new position
        transform.position = newPos;

        //set cam to rot w/ player
        //transform.rotation = Quaternion.Euler(90f, player.eulerAngles.y, 0f);
    }
}

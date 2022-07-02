using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 50; //not a stat bc don't want it affected by modifiers
    public int currHealth; //{ get; private set; } (needs to be settable from 'StatApplications' for bulk stat //can only set thru this class, but can be retrieved by any class 
    public int baseHealth = 50; //never changes for this char

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

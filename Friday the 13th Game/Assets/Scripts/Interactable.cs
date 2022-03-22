using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(SphereCollider))] //needed to act as trigger for interaction
public class Interactable : MonoBehaviourPunCallbacks
{
    public float interactionRadius = 3f; // distance player has to be from obj to interact with it

    public Transform interactionTransform; //if want interaction pnt to be not around obj, set this to desired pnt

    private SphereCollider sphereCollider; //to setup sphere trigger

    //to store all players able to interact with this obj:
    private List<Transform> interactablePlayers;

    private void Start()
    {
        //setup sphere trigger:
        sphereCollider = GetComponent<SphereCollider>();

        //set sphere's trigger radius to interaction radius:
        sphereCollider.radius = interactionRadius;

        sphereCollider.isTrigger = true;

        //if no interactable point set, set it to this obj:
        if (interactionTransform == null)
        {
            interactionTransform = transform;
        }

        //init list of transforms:
        interactablePlayers = new List<Transform>();
    }

    //arg of transform is player interacting with this:
    public virtual void Interact(Transform playerInteracting) //'virtual' so this method can be overwritten in any children classes of this class (dif for each child class)
    {
        //this method meant to be overwritten

        Debug.Log("Interacting w/: " + transform.name);
    }

    private void Update()
    {
        //if any interactable players:
        if (interactablePlayers.Count > 0)
        {
            //if a player within radius pressed the 'Interact' button, call 'Interact()':
            foreach (Transform player in interactablePlayers)
            {
                //if player deleted:
                if(player == null)
                {
                    //remove player from being able to interact:
                    interactablePlayers.Remove(player);
                }

                if(player.GetComponent<PlayerButtons>().playerInteract)
                {
                    Interact(player);

                    //could add a private 'interacted' bool to make sure only 1 player picks up item, if becomes a problem 
                }
            }
        }
    }

    //add player to interactable players list w/ enter radius:
    private void OnTriggerEnter(Collider other)
    {
        //only add an interactable player if tagged w/ 'Player' or 'Enemy'
        if(other.tag != "Player" && other.tag != "Enemy")
        {
            return;
        }

        interactablePlayers.Add(other.transform);

        //Debug.Log("Added: " + other.transform.name);
    }

    //remove player from interactable players list w/ exit radius:
    private void OnTriggerExit(Collider other)
    {
        //only remove an interactable player if tagged w/ 'Player' or 'Enemy'
        if (other.tag != "Player" && other.tag != "Enemy")
        {
            return;
        }

        interactablePlayers.Remove(other.transform);

        //Debug.Log("Removed: " + other.transform.name);
    }
}

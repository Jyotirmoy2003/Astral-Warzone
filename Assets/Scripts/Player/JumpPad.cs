using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour,IInteractable
{
    [SerializeField] float force = 5f;
    

    public void OnTriggerStay(Collider info)
    {
        if(info.CompareTag("Player"))
            Interact(info.gameObject);
    }

    public void Interact(GameObject player)
    {
        player.GetComponent<PlayerController>().Jump(force);
       
    }
}

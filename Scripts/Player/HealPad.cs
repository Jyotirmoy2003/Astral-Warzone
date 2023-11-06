using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealPad : MonoBehaviour
{
    [SerializeField] float amountHeal = 5f;


    public void OnTriggerStay(Collider info)
    {
        if (info.CompareTag("Player"))
            Interact(info.gameObject);
    }

    public void Interact(GameObject player)
    {
        player.GetComponent<PlayerMachanics>().Heal(amountHeal * Time.deltaTime);

    }
}

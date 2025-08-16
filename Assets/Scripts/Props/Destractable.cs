using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Destractable : MonoBehaviourPun, IDamageable
{
    [SerializeField] PhotonView pv;
    [SerializeField] GameObject destractedVesion;
    [SerializeField] float removeTime = 5f;
    private GameObject instantiatedObject;
    

    [PunRPC]
    void Destract()
    {
        instantiatedObject = Instantiate(destractedVesion, transform.position, transform.rotation);
        gameObject.SetActive(false);
        Invoke(nameof(RemoveDebries), removeTime);
    }
    public void TakeDamage(float amount)
    {
        pv.RPC(nameof(Destract),RpcTarget.All);
    }

    void RemoveDebries()
    {
        Destroy(instantiatedObject);
        Destroy(this.gameObject);
    }
}

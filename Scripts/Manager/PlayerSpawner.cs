using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner instance;
    [SerializeField] GameObject playerPrefab,deathEffect;
    [SerializeField] float respawnTimer=5f;
    private GameObject playerObj;


    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        if(PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }
    }

  
    public void SpawnPlayer()
    {
        Transform spawnPoint = SpawnManager.instance.getRandomSpawnPoint();
        playerObj=  PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
        playerObj.GetComponent<PlayerMachanics>().ResetHealth();
        
    }

    public void Die(string damager)
    {
        UIController.instance.desthMessage.text="You Were Killed By "+damager;

        MatchManager.instance.UpdatePlayerSend(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);

        if (playerObj!=null) StartCoroutine(DieCo());
        
    }


    IEnumerator DieCo()
    {
        PhotonNetwork.Instantiate(deathEffect.name,playerObj.transform.position,Quaternion.identity);
        PhotonNetwork.Destroy(playerObj);
        playerObj = null;
        UIController.instance.deathScreen.SetActive(true);

        yield return new WaitForSeconds(respawnTimer);

        UIController.instance.deathScreen.SetActive(false);
        //Only spawn the player when the state is playeing //not match end
        if(MatchManager.instance.state==MatchManager.GameState.Playing && playerObj==null)
            SpawnPlayer();
    }
}

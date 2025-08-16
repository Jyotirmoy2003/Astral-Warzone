using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner instance;
    [SerializeField] GameObject playerPrefab,deathEffect;
    [SerializeField] float respawnTimer=5f;
    private GameObject playerObj;
    public Transform localPlayer;
    public PlayerDataHolder[] allPlayersinRoom;
    [SerializeField] GameEvent onlocalPlayerCached;
    private bool isinitDone=false;


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
    public void Init()
    {
        Array.Clear(allPlayersinRoom,0,allPlayersinRoom.Length);
        allPlayersinRoom=FindObjectsOfType<PlayerDataHolder>();
    }
  
    public void SpawnPlayer()
    {
        Transform spawnPoint = SpawnManager.instance.getRandomSpawnPoint();
        playerObj=  PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
        playerObj.GetComponent<PlayerMachanics>().ResetHealth();
        
        localPlayer=playerObj.transform;
        // if(!isinitDone)Invoke(nameof(Init),2f);
        // else Init();

    }


    

    public void Die(string damager,int actorgotKilled,int actorKilled)
    {
        UIController.instance.desthMessage.text="You Were Killed By "+damager;

        MatchManager.instance.UpdatePlayerSend(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1,actorgotKilled,actorKilled); //-1 NOT VALID

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

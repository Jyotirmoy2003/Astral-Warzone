using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager instance;
   [SerializeField] Transform[] spawnPoints;



   void Awake()
    {
        instance=this;
    }
   
    void Start()
    {
        foreach(Transform item in spawnPoints)
        {
            item.gameObject.SetActive(false);
        }
    }

    
    public Transform getRandomSpawnPoint()
    {
        int index=Random.Range(0,spawnPoints.Length);
        return spawnPoints[index];
    }
}

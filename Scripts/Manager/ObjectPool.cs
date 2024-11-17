using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool instance;

    [SerializeField] GameObject bulletImpactObject;
    [SerializeField] int amountOfBulletObjectInPool;
    private int index=0;

    private List<GameObject> bulletImpactPool = new List<GameObject>();

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        
    }






    void Start()
    {
        GeneratePool();
    }

   void GeneratePool()
   {
        for(int i=0;i<amountOfBulletObjectInPool;i++)
        {
            GameObject obj = Instantiate(bulletImpactObject,transform);
            bulletImpactPool.Add(obj);
            obj.SetActive(true);
        }
   }

    //get Object 
    public GameObject GetBulletImpact()
    {
        index=(index+1)%bulletImpactPool.Count;
        return bulletImpactPool[index];
        
    }


}

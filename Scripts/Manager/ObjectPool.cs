using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool instance;

    [SerializeField] GameObject bulletImpactObject;
    [SerializeField] int amountOfBulletObjectInPool;

    private List<GameObject> bulletImpactPool = new List<GameObject>();

    private void Awake()
    {
        instance = this;
        Debug.Log("Awake is Called");
    }

    private void OnEnable()
    {
        Debug.Log("OnEnabled Is Called");
    }






    void Start()
    {
        Debug.Log("Start is Called");
        GeneratePool();
    }

   void GeneratePool()
   {
        for(int i=0;i<amountOfBulletObjectInPool;i++)
        {
            GameObject obj = Instantiate(bulletImpactObject,transform);
            bulletImpactPool.Add(obj);
            obj.SetActive(false);
        }
   }

    //get Object 
    public GameObject GetBulletImpact()
    {
        foreach(GameObject item in bulletImpactPool)
        {
            if(!item.activeInHierarchy)
            {
                item.SetActive(true);
                return item;
            }
        }

        return null;
    }


}

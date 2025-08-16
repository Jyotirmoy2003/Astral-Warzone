using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Billboard : MonoBehaviour
{
    [SerializeField] Transform cameraTransfomr;
    [SerializeField] Transform myTransform;
    public bool IsActive=false;
    
    void Start()
    {
        if(!cameraTransfomr)
        {
            cameraTransfomr=GameObject.FindGameObjectWithTag("MainCamera").transform;
        }
        if(!myTransform)
        {
            myTransform=transform;
        }
    }

    
    void FixedUpdate()
    {
        if(IsActive)
        myTransform.LookAt(myTransform.position + cameraTransfomr.forward);
    }

    
}

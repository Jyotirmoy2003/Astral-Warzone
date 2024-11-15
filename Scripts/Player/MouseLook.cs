using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MouseLook : MonoBehaviourPunCallbacks
{
    [SerializeField] Transform viewPoint;
   [SerializeField] float mouseSensitivity=1f;

   private float verticalRotstore;
   private Vector2 mouseInput;
   private Transform myTransform;
   private Transform cam;
    private bool IsRicoil = false;
    private float recoilAmountX, recoilAmountY;
    private float randomX, randomY;
    void Start()
    {
        if (!photonView.IsMine) Destroy(this);
        myTransform =GetComponent<Transform>();
        Cursor.lockState=CursorLockMode.Locked;
        cam=Camera.main.transform;
        mouseSensitivity = PlayerPrefs.GetFloat("sensitivity", 1);
    }

    // Update is called once per frame
    void Update()
    {
       
        if(Cursor.lockState==CursorLockMode.None) return;

        mouseInput =new Vector2(Input.GetAxisRaw("Mouse X"),Input.GetAxisRaw("Mouse Y"))*mouseSensitivity;
        if(IsRicoil)
        {
            randomX = Random.Range(recoilAmountX*-1, recoilAmountX);
            randomY = Random.Range(0, recoilAmountY);

        }
        else
        {
            randomX = randomY = 0;
        }

        myTransform.rotation=Quaternion.Euler(myTransform.rotation.eulerAngles.x,myTransform.rotation.eulerAngles.y+mouseInput.x+randomX,myTransform.rotation.z);

        verticalRotstore += mouseInput.y + randomY;
        verticalRotstore=Mathf.Clamp(verticalRotstore,-60f,60f);
        viewPoint.localRotation=Quaternion.Euler(verticalRotstore*-1,viewPoint.rotation.y,viewPoint.rotation.z);
    }
    void LateUpdate()
    {
       

         cam.position=viewPoint.position;
        cam.rotation=viewPoint.rotation;
        
    }

    public void Recoil(bool val,float amountX,float amountY)
    {
        IsRicoil = val;
        recoilAmountX = amountX;
        recoilAmountY = amountY;
    }
}

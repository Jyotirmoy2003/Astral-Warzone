using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;



public class NightVision : BatterySystem
{
    [Header("Reference Night Vision")]
    [SerializeField] GameObject nightVisionOverlay;
    public bool haveNightVision = false;





    void NightVisionUpdate()
    {
        // if(Input.GetAxis("Mouse ScrollWheel")>0)
        // {
        //     if(cam.fieldOfView>10)
        //     {
        //         cam.fieldOfView-=5;
        //         zoomBar.fillAmount=cam.fieldOfView/100f;
        //     }
        // }
        // if(Input.GetAxis("Mouse ScrollWheel")<0)
        // {
        //     if(cam.fieldOfView<60)
        //     {
        //         cam.fieldOfView+=5;
        //         zoomBar.fillAmount=cam.fieldOfView/100f;
        //     }
        // }
        UpdateBattery();
    }

    public void AcitvateNightVision(bool IsActive)
    {
        if (IsActive && isBatteryUsable)
        {
            // cam.fieldOfView=60;
            // zoomBar.fillAmount=0.6f;
            UpdateDel = NightVisionUpdate;
            nightVisionOverlay.SetActive(true);
        }
        else
        {
            // cam.fieldOfView=60;
            // zoomBar.fillAmount=0.6f;
            UpdateDel = null;
            nightVisionOverlay.SetActive(false);
        }
    }

    #region  EVENT
    public void ListenToBatteryConsumed(Component sender, object data)
    {

        if ((bool)data && !isBatteryUsable)
        {
            AcitvateNightVision(false);

        }
    }



    
    #endregion


}


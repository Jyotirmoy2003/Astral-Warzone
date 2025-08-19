using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BatterySystem : MonoBehaviour
{
    [Header("Reference Battery")]
    [SerializeField] protected Image batterChunk;
    [SerializeField] protected float batterConsumeRate = 0.1f;
    [SerializeField] protected float totalBatteryPower = 1f;
    [SerializeField] protected GameEvent BatteryConsumedEvent;
    protected bool isBatteryUsable = true;
    protected Action UpdateDel;



    // Update is called once per frame
    void Update() => UpdateDel?.Invoke();

    protected void UpdateBattery()
    {
        if (totalBatteryPower > 0)
        {
            totalBatteryPower -= batterConsumeRate * Time.deltaTime;

        }
        else if (isBatteryUsable)
        {
            isBatteryUsable = false;
            BatteryConsumedEvent.Raise(this, true);
        }
        batterChunk.fillAmount = totalBatteryPower;
    }


    public bool IsBatteryLeft()
    {
        return isBatteryUsable;
    }

    public void RechargeBattery()
    {
        totalBatteryPower = 1f;
        isBatteryUsable = true;
    }
    
   
}

// using System.Collections;
// using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Jy_Util;

public class DamageIndicator : MonoBehaviour
{
 

    public Vector3 DamageLocation ;
    public Transform DamagingActor;
    //public Transform PlayerSpawner.instance.localPlayer;
    public Transform DamageImagePivot;

    public CanvasGroup DamageImageCanvas;
    public float fadeTime=0.3f,canvasDuration=2f;
    private NoArgumentFun updateFunDel=new util().NullFun;
    public float offset=20f;
    private int damagingActorIndex=-1;
   

    // Update is called once per frame
    void Update()=>updateFunDel();

    void UpdateWhenActive()
    {
        if(!PlayerSpawner.instance.localPlayer) return;
        DamageLocation=PlayerSpawner.instance.allPlayersinRoom[damagingActorIndex].playerTransform.position;
        DamageLocation.y = PlayerSpawner.instance.localPlayer.position.y;
        Debug.Log("Location: "+DamageLocation+ "local Player: "+PlayerSpawner.instance.localPlayer+"Damaging Actor:  "+PlayerSpawner.instance.allPlayersinRoom[damagingActorIndex].playerTransform);
        Vector3 Direction =(DamageLocation - PlayerSpawner.instance.localPlayer . position) . normalized;
        float angle=(Vector3.SignedAngle(Direction,PlayerSpawner.instance.localPlayer.forward,Vector3.up));
        angle+=offset;
        DamageImagePivot.transform.localEulerAngles=new Vector3(0,0,angle);
    }

    public void Init(int index)
    {
        
        this.gameObject.SetActive(true);
        damagingActorIndex=index;
        updateFunDel=UpdateWhenActive;
        //localPlayer=PlayerSpawner.instance.PlayerSpawner.instance.localPlayer;
        
        DamageImageCanvas.DOFade(fadeTime,1);
        Invoke(nameof(End),canvasDuration);
        Invoke(nameof(Deactivate),canvasDuration+1f);
    }

    void End()
    {
        DamageImageCanvas.DOFade(fadeTime,0);
        updateFunDel=new util().NullFun;
    }

    void Deactivate()
    {
        
        this.gameObject.SetActive(false);
    }

    public void ListentolocalPlayerCached(Component sender,object data)
    {
        if((bool)data)
        {
           
            this.gameObject.SetActive(false);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageDebug : MonoBehaviour
{
    public Vector3 DamageLocation ;
    public Transform damagingAcotr;
    public Transform localPlayer;
    public Transform DamageImagePivot;

    public CanvasGroup DamageImageCanvas;
    public float fadeTime=0.3f,canvasDuration=2f;
   
    public float offset=20f;
   
    
    // Update is called once per frame
    void Update()=>UpdateWhenActive();

    void UpdateWhenActive()
    {
        DamageLocation=damagingAcotr.position;
        if(!localPlayer) return;
        DamageLocation.y = localPlayer.position.y;
        Vector3 Direction =(DamageLocation - localPlayer . position) . normalized;
        float angle=(Vector3.SignedAngle(Direction,localPlayer.forward,Vector3.up));
        angle+=offset;
        DamageImagePivot.transform.localEulerAngles=new Vector3(0,0,angle);
    }

    public void Init(Vector3 location)
    {
        
      
    }

    void End()
    {
       
    }

    void Deactivate()
    {
        
        this.gameObject.SetActive(false);
    }
}

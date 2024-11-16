using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Jy_Util;

public class DamagePopup : MonoBehaviour
{
    private static int sortingOrder;

    private const float DISAPPEAR_TIMER_MAX = 0.6f;
    private TextMeshPro textmesh;
    private NoArgumentFun updateDel=new util().NullFun;
    private float disappearTimer;
    private Color textColor;
    private Vector3 moveVector;
 



    public static DamagePopup Create (Vector3 position, int damageAmount,bool isCritical) 
    {
        Transform damagePopupTransform = Instantiate(GameAssets._i.pfDamagePopup, position, Quaternion . identity) ;

        DamagePopup damagePopup  = damagePopupTransform.GetComponent<DamagePopup>();
        damagePopup. Setup (damageAmount,isCritical) ;
        return damagePopup ;
    }

    private void Awake() {
        textmesh=transform. GetComponent<TextMeshPro>();
    }

    public void Setup(int damageAmount,bool isCritical) 
    {

        textmesh.SetText(damageAmount.ToString());
        disappearTimer = DISAPPEAR_TIMER_MAX;
        updateDel=ActivateUpdate;
        if(isCritical)
        {
            textColor=new Color(1,0.07f,0.8f);
            textmesh.fontSize=45f;
        }else{
            textColor=new Color(1,0.5f,0.08f);
            textmesh.fontSize=36f;
        }

        textmesh.color = textColor;

        sortingOrder++;
        textmesh.sortingOrder = sortingOrder;

        moveVector = new Vector3(.3f, 2f) * 2f;
        
        updateDel=ActivateUpdate;

    }
    void SetForward()
    {
        //transform.localPosition+=damagePopupOffset;
        updateDel=ActivateUpdate;

    }

    void Update ()=>updateDel();
   

    void ActivateUpdate()
    {
        transform.position += moveVector * Time.deltaTime;
        moveVector -= moveVector * 8f * Time.deltaTime;

        if (disappearTimer > DISAPPEAR_TIMER_MAX * .5f) {
            // First half of the popup lifetime
            float increaseScaleAmount = 0.4f;
            transform.localScale += Vector3.one * increaseScaleAmount * Time.deltaTime;
        } else {
            // Second half of the popup lifetime
            float decreaseScaleAmount = 0.4f;
            transform.localScale -= Vector3.one * decreaseScaleAmount * Time.deltaTime;
        }

        disappearTimer -= Time.deltaTime;
        if (disappearTimer <= 0) {
            // Start disappearing
            float disappearSpeed = 3f;
            textColor.a -= disappearSpeed * Time.deltaTime;
            textmesh.color = textColor;
            if (textColor.a <= 0) {
                // updateDel=new util().NullFun;
                // this.gameObject.SetActive(false);
                Destroy(this.gameObject);
            }
        }
   
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KillLog : MonoBehaviour
{
   [SerializeField] TMP_Text logText;

   
   public void Init(string logText,float duration)
   {
        this.logText.text=logText;
        this.gameObject.SetActive(true);
        Invoke(nameof(DisableObject),duration);
   }

   void DisableObject()
   {
        this.gameObject.SetActive(false);

   }
}

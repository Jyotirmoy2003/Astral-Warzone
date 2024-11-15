using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAssets : MonoBehaviour
{
    public static GameAssets _i;
    void Awake()
    {
        _i=this;
    }

    public Transform pfDamagePopup;
}

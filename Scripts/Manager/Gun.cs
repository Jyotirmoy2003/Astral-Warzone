using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public bool IsAutomatic;
    public float rateOfFire=0.1f,heatPershot=1,range=5f,damage=10f;
    public SliderBar bar;
    public GameObject muzzleFlash;
    public float adsZoom = 30f;
    [Range(0f,1f)]
    public float recoilX=0.2f;
    [Range(0f, 3f)]
    public float recoilY = 0.3f;
    public AudioSource audioSource;
}

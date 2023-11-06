using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class PlayerMachanics : MonoBehaviourPunCallbacks
{
    
   
    [SerializeField] float distance=5.0f;
    [SerializeField] GameObject bulletImpact,playerHitImpact;
    [SerializeField] SliderBar slider;
    [SerializeField] GameEvent healthChangeEvent;
    [SerializeField] float maxHeat=10f,coolRate=4f,overheatCoolRate=5f;
    [SerializeField] float muzzleFlashDuration=0.05f;
    [SerializeField] Animator gunAnimator;
    [SerializeField] Gun[] allGuns;
    [SerializeField] float health, adsZoomSpeed = 5f;
    [SerializeField] GameObject playerObj;
    [SerializeField] Transform modelGunPoint, gunHolder, adsOut, adsIn;
    [SerializeField] Material[] allSkins;
    [SerializeField] AudioSource gotHitAudio;

    private float curretHealth;
    private float muzzleCounter=0;
    private Ray ray;
    private RaycastHit hit;
    private int selectedGun=0;
    private float shotCounter=0;
    private float heatCounter=0;
    private bool IsOverHeated=false;
    private PhotonView pv;
    private Camera cam;
    private PlayerController pc;
    private MouseLook mouseLook;





    void Start()
    {
        pv=GetComponent<PhotonView>();
        cam = Camera.main;
        pv.RPC("SetGun",RpcTarget.All,selectedGun);
        
        if(photonView.IsMine) 
        {
            mouseLook = GetComponent<MouseLook>();
            pc = gameObject.GetComponent<PlayerController>();
            playerObj.SetActive(false);
            slider.SetMax(maxHeat);
            slider.SetValue(heatCounter);
            ResetHealth();
        }else{
            gunHolder.parent=modelGunPoint;
            gunHolder.localPosition=Vector3.zero;
            gunHolder.localRotation=Quaternion.identity;
        }

        playerObj.GetComponent<Renderer>().material = allSkins[pv.Owner.ActorNumber % allSkins.Length];
    }

   
    void Update()
    {
        if (!photonView.IsMine) return;
        ShootInput();
       ChangeGun();
    }

    void ChangeGun()
    {
        if(Input.GetAxisRaw("Mouse ScrollWheel")>0f)
        {
            selectedGun=(selectedGun+1)%allGuns.Length;
            pv.RPC("SetGun",RpcTarget.All,selectedGun);
        }
        else if(Input.GetAxisRaw("Mouse ScrollWheel")<0f)
        {
            selectedGun--;
            if(selectedGun<0) selectedGun=allGuns.Length-1;
            pv.RPC("SetGun",RpcTarget.All,selectedGun);
        }
    }

    void SwitchGun()
    {
        foreach(Gun item in allGuns)
        {
            item.gameObject.SetActive(false);
        }
        gunAnimator.SetTrigger("Switch");
        allGuns[selectedGun].gameObject.SetActive(true);
        allGuns[selectedGun].muzzleFlash.SetActive(false);
        slider=allGuns[selectedGun].bar;
        heatCounter = 0;
    }

    void ShootInput()
    {
         if(!IsOverHeated)
        {
            if(Input.GetMouseButtonDown(0) && shotCounter<=0) Shoot();
            if(Input.GetMouseButton(0) && allGuns[selectedGun].IsAutomatic && shotCounter<=0)
            {
                    Shoot();
            }
            else
            {
                mouseLook.Recoil(false, 0, 0);
            }
            heatCounter-=coolRate*Time.deltaTime;
        }else{ //cool down weapon
            heatCounter-=overheatCoolRate*Time.deltaTime;
            mouseLook.Recoil(false, 0, 0);
        }
        if(heatCounter<=0)
        {
            heatCounter=0;
            IsOverHeated=false;
            UIController.instance.overHeatedText.gameObject.SetActive(false);
            
        }
        slider.SetValue(heatCounter); //set heat slider

        //set Muzzle Falsh off 
        if(allGuns[selectedGun].muzzleFlash.activeInHierarchy)
        {
            muzzleCounter-=Time.deltaTime;

            if(muzzleCounter<=0)
            {
                allGuns[selectedGun].muzzleFlash.SetActive(false);
            }
        }
        //Scope in
        if(Input.GetMouseButton(1))
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, allGuns[selectedGun].adsZoom,adsZoomSpeed*Time.deltaTime);
            gunHolder.localPosition = Vector3.Lerp(gunHolder.localPosition, adsIn.localPosition, adsZoomSpeed * Time.deltaTime);
        }
        else
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 60f, adsZoomSpeed * Time.deltaTime);
            gunHolder.localPosition = Vector3.Lerp(gunHolder.localPosition, adsOut.localPosition, adsZoomSpeed * Time.deltaTime);
        }

       if(shotCounter>0)
        {
            shotCounter -= Time.deltaTime;
        }

        for (int i=0;i<allGuns.Length;i++)
        {
            if(Input.GetKeyDown((i+1).ToString()))
            {
                selectedGun=i;
                pv.RPC("SetGun",RpcTarget.All,selectedGun);
            }
        }

    }

    void Shoot()
    {
        //Add Recoil
        mouseLook.Recoil(true, allGuns[selectedGun].recoilX, allGuns[selectedGun].recoilY);
        //Muzzle Flash
        allGuns[selectedGun].muzzleFlash.SetActive(true);
        muzzleCounter=muzzleFlashDuration;
        //if already sound is playing then stop previous
        allGuns[selectedGun].audioSource.Stop();
        allGuns[selectedGun].audioSource.Play();
        //ray cast and check hit
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, allGuns[selectedGun].range))
        {

            if (hit.collider.CompareTag("Player"))
            {
                hit.collider.gameObject.GetComponent<PhotonView>().RPC("DealDamage", RpcTarget.All, allGuns[selectedGun].damage, photonView.Owner.NickName, PhotonNetwork.LocalPlayer.ActorNumber);
                PhotonNetwork.Instantiate(playerHitImpact.name, hit.point, Quaternion.identity);
            }
            else
            {
                Transform tempBulletImpact = ObjectPool.instance.GetBulletImpact().transform;
                tempBulletImpact.position = hit.point + (hit.normal * 0.002f);
                tempBulletImpact.rotation = Quaternion.LookRotation(hit.normal, Vector3.up);
                StartCoroutine(DeactivateObject(tempBulletImpact.gameObject));
                //Destroy(Instantiate(bulletImpact, hit.point + (hit.normal * 0.002f), Quaternion.LookRotation(hit.normal, Vector3.up)), 3f);
            }
        }

        shotCounter=allGuns[selectedGun].rateOfFire;
        //add Heat fun
        heatCounter+=allGuns[selectedGun].heatPershot;
        
        if(heatCounter>=maxHeat)
        {
            heatCounter=maxHeat;
            IsOverHeated=true;
            gunAnimator.SetTrigger("OverHeat");
            UIController.instance.overHeatedText.gameObject.SetActive(true);
        }
    }

    IEnumerator DeactivateObject(GameObject obj)
    {
        yield return new WaitForSeconds(3f);
        obj.SetActive(false);
    }

    [PunRPC]
    public void DealDamage(float amount,string damager,int actor)
    {
        TakeDamage(amount,damager,actor);
    }

    void TakeDamage(float amount,string damager,int actor)
    {
        if(photonView.IsMine)
        {   
            curretHealth-=amount;
           healthChangeEvent.Raise(this,curretHealth);
            if (curretHealth <= 0)
            {
                PlayerSpawner.instance.Die(damager);
                MatchManager.instance.UpdatePlayerSend(actor, 0, 1);
            }
            if (gotHitAudio.isPlaying) return;
            gotHitAudio.Play();
        }
    }

    public void Heal(float amount)
    {
        curretHealth += amount;
        if (curretHealth >= health) curretHealth = health;
       healthChangeEvent.Raise(this,curretHealth);

    }

    public void ResetHealth()
    {
        curretHealth=health;
       healthChangeEvent.Raise(this,curretHealth);
    }

  
    [PunRPC]
    public void SetGun(int gunIndex)
    {
        if(gunIndex<allGuns.Length)
        {
            selectedGun=gunIndex;
            SwitchGun();
        }
    }
}

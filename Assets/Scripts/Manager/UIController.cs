using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using TMPro.Examples;


public class UIController : MonoBehaviour
{
    public static UIController instance;
    public GameObject overHeatedText;
    public GameObject deathScreen;
    public TMP_Text desthMessage;
    public SliderBar healthBar;
    public TMP_Text killsText, deathsText;
    
    [Space]
    [Header("Leader Board")]
    public GameObject leaderboard;
    public LeaderboardPlayer leaderboardPlayerDisplay;


    public TMP_Text timerText;
    public KillLog killLogprefab;
    public Transform killLogParent;

    [Space]
    [Header("Settings")]
    public GameObject optionsScreen;
    [SerializeField] KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] GameEvent OnPressedSettings;

    [Space]
    [Header("Damage Indicator ")]
    [SerializeField] List<DamageIndicator> damageIndicators= new List<DamageIndicator>();
    [SerializeField] DamageIndicator damageIndicatorPrefab;
    [SerializeField] Transform damageIndicatorContainer;

    [Space]
    [Header("Match End screen")]
    public GameObject endScreen;
    public TMP_Text nextMatchStartCountdown_text;
    


    void Awake()
    {
        instance=this;
       
    }
    void Start()
    {
        GeneratePool();
    }
    void GeneratePool()
   {
        for(int i=0;i<10;i++)
        {
            DamageIndicator obj = Instantiate(damageIndicatorPrefab,damageIndicatorContainer);
//            obj.localPlayer=PlayerSpawner.instance.GetlocalPlayer().transform;
            damageIndicators.Add(obj);
            obj.gameObject.SetActive(false);
        }
   }

    void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            ShowHideOptions();
        }

        if (optionsScreen.activeInHierarchy && Cursor.lockState != CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void ShowHideOptions()
    {
        if (!optionsScreen.activeInHierarchy)
        {
            optionsScreen.SetActive(true);
        }
        else
        {
            optionsScreen.SetActive(false);
            OnPressedSettings.Raise(this, false);
        }
    }

    public void ShowkillLog(string text,float duration)
    {
        Instantiate(killLogprefab,killLogParent).GetComponent<KillLog>().Init(text, duration);
    }

    public void ShowDamageIndicator(Vector3 location)
    {
        foreach(DamageIndicator item in damageIndicators)
        {
            if(!item.gameObject.activeSelf)
            {
                //item.Init(location);
                break;
            }
        }
    }
     public void ShowDamageIndicator(int index)
    {
        foreach(DamageIndicator item in damageIndicators)
        {
            if(!item.gameObject.activeSelf)
            {
                item.Init(index);
                break;
            }
        }
    }

    public void ReturnToMainMenu()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    
}

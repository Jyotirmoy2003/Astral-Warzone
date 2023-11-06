using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;


public class UIController : MonoBehaviour
{
    public static UIController instance;
    public TMP_Text overHeatedText;
    public GameObject deathScreen;
    public TMP_Text desthMessage;
    public SliderBar healthBar;
    public TMP_Text killsText, deathsText;

    public GameObject leaderboard;
    public LeaderboardPlayer leaderboardPlayerDisplay;

    public GameObject endScreen;

    public TMP_Text timerText;

    public GameObject optionsScreen;
    [SerializeField] KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] GameEvent OnPressedSettings;

    


    void Awake()
    {
        instance=this;
       
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

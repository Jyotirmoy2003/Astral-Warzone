using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using UnityEngine.UI;
public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher instance;
   
    [SerializeField] GameObject loadingScreen;
    [SerializeField] TMP_Text loadingText;

    [SerializeField] GameObject menuButtons;

    [SerializeField] GameObject createRoomPanel;
    [SerializeField] TMP_InputField createRoomInput;

     [SerializeField] GameObject roomPanel;
    [SerializeField] TMP_Text roomNametext, playerNameLable;
    private List<TMP_Text> allNames = new List<TMP_Text>();

     [SerializeField] GameObject errorPanel;
     [SerializeField] TMP_Text errorText;

    [SerializeField] GameObject roomBrowserPanel;
    [SerializeField] RoomButton theRoomButton; //room prefab
    [SerializeField] List<RoomButton> allRooms = new List<RoomButton>();

    [SerializeField] GameObject namePanel;
    [SerializeField] TMP_InputField nameInput;
    private static bool IsNameSet = false;

    [SerializeField] int levelToPlay=0;
    public List<string> allMaps = new List<string>();
    public bool changeMapBetweenRounds = true;
    [SerializeField] GameObject startButton;

    [SerializeField] GameObject roomTestButton;
    [SerializeField] Sprite[] allMapPreview;
    [SerializeField] Image roomPreviewImg;
    //Arrow buttons to change maps
    [SerializeField] GameObject[] propsArrow;
    [SerializeField] int killToWin = 10, matchLength = 3;
    [SerializeField] TMP_Text killToWinText, matchLengthtext;
    ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();
    ExitGames.Client.Photon.Hashtable roomProps = new ExitGames.Client.Photon.Hashtable();





    void Awake()
     {
          instance=this;
     }
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        roomPreviewImg.sprite = allMapPreview[levelToPlay];
        
        CloseMenu();

        loadingScreen.SetActive(true);
        loadingText.text="Connecting to Network..";
        //if not connected then connect (only use when Comming back from a already playing game)
        if(!PhotonNetwork.IsConnected)
        PhotonNetwork.ConnectUsingSettings();

#if UNITY_EDITOR
        roomTestButton.SetActive(true);
#endif

    }
    #region Utility FUN

    void CloseMenu()
   {
      loadingScreen.SetActive(false);
      menuButtons.SetActive(false);
      createRoomPanel.SetActive(false);
      roomPanel.SetActive(false);
      errorPanel.SetActive(false);
       roomBrowserPanel.SetActive(false);
        namePanel.SetActive(false);
   }

   public void OpenRoomCreatePanel()
   {
     CloseMenu();
     createRoomPanel.SetActive(true);
   }

   public void CreateRoom()
   {
     if(string.IsNullOrEmpty(createRoomInput.text)) return;

     RoomOptions options=new RoomOptions();
     options.MaxPlayers=8;
        options.BroadcastPropsChangeToAll = true;
     
     PhotonNetwork.CreateRoom(createRoomInput.text,options);
        

        CloseMenu();
     loadingText.text="Creating Room..";
     loadingScreen.SetActive(true);
        
    }

   public void CloseErrorMenu()
   {
      CloseMenu();
      menuButtons.SetActive(true);
   }

   public void LeaveRoom()
   {
      PhotonNetwork.LeaveRoom();
      loadingText.text="Leaveing Room..";
      CloseMenu();
      loadingScreen.SetActive(true);
   }
    
    public void OpenRoomBrowser()
    {
        CloseErrorMenu();
        roomBrowserPanel.SetActive(true);
    }
    public void CloseRoomBrowser()
    {
        CloseErrorMenu();
        menuButtons.SetActive(true);
    }

    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
        loadingText.text = "Joining Room..";
        loadingScreen.SetActive(true);
    }


    public void QuitGame() => Application.Quit();

    public void StartGame()
    {
        PhotonNetwork.LoadLevel(allMaps[levelToPlay]);
    }
    public void QuictTestRoom()
    {
        PhotonNetwork.CreateRoom("Test");
        CloseMenu();
        loadingText.text = "Creating Room..";
        loadingScreen.SetActive(true);
    }

    private void ListAllPlayers()
    {
        foreach (TMP_Text item in allNames) Destroy(item.gameObject);
        allNames.Clear();

        Player[] players = PhotonNetwork.PlayerList;
        playerNameLable.gameObject.SetActive(false);
        for(int i=0;i<players.Length;i++)
        {
            TMP_Text newText = Instantiate(playerNameLable, playerNameLable.transform.parent);
            if (players[i].IsMasterClient)
                newText.text = players[i].NickName + "\n(Host)";
            else
                newText.text = players[i].NickName;
            newText.gameObject.SetActive(true);

            allNames.Add(newText);
        }
    }

    public void SetNickName()
    {
        if(!string.IsNullOrEmpty(nameInput.text))
        {
            PhotonNetwork.NickName = nameInput.text;
            PlayerPrefs.SetString("playerName",nameInput.text);
            IsNameSet = true;
            CloseMenu();
            menuButtons.SetActive(true);
        }
    }
    #region Arrows
    public void LeftArrow()
    {
        if(levelToPlay<=0)
        {
            levelToPlay= allMaps.Capacity - 1;
        }
        else
        {
            levelToPlay--;
        }
        roomPreviewImg.sprite = allMapPreview[levelToPlay];
        //Update properties so the others can see 
        SetRoomPorties();
    }
    public void RightArrow()
    {
        levelToPlay = (levelToPlay + 1) % allMaps.Capacity;
        roomPreviewImg.sprite = allMapPreview[levelToPlay];
        SetRoomPorties();
    }
    
    public void UpNumberOfKill()
    {
        if (killToWin >= 30) return;
        killToWin++;
        SetRoomPorties();
    }
    public void DownNumberOfKill()
    {
        if (killToWin <= 1) return;
        killToWin--;
        SetRoomPorties();
    }

    public void UpTimer()
    {
        if (matchLength >= 10) return;
        matchLength++;
        SetRoomPorties();
    }
    public void DownTimer()
    {
        if (matchLength <= 1) return;
        matchLength--;
        SetRoomPorties();
    }
    
    #endregion

    public void SetRoomPorties()
    {
        killToWinText.text = killToWin.ToString();
        matchLengthtext.text = matchLength.ToString();
        roomProps["levelToPlay"] = levelToPlay;
        roomProps["killToWin"] = killToWin;
        roomProps["matchLength"] = matchLength;
        //transmit the data to all other players

        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
    }

    public void UpdatePlayerProps(Player _targetPlayer)
    {
        
        if(_targetPlayer.CustomProperties.ContainsKey("levelToPlay"))
        {
            roomPreviewImg.sprite = allMapPreview[(int)_targetPlayer.CustomProperties["levelToPlay"]];
        }
    }

    public void UpdateRoomProps()
    {
        
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("levelToPlay"))
        {
            roomPreviewImg.sprite = allMapPreview[(int)PhotonNetwork.CurrentRoom.CustomProperties["levelToPlay"]];
        }
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("killToWin"))
            killToWinText.text = ((int)PhotonNetwork.CurrentRoom.CustomProperties["killToWin"]).ToString();

        if(PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("matchLength"))
            matchLengthtext.text= ((int)PhotonNetwork.CurrentRoom.CustomProperties["matchLength"]).ToString();
    }

#endregion
    #region  PUN
    public override void OnConnectedToMaster()
  {
    PhotonNetwork.JoinLobby();
    PhotonNetwork.AutomaticallySyncScene=true;
    loadingText.text="Joining Lobby..";
  }

  public override void OnJoinedLobby()
  {
    CloseMenu();
    if(!IsNameSet)
    {
            namePanel.SetActive(true);
            if(PlayerPrefs.HasKey("playerName"))
            {
                nameInput.text = PlayerPrefs.GetString("playerName");
            }
    }else
    {
            PhotonNetwork.NickName = PlayerPrefs.GetString("playerName");
            menuButtons.SetActive(true);
        }
   }

    public override void OnJoinedRoom()
    {
        CloseMenu();
        roomPanel.SetActive(true);
        roomNametext.text = PhotonNetwork.CurrentRoom.Name;
        SetRoomPorties(); //after creating the room just set room prop


        ListAllPlayers(); //show Player Names In The Room
        UpdateRoomProps(); //show room properties;

        if (!PhotonNetwork.IsMasterClient)
        {
            startButton.SetActive(false);
            foreach(GameObject item in propsArrow)
            {
                item.SetActive(false);
            }
            
        }//if not master clint then hide the start button
        else { 
            startButton.SetActive(true);
            foreach (GameObject item in propsArrow)
            {
                item.SetActive(true);
            }
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (!PhotonNetwork.IsMasterClient) startButton.SetActive(false); //if not master clint then hide the start button
        else startButton.SetActive(true);
    }

    public override void OnCreateRoomFailed(short returnCode,string messg)
  {
    errorText.text="Room Creation Failed : "+messg;
    CloseMenu();
    errorPanel.SetActive(true);
  }

  public override void OnLeftRoom()
  {
    CloseMenu();
    menuButtons.SetActive(true);
  }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomButton item in allRooms) Destroy(item.gameObject);
        allRooms.Clear();

        theRoomButton.gameObject.SetActive(false);
        for(int i=0;i<roomList.Count;i++)
        {
            if (roomList[i].PlayerCount >= roomList[i].MaxPlayers || roomList[i].RemovedFromList) continue;

            RoomButton newButton = Instantiate(theRoomButton, theRoomButton.transform.parent);
            newButton.SetRoomInfo(roomList[i]);
            newButton.gameObject.SetActive(true);

            allRooms.Add(newButton);
        }
    }


    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        TMP_Text newText = Instantiate(playerNameLable, playerNameLable.transform.parent);
            newText.text = newPlayer.NickName;
        newText.gameObject.SetActive(true);

        allNames.Add(newText);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {

        ListAllPlayers();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
       
        UpdatePlayerProps(targetPlayer);
    }
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        UpdateRoomProps();

    }
    #endregion
}

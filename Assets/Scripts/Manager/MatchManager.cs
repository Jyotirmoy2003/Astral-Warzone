using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class MatchManager : MonoBehaviourPunCallbacks,IOnEventCallback
{
    public static MatchManager instance;
    public enum EventCodes: byte
    {
        NewPlayer,
        ListPlayer,
        UpdateStats,
        NextMatch,
        TimerSync
    }
    public enum GameState
    {
        Waiting,
        Playing,
        Ending
    }

    private void Awake()
    {
        instance = this;
    }

    public List<PlayerInfo> allPlayers = new List<PlayerInfo>();

    private List<LeaderboardPlayer> lboardPlayers = new List<LeaderboardPlayer>();
    private int Index;

    [SerializeField] int killsToWin = 3;
    [SerializeField] Transform mapCamPoint;
    public GameState state = GameState.Waiting;
    [SerializeField] int waitAfterEnding = 5;

    public bool perpetual;
    public float matchLength = 180f;
    private float currentMatchTime;
    private float sendTimer;
    private bool isLoadingMap=false;
    void Start()
    {
        if(!PhotonNetwork.IsConnected)
        {
            SceneManager.LoadScene(0);
        }
        else
        {
            NewPlayerSend(PhotonNetwork.NickName);
            state = GameState.Playing;
            if(PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("killToWin"))
                killsToWin =(int) PhotonNetwork.CurrentRoom.CustomProperties["killToWin"];
            if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("matchLength"))
                matchLength = ((int)PhotonNetwork.CurrentRoom.CustomProperties["matchLength"])* 60f;
            SetupTimer();
        }
        isLoadingMap=false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) && state != GameState.Ending)
        {
            if (UIController.instance.leaderboard.activeInHierarchy)
            {
                UIController.instance.leaderboard.SetActive(false);
            }
            else
            {
                ShowLeaderboard();
            }
        }

        if (PhotonNetwork.IsMasterClient)
        {
            if (currentMatchTime > 0f && state == GameState.Playing)
            {
                currentMatchTime -= Time.deltaTime;

                if (currentMatchTime <= 0f)
                {
                    currentMatchTime = 0f;

                    state = GameState.Ending;

                    ListPlayerSend();

                    StateCheck();
                }

                UpdateTimerDisplay();

                sendTimer -= Time.deltaTime;
                if (sendTimer <= 0)
                {
                    sendTimer += 1f;

                    TimerSend();
                }
                
            }
        }
    }
    
    #region Events
    public void OnEvent(EventData photonEvent)
  {
        if(photonEvent.Code<200)
        {
            EventCodes theEvent = (EventCodes)photonEvent.Code;
            object[] data = (object[])photonEvent.CustomData;

   
            switch (theEvent)
            {
                case EventCodes.NewPlayer:
                    NewPlayerRecived(data);
                    break;
                case EventCodes.ListPlayer:
                    ListPlayerRecived(data);
                    break;
                case EventCodes.UpdateStats:
                    UpdatePlayerRecived(data);
                    break;
                case EventCodes.NextMatch:
                    NextMatchReceive();
                    break;
                case EventCodes.TimerSync:
                    TimerReceive(data);
                    break;
            }
        }

        
       
  }

    public override void OnEnable()
    {
        //Register Event
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        //Unregister Event
        PhotonNetwork.RemoveCallbackTarget(this);
    }



    public void NewPlayerSend(string username)
    {
        object[] package = new object[4];
        //creating new Package as PlayerInfo s Requirement
        package[0] = username;
        package[1] = PhotonNetwork.LocalPlayer.ActorNumber;
        package[2] = 0;
        package[3] = 0;
        


        //raise the event
        PhotonNetwork.RaiseEvent((byte)EventCodes.NewPlayer, package,
            new RaiseEventOptions { Receivers = ReceiverGroup.MasterClient },
            new SendOptions { Reliability = true });
    }
    public void NewPlayerRecived(object[] dataReceived)
    {
        PlayerInfo player = new PlayerInfo((string)dataReceived[0],(int)dataReceived[1],(int)dataReceived[2],(int)dataReceived[3]);
        allPlayers.Add(player);

        ListPlayerSend();
    }



    public void ListPlayerSend()
    {
        object[] package = new object[allPlayers.Count + 1];

        package[0] = state;

        for (int i = 0; i < allPlayers.Count; i++)
        {
            object[] piece = new object[4];

            piece[0] = allPlayers[i].name;
            piece[1] = allPlayers[i].actor;
            piece[2] = allPlayers[i].kills;
            piece[3] = allPlayers[i].deaths;

            package[i + 1] = piece;
        }

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.ListPlayer,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }
    public void ListPlayerRecived(object[] dataReceived)
    {
        allPlayers.Clear();

        state = (GameState)dataReceived[0];

        for (int i = 1; i < dataReceived.Length; i++)
        {
            object[] piece = (object[])dataReceived[i];

            PlayerInfo player = new PlayerInfo(
                (string)piece[0],
                (int)piece[1],
                (int)piece[2],
                (int)piece[3]
                );

            allPlayers.Add(player);

            if (PhotonNetwork.LocalPlayer.ActorNumber == player.actor)
            {
                Index = i - 1;
            }
        }

        StateCheck();
    }

    // public void UpdatePlayerSend(int actorSending, int statToUpdate, int amountToChange)
    // {
    //     object[] package = new object[] { actorSending, statToUpdate, amountToChange };

    //     PhotonNetwork.RaiseEvent(
    //         (byte)EventCodes.UpdateStats,
    //         package,
    //         new RaiseEventOptions { Receivers = ReceiverGroup.All },
    //         new SendOptions { Reliability = true }
    //         );
    // }
    public void UpdatePlayerSend(int actorSending, int statToUpdate, int amountToChange,int actorGotKilled,int actorKilled)
    {
       
        object[] package = new object[] { actorSending, statToUpdate, amountToChange,actorGotKilled,actorGotKilled };
        

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.UpdateStats,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }
    public void UpdatePlayerRecived(object[] dataReceived)
    {
        int actor = (int)dataReceived[0];
        int statType = (int)dataReceived[1];
        int amount = (int)dataReceived[2];
        int actorGotKilled = (int)dataReceived[3];
        int actorKilled = (int)dataReceived[4];

        for (int i = 0; i < allPlayers.Count; i++)
        {
            if (allPlayers[i].actor == actor)
            {
                
                switch (statType)
                {
                    case 0: //kills
                        allPlayers[i].kills += amount;           
                        break;

                    case 1: //deaths
                        allPlayers[i].deaths += amount;
                        //if(actorKilled==-1) return;
                        Debug.Log("Killer Actor: "+actorKilled + "Actor got Killed"+ actorGotKilled +"And "+actor);
                        for(int j=0;j<allPlayers.Count;j++)
                        {
                            if(allPlayers[j].actor == actorKilled)
                            {
                                UIController.instance.ShowkillLog(allPlayers[i].name+" was killed by "+allPlayers[j].name,3f);
                                break;
                            }
                        }
                        //UIController.instance.ShowkillLog(allPlayers[i].name+" was killed",3f);
                        break;
                }

                if (i == Index)
                {
                    UpdateStatsDisplay();
                }
                
                if (UIController.instance.leaderboard.activeInHierarchy)
                {
                    ShowLeaderboard();
                }

                break;
            }
        }
       
        ScoreCheck();
    }
    //Starting a new match send Info
    public void NextMatchSend()
    {
        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.NextMatch,
            null,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }

    //Starting anew match Received Info
    public void NextMatchReceive()
    {
        state = GameState.Playing;

        UIController.instance.endScreen.SetActive(false);
        UIController.instance.leaderboard.SetActive(false);
        //Resst Player scores
        foreach (PlayerInfo player in allPlayers)
        {
            player.kills = 0;
            player.deaths = 0;
        }
        //Update new Stats
        UpdateStatsDisplay();
        //Let all player spawn
        PlayerSpawner.instance.SpawnPlayer();
        //reset the Timer after starting anew match
        SetupTimer();
    }

    //Sync the timmer over network
    //send timer data
    public void TimerSend()
    {
        object[] package = new object[] { (int)currentMatchTime, state };

        PhotonNetwork.RaiseEvent(
            (byte)EventCodes.TimerSync,
            package,
            new RaiseEventOptions { Receivers = ReceiverGroup.All },
            new SendOptions { Reliability = true }
            );
    }

    public void TimerReceive(object[] dataReceived)
    {
        currentMatchTime = (int)dataReceived[0];
        state = (GameState)dataReceived[1];

        UpdateTimerDisplay();

        UIController.instance.timerText.gameObject.SetActive(true);
    }

    #endregion

    public void UpdateStatsDisplay()
    {
        if (allPlayers.Count > Index)
        {

            UIController.instance.killsText.text = "Kills: " + allPlayers[Index].kills;
            UIController.instance.deathsText.text = "Deaths: " + allPlayers[Index].deaths;
        }
        else
        {
            UIController.instance.killsText.text = "Kills: 0";
            UIController.instance.deathsText.text = "Deaths: 0";
        }
    }


    void ShowLeaderboard()
    {
        UIController.instance.leaderboard.SetActive(true);

        foreach (LeaderboardPlayer lp in lboardPlayers)
        {
            Destroy(lp.gameObject);
        }
        lboardPlayers.Clear();

        UIController.instance.leaderboardPlayerDisplay.gameObject.SetActive(false);

        List<PlayerInfo> sorted = SortPlayers(allPlayers);

        foreach (PlayerInfo player in sorted)
        {
            LeaderboardPlayer newPlayerDisplay = Instantiate(UIController.instance.leaderboardPlayerDisplay, UIController.instance.leaderboardPlayerDisplay.transform.parent);

            newPlayerDisplay.SetDetails(player.name, player.kills, player.deaths);

            newPlayerDisplay.gameObject.SetActive(true);

            lboardPlayers.Add(newPlayerDisplay);
        }
    }



    private List<PlayerInfo> SortPlayers(List<PlayerInfo> players)
    {
        List<PlayerInfo> sorted = new List<PlayerInfo>();

        while (sorted.Count < players.Count)
        {
            int highest = -1;
            PlayerInfo selectedPlayer = players[0];

            foreach (PlayerInfo player in players)
            {
                if (!sorted.Contains(player))
                {
                    if (player.kills > highest)
                    {
                        selectedPlayer = player;
                        highest = player.kills;
                    }
                }
            }

            sorted.Add(selectedPlayer);
        }

        return sorted;
    }


    void ScoreCheck()
    {
        bool winnerFound = false;

        foreach (PlayerInfo player in allPlayers)
        {
            if (player.kills >= killsToWin && killsToWin > 0)
            {
                winnerFound = true;
                break;
            }
        }

        if (winnerFound)
        {
            if (PhotonNetwork.IsMasterClient && state != GameState.Ending)
            {
                state = GameState.Ending;
                ListPlayerSend();
            }
        }
    }

    void StateCheck()
    {
        if (state == GameState.Ending)
        {
            EndGame();
        }
    }
    #region Match End
    void EndGame()
    {
        state = GameState.Ending;

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.DestroyAll();
        }

        UIController.instance.endScreen.SetActive(true);
        //Deactivate all other widgets
        UIController.instance.optionsScreen.SetActive(false);
        UIController.instance.overHeatedText.SetActive(false);

        ShowLeaderboard();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Camera.main.transform.position = mapCamPoint.position;
        Camera.main.transform.rotation = mapCamPoint.rotation;

        StartCoroutine(EndCo());
    }


    private IEnumerator EndCo()
    {
        int temp_holding_waitingTime=waitAfterEnding;
        while(temp_holding_waitingTime>0)
        {
            yield return new WaitForSeconds(1);
            temp_holding_waitingTime--;
            UIController.instance.nextMatchStartCountdown_text.text="STARTING NEW MATCH IN: "+temp_holding_waitingTime;
        }
        //yield return new WaitForSeconds(waitAfterEnding);
        if (!perpetual)
        {
            PhotonNetwork.AutomaticallySyncScene = false;
            PhotonNetwork.LeaveRoom();
        }
        else
        {
            if(PhotonNetwork.IsMasterClient && !isLoadingMap)
            {
                if (!Launcher.instance.changeMapBetweenRounds)
                {
                    NextMatchSend();
                }
                else
                {
                    int newLevel = Random.Range(0, Launcher.instance.allMaps.Count);

                    if (Launcher.instance.allMaps[newLevel] == SceneManager.GetActiveScene().name)
                    {
                        NextMatchSend();
                    }
                    else
                    {
                        isLoadingMap=true;
                        PhotonNetwork.LoadLevel(Launcher.instance.allMaps[newLevel]);
                    }
                }
            }
        }
        
    }
    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        SceneManager.LoadScene(0);
    }
    #endregion
    public void SetupTimer()
    {
        if (matchLength > 0)
        {
            currentMatchTime = matchLength;
            UpdateTimerDisplay();
        }
    }

    public void UpdateTimerDisplay()
    {

        var timeToDisplay = System.TimeSpan.FromSeconds(currentMatchTime);

        UIController.instance.timerText.text = timeToDisplay.Minutes.ToString("00") + ":" + timeToDisplay.Seconds.ToString("00");
        if(currentMatchTime<=0)
        {
            state = GameState.Ending;
            StateCheck();
        }
    }
}


[System.Serializable]
public class PlayerInfo
{
    public string name;
    public int actor, kills, deaths;

    //Constructer
    public PlayerInfo(string name,int actor,int kills,int deaths)
    {
        this.name = name;
        this.actor = actor;
        this.kills = kills;
        this.deaths = deaths;
    }

    public PlayerInfo(PlayerInfo player)
    {
        name = player.name;
        actor = player.actor;
        kills = player.kills;
        deaths = player.deaths;
    }
}

using UnityEngine;
using Photon.Realtime;
using TMPro;

public class RoomButton : MonoBehaviour
{
    [SerializeField] TMP_Text roomText;
    private RoomInfo info;

    public void SetRoomInfo(RoomInfo info)
    {
        this.info=info;
        roomText.text=info.Name;
    }

    //button
    public void OpenRoom()
    {
        Launcher.instance.JoinRoom(info);
    }
}

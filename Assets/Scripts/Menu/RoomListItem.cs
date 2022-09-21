using Photon.Realtime;
using TMPro;
using UnityEngine;

public class RoomListItem : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    public RoomInfo roomInfo;

    public void SetUp(RoomInfo info)
    {
        roomInfo = info;
        text.text = info.Name;
    }

    public void OnClick()
    {
        Launcher.Singleton.JoinRoom(roomInfo);
    }
}

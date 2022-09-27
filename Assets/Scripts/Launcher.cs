using Photon.Pun;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{
    public enum LEVELS // enum with level build indexes
    {
        SampleScene,
        StartScene
    }

    public static Launcher Singleton;

    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] Transform roomListContent;
    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject roomListItemPrefab;
    [SerializeField] GameObject playerListItemPrefab;
    [SerializeField] GameObject startGameButton;

    private static Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();

    private void Awake()
    {
        Singleton = this;
    }

    void Start()
    {
        Debug.Log("Connecting to master");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to master");
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedLobby()
    {
        MenuManager.Singleton.OpenMenu("TitleMenu");
        Debug.Log("Joined lobby");
    }

    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInputField.text))
            return;

        PhotonNetwork.CreateRoom(roomNameInputField.text);
        MenuManager.Singleton.OpenMenu("LoadingMenu");
    }

    public override void OnJoinedRoom()
    {
        DestroyPlayerList();    // clear list to avoid players duplication, fake connections, etc.

        MenuManager.Singleton.OpenMenu("RoomMenu");
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        foreach (Player player in PhotonNetwork.PlayerList)
            Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(player);

        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);    // only host can start a game
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Room creation failed: " + message;
        MenuManager.Singleton.OpenMenu("ErrorMenu");
    }

    public void StartGame()
    {
        PhotonNetwork.LoadLevel((ushort)LEVELS.StartScene);
    }

    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.Singleton.OpenMenu("LoadingMenu");
    }    

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        MenuManager.Singleton.OpenMenu("LoadingMenu");
    }

    public override void OnLeftRoom()
    {
        MenuManager.Singleton.OpenMenu("TitleMenu");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        DestroyRoomList();  // clear list to avoid room duplication

        for(int i = 0; i < roomList.Count; i++)
        {
            RoomInfo info = roomList[i];
            if (info.RemovedFromList)
                cachedRoomList.Remove(info.Name);
            else
                cachedRoomList[info.Name] = info;
        }

        foreach (KeyValuePair<string, RoomInfo> room in cachedRoomList)
        {
            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(cachedRoomList[room.Key]);
        }
    }

    public void DestroyRoomList()
    {
        foreach (Transform trans in roomListContent)
            Destroy(trans.gameObject);
    }

    public void DestroyPlayerList()
    {
        foreach (Transform trans in playerListContent)
            Destroy(trans.gameObject);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);
    }
}

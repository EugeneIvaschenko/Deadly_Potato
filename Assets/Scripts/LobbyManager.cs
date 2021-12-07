using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks {
    [SerializeField] private Button _joinRoomButton = null;
    [SerializeField] private InputField _nicknameInput = null;

    public Text LogText;

    void Start() {
        PhotonNetwork.NickName = "Player " + Random.Range(1000, 9999);
        Log("Player's name is set to " + PhotonNetwork.NickName);
        _nicknameInput.text = PhotonNetwork.NickName;

        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = "0.0.6";
        PhotonNetwork.ConnectUsingSettings();
    }

    private void Update() {
        if (Input.GetKeyDown("escape")) Application.Quit();
    }

    public void OnEditNickname() {
        if(_nicknameInput.text != "") PhotonNetwork.NickName = _nicknameInput.text;
    }

    public override void OnConnectedToMaster() {
        Log("Connected to Master");
        _joinRoomButton.interactable = true;
        SpawnZones.ClearZones();
    }

    public void CreateRoom() {
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 5 });
    }

    public void JoinRoom() {
        if (PhotonNetwork.CountOfRooms > 0) PhotonNetwork.JoinRandomRoom();
        else CreateRoom();
    }

    public override void OnJoinedRoom() {
        Log("Joined the room");

        PhotonNetwork.LoadLevel("KeillLevel");
    }

    private void Log(string message) {
        Debug.Log(message);
        LogText.text += "\n";
        LogText.text += message;
    }
}
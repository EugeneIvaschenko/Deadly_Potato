using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SessionManager : MonoBehaviourPunCallbacks {
    public GameObject PlayerPrefab;
    public CameraFollowing cameraFollower;

    void Start() {
        Vector3 pos = new Vector3(Random.Range(-5f, 5f), 3f, Random.Range(-1f, 1f));
        GameObject player = PhotonNetwork.Instantiate(PlayerPrefab.name, pos, Quaternion.identity);
        cameraFollower.target = player.transform;
        UpdateOnlineList();
        if (PhotonNetwork.IsMasterClient) {
            DestroingSerializator.SetWallHashtables();
        }
        else {
            DestroingSerializator.SynchronizeWalls();
        }
    }

    public void Leave() {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom() {
        //Текущий игрок покинул комнату
        SceneManager.LoadScene("Lobby");
        Debug.Log("You leaved room");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) {
        Debug.LogFormat("Player {0} entered room", newPlayer.NickName);
        UpdateOnlineList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer) {
        Debug.LogFormat("Player {0} left room", otherPlayer.NickName);
        UpdateOnlineList();
    }

    private void UpdateOnlineList() {
        string onlineList = "";
        foreach (KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players) {
            onlineList += player.Value.NickName;
            onlineList += "\n";
        }
        Messenger<string>.Broadcast(GameEvent.ONLINE_LIST_UPDATE, onlineList);
    }
}
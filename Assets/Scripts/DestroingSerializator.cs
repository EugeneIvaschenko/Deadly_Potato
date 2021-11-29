using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class DestroingSerializator : MonoBehaviourPunCallbacks {
    private static Breakable[] walls;

    void Awake() {
        walls = FindObjectsOfType<Breakable>();
        Debug.Log("Finded " + walls.Length + " walls");
    }

    //hashing walls on room creating
    public static void SetWallHashtables() {
        foreach (Breakable wall in walls) {
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { wall.Id, true } });
        }
    }

    //synchronize walls on enter room
    public static void SynchronizeWalls() {
        DestroyWallsOnEnterRoom();
        Debug.Log("Synchronizing walls");
    }

    private static void DestroyWallsOnEnterRoom() {
        Hashtable wallsHash = PhotonNetwork.CurrentRoom.CustomProperties;
        Debug.Log(wallsHash.ToString());
        foreach (Breakable wall in walls) {
            if (wallsHash.ContainsKey(wall.Id)) {
                if (!(bool)wallsHash[wall.Id]) {
                    if (wall != null) {
                        Destroy(wall.gameObject);
                        Debug.LogFormat("{0} wall destroyed on enter room", wall.Id);
                    }
                }
            }
        }
    }

    public static void DestroyWall(string wallId) {
        Hashtable wallsHash = new Hashtable { { wallId, false } };
        Debug.Log("Hash to destroy: " + wallsHash.ToString());
        PhotonNetwork.CurrentRoom.SetCustomProperties(wallsHash);
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged) {
        Debug.Log("OnHashUpdate" + propertiesThatChanged.ToString());
        foreach(Breakable wall in walls) {
            if (propertiesThatChanged.ContainsKey(wall.Id)) {
                if (!(bool)propertiesThatChanged[wall.Id]) {
                    Debug.LogFormat("wall {0} destroyed", wall.Id);
                    if(wall != null) Destroy(wall.gameObject);
                }
            }
        }
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour {
    private static List<SpawnZone> spawnZones = new List<SpawnZone>();

    private void Awake() {
        Messenger<PlayerController, int>.AddListener(GameEvent.PLAYER_DIED, OnPlayerDied);
    }

    private void OnDestroy() {
        Messenger<PlayerController, int>.RemoveListener(GameEvent.PLAYER_DIED, OnPlayerDied);
    }

    public static void AddToSpawnList(SpawnZone zone) {
        spawnZones.Add(zone);
    }

    private void OnPlayerDied(PlayerController player, int time = 0) {
        StartCoroutine(DelayedRespawn(player, time));
    }

    private IEnumerator DelayedRespawn(PlayerController player, int time) {
        yield return new WaitForSeconds(time);
        while (true) {
            if (player._photonView.IsMine) {
                if (spawnZones.Count > 0) {
                    int spawnNumber = Random.Range(0, spawnZones.Count - 1);
                    if (spawnZones[spawnNumber] == null || spawnZones[spawnNumber].gameObject == null) continue;
                    player.transform.position = spawnZones[spawnNumber].gameObject.transform.position;
                }
                else {
                    player.transform.position = new Vector3(0f, 1.5f, 0f);
                }
                player.Rebirth();
                break;
            }
            else break;
        }
    }
}
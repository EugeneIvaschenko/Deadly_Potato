using System.Collections.Generic;
using UnityEngine;

public class SpawnZones {
    private static List<SpawnZone> spawnZones = new List<SpawnZone>();
    public static Vector3 LevelCenter = new Vector3(0f, 1.5f, 0f);

    public static void AddToSpawnList(SpawnZone zone) {
        spawnZones.Add(zone);
    }

    public static Vector3 GetRandomSpawnPoint() {
        return spawnZones.Count > 0 ? spawnZones[Random.Range(0, spawnZones.Count - 1)].gameObject.transform.position : LevelCenter;
    }
}
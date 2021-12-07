using UnityEngine;

public class SpawnZone : MonoBehaviour {
    private void Awake() {
        SpawnZones.AddToSpawnList(this);
    }
}
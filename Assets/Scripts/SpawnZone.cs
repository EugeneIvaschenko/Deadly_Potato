using UnityEngine;

public class SpawnZone : MonoBehaviour {
    private void Start() {
        SpawnManager.AddToSpawnList(this);
    }
}
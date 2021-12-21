using UnityEngine;

public class EffectZone : MonoBehaviour {

    [SerializeField] private ZoneEffectType effectType = ZoneEffectType.None;

    private Collider _collider;

    void OnEnable()
    {
        if (TryGetComponent(out Collider collider)) {
            _collider = GetComponent<Collider>();
            _collider.isTrigger = true;
        }
        if (TryGetComponent(out MeshCollider meshCol)) {
            meshCol.convex = true;
            meshCol.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.TryGetComponent(out PlayerPhysics physics)) {
            if (effectType != ZoneEffectType.None) physics.AddEffect(effectType);
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.TryGetComponent(out PlayerPhysics physics)) {
            if (effectType != ZoneEffectType.None) physics.RemoveEffect(effectType);
        }
    }
}

public enum ZoneEffectType {
    None,
    SpeedUp,
    SpeedDown,
    AccelerationUp,
    AccelerationDown,
    RotationSpeedUp,
    RotationSpeedDown
}
using UnityEngine;

public class CameraFollowing : MonoBehaviour {
    public Transform target;

    public float smoothTime = 0.2f;

    private Vector3 _offset = new Vector3(0, -15.5f, 3.5f);
    private Vector3 _velocity = Vector3.zero;
    Vector3 newPosition;

    public void Start() {
        newPosition = transform.position;
    }
    
    void LateUpdate() {
        if (target == null) return;
        newPosition = target.position - _offset;

        transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref _velocity, smoothTime, Mathf.Infinity, Time.deltaTime);
        if (target.position.z < transform.position.z) {
            transform.position = new Vector3(transform.position.x, transform.position.y, target.position.z);
        }

        Vector3 look = new Vector3(transform.position.x, target.position.y, target.position.z);
        transform.LookAt(look);
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirTracker : MonoBehaviour {
    public PlayerController playerBall;
    public float distance = 1;

    private Rigidbody _rigid;

    private void Start() {
        _rigid = playerBall.GetComponent<Rigidbody>();
        transform.position = Vector3.forward * distance + playerBall.transform.position;
        transform.LookAt(playerBall.transform);
    }

    private void Update() {
        if (_rigid == null) return;
        if (new Vector3(_rigid.velocity.x, 0, _rigid.velocity.z).magnitude > 0.01) UpdatePosition();
        else UpdateYPosition();
    }

    private void UpdatePosition() {
        Vector3 direction = new Vector3(_rigid.velocity.x, 0, _rigid.velocity.z).normalized;
        transform.position = direction * distance + playerBall.transform.position;
        transform.LookAt(playerBall.transform);
    }

    private void UpdateYPosition() {
        transform.position = new Vector3(transform.position.x, playerBall.transform.position.y, transform.position.z);
    }
}
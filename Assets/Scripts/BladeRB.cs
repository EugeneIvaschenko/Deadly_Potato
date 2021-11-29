using UnityEngine;

public class BladeRB : MonoBehaviour {

    public Rigidbody RB { get; set; }

    void Start() {
        RB = GetComponent<Rigidbody>();
        gameObject.SetActive(false);
    }
}
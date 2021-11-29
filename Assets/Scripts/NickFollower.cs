using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NickFollower : MonoBehaviour {
    private Text textField;
    public Transform Target { get; set; }
    public string PlayerId { get; set; }

    void Awake() {
        textField = GetComponentInChildren<Text>();
        transform.position = Vector3.zero;
    }
    
    void LateUpdate() {
        if (Target == null) {
            if(gameObject != null) Destroy(gameObject);
            return;
        }
        transform.position = Camera.main.WorldToScreenPoint(Target.position) + new Vector3(0, 35);
    }

    public void SetNick(string nick) {
        textField.text = nick;
    }
}

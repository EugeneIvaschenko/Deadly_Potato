using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakable : MonoBehaviour{
    public static Color speedCond = new Color(0.233357f, 0.233357f, 0.8679245f);
    public static Color bladeCond = new Color(0.8396226f, 0.2099057f, 0.2099057f);
    public static Color anyCond = new Color(0.1569064f, 0.8113208f, 0.1569064f);
    public static Color bothCond = new Color(0.6509434f, 0.2057227f, 0.6509434f);

    public BreakCondition breakCondition = BreakCondition.Speed;
    public float speedForBreak = 14;

    [SerializeField] private string id;
    private bool isBladeCollision = false;
    private bool isSpeedCollision = false;

    private Renderer _mat;

    public string Id { get => "WID" + id; private set => id = value; }

    private void Start() {
        _mat = GetComponent<Renderer>();
        switch (breakCondition) {
            case BreakCondition.Speed:
                _mat.material.color = speedCond;
                break;
            case BreakCondition.Blade:
                _mat.material.color = bladeCond;
                break;
            case BreakCondition.Any:
                _mat.material.color = anyCond;
                break;
            case BreakCondition.Both:
                _mat.material.color = bothCond;
                break;
        }
    }

    public bool TryBreak(float speed, bool isBlade) {
        if(speed >= speedForBreak) isSpeedCollision = true;
        isBladeCollision = isBlade;

        switch (breakCondition) {
            case BreakCondition.Speed:
                if (isSpeedCollision) {
                    DestroyThisWall();
                    return true;
                }
                break;
            case BreakCondition.Blade:
                if (isBladeCollision) {
                    DestroyThisWall();
                    return true;
                }
                break;
            case BreakCondition.Any:
                if (isSpeedCollision || isBladeCollision) {
                    DestroyThisWall();
                    return true;
                }
                break;
            case BreakCondition.Both:
                if (isSpeedCollision && isBladeCollision) {
                    DestroyThisWall();
                    return true;
                }
                break;
        }
        isSpeedCollision = false;
        isBladeCollision = false;
        return false;
    }

    private void DestroyThisWall() {
        DestroingSerializator.DestroyWall(Id);
        Debug.LogFormat("I've destroy {0} wall", Id);
        gameObject.SetActive(false);
    }
}

public enum BreakCondition {
    Speed,
    Blade,
    Any,
    Both
}
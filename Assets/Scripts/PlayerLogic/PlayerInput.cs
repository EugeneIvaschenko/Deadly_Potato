using UnityEngine;

public class PlayerInput : MonoBehaviour {
    private Joystick moveStick = null;

    public float horInput { get; private set; }
    public float vertInput { get; private set; }
    public bool turboInput { get; private set; }
    public bool brakingInput { get; set; }
    public bool brakingDown { get; private set; }
    public bool brakingUp { get; private set; }
    public bool attackInput { get; private set; }
    public bool shieldInput { get; private set; }
    public bool tabInput { get; private set; }
    public bool isAxisInput { get; private set; } = false;

    public void InputRead() {
#if UNITY_STANDALONE || UNITY_WEBGL
        PCInputRead();
#elif UNITY_ANDROID || UNITY_IOS
        MobileInputRead();
#endif
    }

    private void PCInputRead() {
        horInput = Input.GetAxis("Horizontal");
        vertInput = Input.GetAxis("Vertical");
        turboInput = Input.GetButtonDown("Turbo");
        attackInput = Input.GetButtonDown("Attack");
        shieldInput = Input.GetKeyDown("e");
        if (brakingInput != Input.GetKey("q")) {
            Messenger<bool>.Broadcast(GameEvent.BRAKING_SWITCHED, Input.GetKey("q"));
            brakingDown = Input.GetKey("q");
            if (Input.GetKey("q")) {
                brakingDown = true;
            } else {
                brakingUp = true;
            }
        } else {
            brakingDown = false;
            brakingUp = false;
        }
        brakingInput = Input.GetKey("q");
        if (tabInput != Input.GetKey(KeyCode.Tab))
            Messenger<bool>.Broadcast(GameEvent.ONLINE_LIST_VISIBLE, Input.GetKey(KeyCode.Tab));
        tabInput = Input.GetKey(KeyCode.Tab);
        isAxisInput = (vertInput != 0 || horInput != 0);
    }

    private void MobileInputRead() {
        if (moveStick == null) {
            Messenger<PlayerInput>.Broadcast(GameEvent.GET_MOVESTICK, this);
        } else {
            horInput = moveStick.Horizontal;
            vertInput = moveStick.Vertical;
            if (tabInput != Input.GetKey(KeyCode.Tab))
                Messenger<bool>.Broadcast(GameEvent.ONLINE_LIST_VISIBLE, Input.GetKey(KeyCode.Tab));
            tabInput = Input.GetKey(KeyCode.Tab);
            isAxisInput = (vertInput != 0 || horInput != 0);
        }
    }

    public void SetMoveStick(Joystick joystick) {
        moveStick = joystick;
    }

    public void AttackOnDown() { attackInput = true; }

    public void AttackOnUp() { attackInput = false; }

    public void TurboOnDown() { turboInput = true; }

    public void TurboOnUp() { turboInput = false; }

    public void ShieldOnDown() { shieldInput = true; }

    public void ShieldOnUp() { shieldInput = false; }

    public void BrakeOnDown() {
        if (!brakingInput)
            Messenger<bool>.Broadcast(GameEvent.BRAKING_SWITCHED, true);
        brakingInput = true;
    }

    public void BrakeOnUp() {
        if (brakingInput)
            Messenger<bool>.Broadcast(GameEvent.BRAKING_SWITCHED, false);
        brakingInput = false;
    }
}

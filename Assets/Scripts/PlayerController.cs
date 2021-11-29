using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour, IPunObservable, IOnEventCallback {
    [SerializeField] private Blade blade = null;
    [SerializeField] private BladeRB bladeRB = null;
    [SerializeField] private Shield shield = null;
    [SerializeField] private GameObject arrowTracker = null;

    private Joystick moveStick = null;

    public float attackDelay = 1.0f;
    public float turboDuration = 3.0f;
    public float turboDelay = 6.0f;
    public float shieldDuration = 3.0f;
    public float shieldDelay = 6.0f;

    public float normalSpeed = 10.0f;
    public float turboSpeed = 20.0f;
    public float shieldSpeed = 7.0f;
    public float acceleration = 10.0f;
    public float turboAccel = 20.0f;
    public float deceleration = 5.0f;
    public float brakingDecel = 20.0f;

    private float curMaxSpeed = 0f;
    private float targetMaxSpeed = 0f;

    private bool isNeedAbsoluteSerialize = true;

    private Rigidbody _rigid;
    private SphereCollider _collider;
    private PhotonView photonView;
    private Vector3 _networkPosition;
    private Quaternion _networkRotation;

    private float horInput;
    private float vertInput;
    private bool turboInput;
    private bool brakingInput;
    private bool attackInput;
    private bool shieldInput;
    private bool tabInput;

    private bool isAttack = false;
    private bool canAttack = true;
    private bool canTurbo = true;
    private bool isTurbo = false;
    private bool isTurboPreparing = false;
    private bool canShield = true;
    public bool IsShield { get; private set; } = false;
    private bool isAxisInput = false;
    public bool IsDead { get; private set; } = false;

    private void Start() {
        _rigid = GetComponent<Rigidbody>();
        _collider = GetComponent<SphereCollider>();
        photonView = GetComponent<PhotonView>();
        shield.lifetime = shieldDuration;
        if (photonView.IsMine) {
            Instantiate(arrowTracker).GetComponent<DirTracker>().playerBall = this;
        } else {
            Messenger<string, string, Transform>.Broadcast(GameEvent.PLAYER_ENTERED_ROOM, photonView.Owner.NickName, photonView.Owner.UserId, transform);
        }
    }

    public void NetworkRebirth(int actorNr) {
        if (actorNr == photonView.OwnerActorNr) {
            Rebirth(false);
            isNeedAbsoluteSerialize = true;
        }
    }

    public void Rebirth(bool sendEvent = true) {
        IsDead = false;
        gameObject.SetActive(true);

        if (sendEvent) {
            int content = photonView.ControllerActorNr;
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            PhotonNetwork.RaiseEvent(GameNetworkEvent.PLAYER_REBIRTH, content, raiseEventOptions, SendOptions.SendReliable);
        }
    }

    public void Kill(bool sendEvent = true, int lagTime = 0) {
        if (IsDead) return;
        DisableTurbo();
        DisablaShield();
        StopAttack();
        canTurbo = true;
        canShield = true;
        _rigid.velocity = Vector3.zero;
        IsDead = true;
        gameObject.SetActive(false);
        int rebirthTime = 2 - lagTime;
        Messenger<PlayerController, int>.Broadcast(GameEvent.PLAYER_DIED, this, rebirthTime);

        if (sendEvent) {
            int[] content = new int[] { photonView.OwnerActorNr, rebirthTime };
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(GameNetworkEvent.PLAYER_DIED, content, raiseEventOptions, SendOptions.SendReliable);
        }
    }

    private void OnDestroy() {
        Messenger<string>.Broadcast(GameEvent.PLAYER_LEFT_ROOM, photonView.Owner.UserId);
    }

    private void Update() {
        if (Input.GetKeyDown("escape")) Application.Quit();

        if (!IsDead && transform.position.y < -5) {
            if (!photonView.IsMine) {
                isNeedAbsoluteSerialize = true;
                return;
            }
            Kill();
        }

        blade.transform.localRotation = Quaternion.Inverse(transform.rotation);

        if (!photonView.IsMine) {
            SynchronizeSkills();
            return;
        }
        if (!IsDead) {
            InputRead();
            Skills();
        }

        Vector3 xzVector3 = _rigid.velocity;
        xzVector3.y = 0;
        Messenger<float>.Broadcast(GameEvent.SPEED_CHANGED, xzVector3.magnitude / turboSpeed);
    }

    private void FixedUpdate() {
        BallRigidMoving();
    }

    private void InputRead() {
#if UNITY_STANDALONE
        horInput = Input.GetAxis("Horizontal");
        vertInput = Input.GetAxis("Vertical");
        turboInput = Input.GetButtonDown("Turbo");
        attackInput = Input.GetButtonDown("Attack");
        shieldInput = Input.GetKeyDown("e");
        if(brakingInput != Input.GetKey("q")) Messenger<bool>.Broadcast(GameEvent.BRAKING_SWITCHED, Input.GetKey("q"));
        brakingInput = Input.GetKey("q");
        if (tabInput != Input.GetKey(KeyCode.Tab)) Messenger<bool>.Broadcast(GameEvent.ONLINE_LIST_VISIBLE, Input.GetKey(KeyCode.Tab));
        tabInput = Input.GetKey(KeyCode.Tab);
        isAxisInput = (vertInput != 0 || horInput != 0);

#elif UNITY_ANDROID || UNITY_IOS
        if (moveStick == null) {
            Messenger<PlayerController>.Broadcast(GameEvent.GET_MOVESTICK, this);
        }
        else {
            horInput = moveStick.Horizontal;
            vertInput = moveStick.Vertical;
            if (tabInput != Input.GetKey(KeyCode.Tab)) Messenger<bool>.Broadcast(GameEvent.ONLINE_LIST_VISIBLE, Input.GetKey(KeyCode.Tab));
            tabInput = Input.GetKey(KeyCode.Tab);
            isAxisInput = (vertInput != 0 || horInput != 0);
        }
#endif
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
        if (!brakingInput) Messenger<bool>.Broadcast(GameEvent.BRAKING_SWITCHED, true);
        brakingInput = true;
    }

    public void BrakeOnUp() {
        if (brakingInput) Messenger<bool>.Broadcast(GameEvent.BRAKING_SWITCHED, false);
        brakingInput = false;
    }

    private void SynchronizeSkills() {
        if (isAttack && canAttack) {
            Attack();
        }
        if (isTurbo && canTurbo && !IsShield) {
            EnableTurbo();
        }
        if (brakingInput && isTurbo) {
            DisableTurbo();
        }
        if (IsShield && canShield) {
            EnableShield();
        }
    }

    private void Skills() {
        if (attackInput && canAttack && !IsShield) {
            Attack();
        }
        if (turboInput && canTurbo && !IsShield) {
            if (_rigid.velocity.magnitude < 1 && !isAxisInput) {
                isTurboPreparing = true;
                canTurbo = false;
            } else {
                EnableTurbo();
            }
        }
        if (isTurboPreparing && _rigid.velocity.magnitude >= 0.3) {
            EnableTurbo();
        }
        if (brakingInput && isTurboPreparing) {
            isTurboPreparing = false;
            StartCoroutine(TurboRefresh());
        }
        if(brakingInput && isTurbo) {
            DisableTurbo();
        }
        if (shieldInput && canShield) {
            EnableShield();
        }
    }

    private void Attack() {
        blade.Attack();
        canAttack = false;
        isAttack = true;
        StartCoroutine(AttackRefresh());
        StartCoroutine(StopAttacking());
    }

    private IEnumerator AttackRefresh() {
        if (photonView.IsMine) Messenger<float>.Broadcast(GameEvent.ATTACK_REFRESH, attackDelay);
        yield return new WaitForSeconds(attackDelay);
        canAttack = true;
    }

    private IEnumerator StopAttacking() {
        yield return new WaitForSeconds(blade.lifetime);
        isAttack = false;
    }

    private void StopAttack() {
        isAttack = false;
        canAttack = true;
        blade.Refresh();
    }

    private void EnableShield() {
        shield.ActivateShield();
        IsShield = true;
        if (photonView.IsMine) Messenger<bool>.Broadcast(GameEvent.SHIELD_SWITCHED, IsShield);
        canShield = false;
        DisableTurbo();
        if (isTurboPreparing) {
            isTurboPreparing = false;
            StartCoroutine(TurboRefresh());
        }
        StartCoroutine(ShieldRefresh());
        StartCoroutine(ShieldDelayDeactivating());
    }

    private IEnumerator ShieldRefresh() {
        if (photonView.IsMine) Messenger<float>.Broadcast(GameEvent.SHIELD_REFRESH, shieldDelay);
        yield return new WaitForSeconds(shieldDelay);
        canShield = true;
    }

    private IEnumerator ShieldDelayDeactivating() {
        yield return new WaitForSeconds(shieldDuration);
        DisablaShield();
    }

    private void DisablaShield() {
        IsShield = false;
        if (photonView.IsMine) Messenger<bool>.Broadcast(GameEvent.SHIELD_SWITCHED, IsShield);
    }

    private void EnableTurbo() {
        isTurbo = true;
        if (photonView.IsMine) Messenger<bool>.Broadcast(GameEvent.TURBO_SWITCHED, isTurbo);
        canTurbo = false;
        isTurboPreparing = false;
        StartCoroutine(TurboRefresh());
        StartCoroutine(TurboDelayDeactivating());
    }

    private void DisableTurbo() {
        isTurbo = false;
        if (photonView.IsMine) Messenger<bool>.Broadcast(GameEvent.TURBO_SWITCHED, isTurbo);
    }

    private IEnumerator TurboRefresh() {
        if (photonView.IsMine) Messenger<float>.Broadcast(GameEvent.TURBO_REFRESH, turboDelay);
        yield return new WaitForSeconds(turboDelay);
        canTurbo = true;
    }

    private IEnumerator TurboDelayDeactivating() {
        yield return new WaitForSeconds(turboDuration);
        DisableTurbo();
    }

    private void BallRigidMoving() {
        if (photonView.IsMine) {
            TryCollision();

            targetMaxSpeed = IsShield ? shieldSpeed : isTurbo ? turboSpeed : normalSpeed;
            if (targetMaxSpeed < curMaxSpeed) curMaxSpeed += (targetMaxSpeed - curMaxSpeed) * Time.fixedDeltaTime * 0.8f;
            else curMaxSpeed = targetMaxSpeed;

            if (!brakingInput && (isTurbo || horInput != 0 || vertInput != 0)) { //Активное движение
                Vector3 targetDirection = new Vector3(horInput, 0, vertInput);
                if (isTurbo) { //Медленный разворот во время турбо
                    float radians = 15f;
                    Vector3 tmpDir = _rigid.velocity;
                    tmpDir.y = 0;
                    float savedY = _rigid.velocity.y;
                    targetDirection = Vector3.RotateTowards(tmpDir, targetDirection, radians * Time.fixedDeltaTime, 0);
                    _rigid.velocity.Set(targetDirection.x, savedY, targetDirection.z);
                }
                //Изменение вектора скорости при активном движении
                targetDirection = Vector3.ClampMagnitude(targetDirection, 1);
                Vector3 deltaVelocity = targetDirection * Time.fixedDeltaTime * (isTurbo ? turboAccel : acceleration);
                _rigid.velocity += deltaVelocity;
                Vector3 tmp = _rigid.velocity; //Временный вектор, чтобы выполнить функцию ClampMagnitude() без учёта координаты Y, т.е. не влияем на падение
                tmp.y = 0;
                tmp = Vector3.ClampMagnitude(tmp, curMaxSpeed);
                tmp.y = _rigid.velocity.y;
                _rigid.velocity = tmp;
            }
            else { //Пасивное замедление или торможение
                Vector3 tmp = _rigid.velocity; //Временный вектор, чтобы не изменить скорость падения в следующих преобразованиях. Координата Y 
                tmp.y = 0;
                Vector3 drag = tmp.normalized;
                Vector3 deltaVelocity = drag * Time.fixedDeltaTime * (brakingInput ? brakingDecel : deceleration);
                if (Vector3.Dot(tmp, tmp - deltaVelocity) > 0)
                    _rigid.velocity -= deltaVelocity;
                else {
                    _rigid.velocity = new Vector3(0, _rigid.velocity.y, 0);
                }
            }
        }
        else {
            _rigid.position = Vector3.MoveTowards(_rigid.position, _networkPosition, Time.fixedDeltaTime);
            _rigid.rotation = Quaternion.RotateTowards(_rigid.rotation, _networkRotation, Time.fixedDeltaTime * 100.0f);
        }

        if (brakingInput) _rigid.constraints = RigidbodyConstraints.FreezeRotation;
        else _rigid.constraints = RigidbodyConstraints.None;

        if (isTurboPreparing) {
            float degrees = 500 * Time.fixedDeltaTime;
            transform.Rotate(0, degrees, 0, Space.World);
        }
    }

    private void TryCollision() {
        RaycastHit[] hits;
        if (isAttack) {
            bladeRB.gameObject.SetActive(true);
            bladeRB.transform.localScale = blade.transform.localScale * 1.01f;
            float minDistance = 0.01f;
            float distance = (_rigid.velocity.magnitude > minDistance) ? _rigid.velocity.magnitude : minDistance;
            hits = bladeRB.RB.SweepTestAll(_rigid.velocity, distance * Time.fixedDeltaTime);
            bladeRB.gameObject.SetActive(false);
        } else {
            hits = _rigid.SweepTestAll(_rigid.velocity, _rigid.velocity.magnitude * Time.fixedDeltaTime);
        }
        foreach (RaycastHit hit in hits) {
            if (hit.collider != null) {
                Breakable wall = hit.collider.gameObject.GetComponent<Breakable>();
                if (wall != null) {
                    bool wallDestroyed = wall.TryBreak(_rigid.velocity.magnitude, isAttack);
                    if (wallDestroyed) return;
                }
                if (isTurbo && hit.collider.gameObject.CompareTag("Wall")) {
                    _rigid.velocity = Vector3.Reflect(_rigid.velocity, hit.normal);
                    return;
                }
                PlayerController player = hit.collider.gameObject.GetComponent<PlayerController>();
                if (player != null) {
                    if (player.Equals(this)) return;
                    if(isAttack && !player.IsShield && !player.IsDead) {
                        player.Kill();
                    }
                }
            }
        }
    }

    private void KillPlayer(int actorNr, int time) {
        if (photonView.OwnerActorNr == actorNr) {
            Kill(false);
            Debug.LogFormat("Player {0} is dead", actorNr);
            return;
        }
    }

    public void OnEvent(EventData photonEvent) {
        byte eventCode = photonEvent.Code;

        switch (eventCode) {
            case GameNetworkEvent.PLAYER_DIED:
                int[] dieData = (int[])photonEvent.CustomData;
                KillPlayer(dieData[0], dieData[1]);
                break;
            case GameNetworkEvent.PLAYER_REBIRTH:
                int actorNrRebirth = (int)photonEvent.CustomData;
                Debug.LogFormat("Player {0} is Rebirth", actorNrRebirth);
                Messenger<int>.Broadcast(GameEvent.PLAYER_REBIRTH, actorNrRebirth);
                break;
        }
    }

    private void OnEnable() {
        PhotonNetwork.AddCallbackTarget(this);
        Messenger<int>.AddListener(GameEvent.PLAYER_REBIRTH, NetworkRebirth);
    }

    private void OnDisable() {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (_rigid == null) return;
        if (stream.IsWriting) {
            stream.SendNext(_rigid.position);
            stream.SendNext(_rigid.rotation);
            stream.SendNext(_rigid.velocity);
            stream.SendNext(isAttack);
            stream.SendNext(isTurbo);
            stream.SendNext(isTurboPreparing);
            stream.SendNext(IsShield);
            stream.SendNext(brakingInput);
        }
        else {
            _networkPosition = (Vector3)stream.ReceiveNext();
            _networkRotation = (Quaternion)stream.ReceiveNext();
            _rigid.velocity = (Vector3)stream.ReceiveNext();
            isAttack = (bool)stream.ReceiveNext();
            isTurbo = (bool)stream.ReceiveNext();
            isTurboPreparing = (bool)stream.ReceiveNext();
            IsShield = (bool)stream.ReceiveNext();
            brakingInput = (bool)stream.ReceiveNext();

            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            _networkPosition += _rigid.velocity * lag;

            //Телепорт игрока, если он далеко от своего реального местоположения. Нужно придумать компенсацию лагов получше. Refuck
            if ((_rigid.position - _networkPosition).magnitude > (isTurbo ? 10f : 4f)) isNeedAbsoluteSerialize = true;

            if (isNeedAbsoluteSerialize) {
                _rigid.position = _networkPosition;
                _rigid.rotation = _networkRotation;
                isNeedAbsoluteSerialize = false;
            }
        }
    }
}
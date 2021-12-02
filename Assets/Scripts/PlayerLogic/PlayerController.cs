using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerAbilities))]
[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerPhysics))]
[RequireComponent(typeof(PlayerNetworkSync))]
public class PlayerController : MonoBehaviour {
    [SerializeField] public Blade blade = null;
    [SerializeField] public Shield shield = null;
    [SerializeField] private GameObject arrowTracker = null;

    private Rigidbody _rigid;
    private SphereCollider _collider;
    private PlayerNetworkSync _playerSync;
    public PlayerAbilities _abilities { get; private set; }
    public PlayerInput _playerInput { get; private set; }
    public PlayerPhysics _physics { get; private set; }
    public PhotonView _photonView { get; private set; }

    public bool IsDead { get; private set; } = false;

    private void Start() {
        _rigid = GetComponent<Rigidbody>();
        _collider = GetComponent<SphereCollider>();
        _photonView = GetComponent<PhotonView>();
        _abilities = GetComponent<PlayerAbilities>();
        _playerInput = GetComponent<PlayerInput>();
        _physics = GetComponent<PlayerPhysics>();
        _playerSync = GetComponent<PlayerNetworkSync>();

        shield.lifetime = _abilities.shieldDuration;
        if (_photonView.IsMine) {
            Instantiate(arrowTracker).GetComponent<DirTracker>().playerBall = this;
        } else {
            Messenger<string, string, Transform>.Broadcast(GameEvent.PLAYER_ENTERED_ROOM, _photonView.Owner.NickName, _photonView.Owner.UserId, transform);
        }
    }

    private void Update() {
        if (Input.GetKeyDown("escape")) Application.Quit();

        if (!IsDead && transform.position.y < -5) {
            if (!_photonView.IsMine) {
                _playerSync.isNeedAbsoluteSerialize = true;
                return;
            }
            KillThis();
        }

        blade.transform.localRotation = Quaternion.Inverse(transform.rotation);

        if (!_photonView.IsMine) {
            _playerSync.SynchronizeSkills();
            return;
        }

        if (!IsDead) {
            _playerInput.InputRead();
            _abilities.Skills();
        }

        Vector3 xzVector3 = _rigid.velocity;
        xzVector3.y = 0;
        Messenger<float>.Broadcast(GameEvent.SPEED_CHANGED, xzVector3.magnitude / _abilities.turboSpeed);
    }

    private void FixedUpdate() {
        _physics.BallRigidMoving();
    }

    private void OnDestroy() {
        Messenger<string>.Broadcast(GameEvent.PLAYER_LEFT_ROOM, _photonView.Owner.UserId);
    }

    private void OnEnable() {
        PhotonNetwork.AddCallbackTarget(this);
        Messenger<int>.AddListener(GameEvent.PLAYER_REBIRTH, NetworkRebirth);
    }

    private void OnDisable() {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void NetworkRebirth(int actorNr) {
        if (actorNr == _photonView.OwnerActorNr) {
            Rebirth(false);
            _playerSync.isNeedAbsoluteSerialize = true;
        }
    }

    public void Rebirth(bool sendEvent = true) {
        IsDead = false;
        gameObject.SetActive(true);

        if (sendEvent) {
            int content = _photonView.ControllerActorNr;
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
            PhotonNetwork.RaiseEvent(GameNetworkEvent.PLAYER_REBIRTH, content, raiseEventOptions, SendOptions.SendReliable);
        }
    }

    public void KillThis(bool sendEvent = true, int lagTime = 0) {
        if (IsDead) return;
        _abilities.DisableTurbo();
        _abilities.DisablaShield();
        _abilities.StopAttack();
        _abilities.CanTurbo = true;
        _abilities.CanShield = true;
        _rigid.velocity = Vector3.zero;
        IsDead = true;
        gameObject.SetActive(false);
        int rebirthTime = 2 - lagTime;
        Messenger<PlayerController, int>.Broadcast(GameEvent.PLAYER_DIED, this, rebirthTime);

        if (sendEvent) {
            int[] content = new int[] { _photonView.OwnerActorNr};
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
            PhotonNetwork.RaiseEvent(GameNetworkEvent.PLAYER_DIED, content, raiseEventOptions, SendOptions.SendReliable);
        }
    }
}
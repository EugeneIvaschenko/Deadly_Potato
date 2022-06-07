using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerAbilities))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerPhysics))]
[RequireComponent(typeof(PlayerNetworkSync))]
[RequireComponent(typeof(EffectsHandler))]
public class PlayerController : MonoBehaviour {
    [SerializeField] public BladeAnimation blade = null;
    [SerializeField] public Shield shield = null;
    [SerializeField] private GameObject arrowTracker = null;
    [SerializeField] private SkinsSO skins;
    [SerializeField] private int delayBetweenEffectStartAndSpawn = 50;

    private PlayerNetworkSync _playerSync;
    public Rigidbody _rigid { get; private set; }
    public PlayerAbilities _abilities { get; private set; }
    public PlayerInput _playerInput { get; private set; }
    public PlayerPhysics _physics { get; private set; }
    public PhotonView PhotonView { get; private set; }
    public int RebirthDelay { get; private set; } = 2;

    public bool IsDead { get; private set; } = false;
    public string _skinID { get; private set;}

    public Action<Vector3> OnShieldBlocked;
    public Action OnDeath;
    public Action OnRebirth;
    public Action<Vector3> OnCollShieldShield;
    public Action<Vector3> OnCollBladeWall;
    public Action<Vector3> OnCollBallWall;
    public Action<Vector3> OnCollShieldWall;
    public Action<bool> OnBreak;
    public Action<bool> OnTurbo;
    public Action<bool> OnChargeTurbo;
    public Action<bool> OnHigSpeed;
    public Action<Vector3> OnParry;

    public Action OnShieldActivation;
    public Action<Vector3> OnCollBallBall;

    private void Awake() {
        _rigid = GetComponent<Rigidbody>();
        PhotonView = GetComponent<PhotonView>();
        _abilities = GetComponent<PlayerAbilities>();
        _playerInput = GetComponent<PlayerInput>();
        _physics = GetComponent<PlayerPhysics>();
        _playerSync = GetComponent<PlayerNetworkSync>();
    }

    private void Start() {
        shield.lifetime = _abilities.shieldDuration;
        if (PhotonView.IsMine) {
            Instantiate(arrowTracker).GetComponent<DirTracker>().playerBall = this;
            SetSkin(PlayerSettings.selectedSkinID);
        } else {
            Messenger<string, string, Transform>.Broadcast(GameEvent.PLAYER_ENTERED_ROOM, PhotonView.Owner.NickName, PhotonView.Owner.UserId, transform);
        }
    }

    private void Update() {
        if (Input.GetKeyDown("escape")) Application.Quit();

        if (!IsDead && transform.position.y < -5) {
            Debug.Log("MustDie");
            if (!PhotonView.IsMine) {
                _playerSync.isNeedAbsoluteSerialize = true;
                return;
            }
            Kill();
        }

        blade.transform.localRotation = Quaternion.Inverse(transform.rotation);

        if (!PhotonView.IsMine) {
            _playerSync.SynchronizeSkills();
            return;
        }

        if (!IsDead) {
            _playerInput.InputRead();
            _abilities.Skills();
        }

        Vector3 xzVector3 = _rigid.velocity;
        xzVector3.y = 0;
        Messenger<float>.Broadcast(GameEvent.SPEED_CHANGED, xzVector3.magnitude / _physics.turboSpeed);
    }

    private void FixedUpdate() {
        _physics.BallRigidMoving();
    }

    public void SetSkin(string skinID) {
        _skinID = skinID;
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material = (from skin in skins.skinList where skin.uniqID == skinID select skin.materials).First();
    }

    public void Kill() {
        if (IsDead) return;
        Debug.Log("Died");
        OnDeath?.Invoke();
        _abilities.DisableTurbo();
        _abilities.DisablaShield();
        _abilities.StopAttack();
        _abilities.CanTurbo = true;
        _abilities.CanShield = true;
        _rigid.velocity = Vector3.zero;
        IsDead = true;
        gameObject.SetActive(false);
        DelayedRespawn();
        if (!PhotonView.IsMine) transform.position = new Vector3(1500, 300, 0);
    }

    public void Rebirth() {
        if (!IsDead) return;
        IsDead = false;
        gameObject.SetActive(true);
    }

    public void PrepareRebirth() {
        if (!IsDead) return;
        transform.position = SpawnZones.GetRandomSpawnPoint();
        OnRebirth?.Invoke();
    }

    public void NetworkKill(int ownerActorNr) {
        int[] content = new int[] { ownerActorNr };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(GameNetworkEvent.PLAYER_DIED, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void NetworkRebirth(int ownerActorNr) {
        int[] content = new int[] { ownerActorNr };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All };
        PhotonNetwork.RaiseEvent(GameNetworkEvent.PLAYER_REBIRTH, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public bool TryBreakWall(Collider other) {
        if (_abilities.IsAttack && other.CompareTag("Wall")) {
            Vector3 pos = Vector3.MoveTowards(transform.position, other.transform.position, blade.transform.lossyScale.x);
            OnCollBladeWall?.Invoke(pos);
        }
        Breakable wall = other.gameObject.GetComponent<Breakable>();
        if (wall != null) {
            bool wallDestroyed = wall.TryBreak(_rigid.velocity.magnitude, _abilities.IsAttack);
            return wallDestroyed;
        }
        return false;
    }

    public bool TryKillPlayer(Collider other) {
        PlayerController player = other.gameObject.GetComponent<PlayerController>();
        if (player == null) return false;
        if (player.Equals(this)) return false;
        if (player._abilities.IsShield && _abilities.IsAttack) {
            Vector3 pos = Vector3.MoveTowards(player.transform.position, transform.position, player.transform.lossyScale.x);
            player.OnShieldBlocked?.Invoke(pos);
            return false;
        }
        if(player._abilities.IsShield && _abilities.IsShield) {
            Vector3 pos = Vector3.Lerp(player.transform.position, transform.position, 0.5f);
            OnCollShieldShield?.Invoke(pos);
            return false;
        }
        if (_abilities.IsAttack && player._abilities.IsAttack) {
            OnParry?.Invoke(Vector3.Lerp(transform.position, player.transform.position, 0.5f));
            Vector3 normal = player.transform.position - Vector3.Lerp(transform.position, player.transform.position, 0.5f);
            _rigid.velocity = Vector3.Reflect(_rigid.velocity, normal);
            return false;
        }
        if (_abilities.IsAttack && !player._abilities.IsShield && !player.IsDead) {
            player.Kill();
            NetworkKill(player.PhotonView.OwnerActorNr);
            return true;
        }
        OnCollBallBall?.Invoke(Vector3.Lerp(transform.position, player.transform.position, 0.5f));
        return false;
    }

    private async void DelayedRespawn() {
        await Task.Delay(RebirthDelay * 1000);
        PrepareRebirth();
        if (delayBetweenEffectStartAndSpawn > 0) await Task.Delay(delayBetweenEffectStartAndSpawn);
        Rebirth();
        NetworkRebirth(PhotonView.OwnerActorNr);
        Debug.Log("DelayedRespawn() - " + PhotonView.OwnerActorNr);
    }
}
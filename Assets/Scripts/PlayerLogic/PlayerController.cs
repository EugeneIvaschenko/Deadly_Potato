using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerAbilities))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerPhysics))]
[RequireComponent(typeof(PlayerNetworkSync))]
public class PlayerController : MonoBehaviour {
    [SerializeField] public BladeAnimation blade = null;
    [SerializeField] public Shield shield = null;
    [SerializeField] private GameObject arrowTracker = null;
    [SerializeField] private SkinListSO skins;

    private Rigidbody _rigid;
    private PlayerNetworkSync _playerSync;
    public PlayerAbilities _abilities { get; private set; }
    public PlayerInput _playerInput { get; private set; }
    public PlayerPhysics _physics { get; private set; }
    public PhotonView PhotonView { get; private set; }
    public int RebirthDelay { get; private set; } = 2;

    public bool IsDead { get; private set; } = false;
    public string _skinID { get; private set;}

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
        transform.position = SpawnZones.GetRandomSpawnPoint();
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
        if (player.PhotonView.IsMine) return false;
        if (player.Equals(this)) return false;
        if (_abilities.IsAttack && !player._abilities.IsShield && !player.IsDead) {
            player.Kill();
            NetworkKill(player.PhotonView.OwnerActorNr);
            return true;
        }
        return false;
    }

    private async void DelayedRespawn() {
        //yield return new WaitForSeconds(RebirthDelay);
        await Task.Delay(RebirthDelay * 1000);
        Rebirth();
        NetworkRebirth(PhotonView.OwnerActorNr);
        Debug.Log("DelayedRespawn() - " + PhotonView.OwnerActorNr);
    }
}
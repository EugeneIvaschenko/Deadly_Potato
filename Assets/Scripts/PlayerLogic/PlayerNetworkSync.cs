using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PlayerNetworkSync : MonoBehaviour, IPunObservable, IOnEventCallback {
    public Vector3 _networkPosition { get; private set; }
    public Quaternion _networkRotation { get; private set; }
    public bool isNeedAbsoluteSerialize { get; set; } = true;
    
    private Rigidbody _rigid;
    private PlayerAbilities _abilities;
    private PlayerInput _playerInput;

    private void Start() {
        _rigid = GetComponent<Rigidbody>();
        _abilities = GetComponent<PlayerAbilities>();
        _playerInput = GetComponent<PlayerInput>();
    }

    public void SynchronizeSkills() {
        if (_abilities.IsAttack && _abilities.CanAttack) {
            _abilities.Attack();
        }
        if (_abilities.IsTurbo && _abilities.CanTurbo && !_abilities.IsShield) {
            _abilities.EnableTurbo();
        }
        if (_playerInput.brakingInput && _abilities.IsTurbo) {
            _abilities.DisableTurbo();
        }
        if (_abilities.IsShield && _abilities.CanShield) {
            _abilities.EnableShield();
        }
    }

    public void OnEvent(EventData photonEvent) {
        byte eventCode = photonEvent.Code;

        switch (eventCode) {
            case GameNetworkEvent.PLAYER_DIED:
                int[] dieData = (int[])photonEvent.CustomData;
                _abilities.KillPlayer(dieData[0]);
                break;
            case GameNetworkEvent.PLAYER_REBIRTH:
                int actorNrRebirth = (int)photonEvent.CustomData;
                Debug.LogFormat("Player {0} is Rebirth", actorNrRebirth);
                Messenger<int>.Broadcast(GameEvent.PLAYER_REBIRTH, actorNrRebirth);
                break;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (_rigid == null) return;
        if (stream.IsWriting) {
            stream.SendNext(_rigid.position);
            stream.SendNext(_rigid.rotation);
            stream.SendNext(_rigid.velocity);
            stream.SendNext(_abilities.IsAttack);
            stream.SendNext(_abilities.IsTurbo);
            stream.SendNext(_abilities.IsTurboPreparing);
            stream.SendNext(_abilities.IsShield);
            stream.SendNext(_playerInput.brakingInput);
        }
        else {
            _networkPosition = (Vector3)stream.ReceiveNext();
            _networkRotation = (Quaternion)stream.ReceiveNext();
            _rigid.velocity = (Vector3)stream.ReceiveNext();
            _abilities.IsAttack = (bool)stream.ReceiveNext();
            _abilities.IsTurbo = (bool)stream.ReceiveNext();
            _abilities.IsTurboPreparing = (bool)stream.ReceiveNext();
            _abilities.IsShield = (bool)stream.ReceiveNext();
            _playerInput.brakingInput = (bool)stream.ReceiveNext();

            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            _networkPosition += _rigid.velocity * lag;

            //Телепорт игрока, если он далеко от своего реального местоположения. Нужно придумать компенсацию лагов получше. Refuck
            if ((_rigid.position - _networkPosition).magnitude > (_abilities.IsTurbo ? 10f : 4f)) isNeedAbsoluteSerialize = true;

            if (isNeedAbsoluteSerialize) {
                _rigid.position = _networkPosition;
                _rigid.rotation = _networkRotation;
                isNeedAbsoluteSerialize = false;
            }
        }
    }
}
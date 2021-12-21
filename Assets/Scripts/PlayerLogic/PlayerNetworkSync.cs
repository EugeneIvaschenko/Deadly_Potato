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
    private PlayerController _playerController;

    private void Awake() {
        _rigid = GetComponent<Rigidbody>();
        _abilities = GetComponent<PlayerAbilities>();
        _playerInput = GetComponent<PlayerInput>();
        _playerController = GetComponent<PlayerController>();
    }

    private void OnEnable() {
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable() {
        PhotonNetwork.RemoveCallbackTarget(this);
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
            stream.SendNext(_playerController._skinID);
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
            string skinID = (string)stream.ReceiveNext();
            if(skinID != _playerController._skinID) {
                _playerController.SetSkin(skinID);
            }

            float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            _networkPosition += _rigid.velocity * lag;

            //Телепорт игрока, если он далеко от своего реального местоположения. Нужно придумать компенсацию лагов получше. TODO
            if ((_rigid.position - _networkPosition).magnitude > (_abilities.IsTurbo ? 10f : 4f)) isNeedAbsoluteSerialize = true;

            TryAbsolutePositionSerialize();
        }
    }

    public void OnEvent(EventData photonEvent) {
        byte eventCode = photonEvent.Code;
        switch (eventCode) {
            case GameNetworkEvent.PLAYER_DIED:
                int[] eventDataDie = (int[])photonEvent.CustomData;
                Debug.LogFormat("Die event. Data - {0}, receiver - {1}", eventDataDie[0], _playerController.PhotonView.OwnerActorNr);
                if (_playerController.PhotonView.OwnerActorNr == eventDataDie[0]) {
                    _playerController.Kill();
                    Debug.LogFormat("Player {0} is Died", eventDataDie[0]);
                }
                break;
            case GameNetworkEvent.PLAYER_REBIRTH:
                int[] eventDataBirth = (int[])photonEvent.CustomData;
                Debug.LogFormat("Rebirth event. Data - {0}, receiver - {1}", eventDataBirth[0], _playerController.PhotonView.OwnerActorNr);
                if (_playerController.PhotonView.OwnerActorNr == eventDataBirth[0]) {
                    _playerController.Rebirth();
                    Debug.LogFormat("Player {0} is Rebirthed", eventDataBirth[0]);
                }
                break;
        }
    }

    private void TryAbsolutePositionSerialize() {
        if (isNeedAbsoluteSerialize) {
            _rigid.position = _networkPosition;
            _rigid.rotation = _networkRotation;
            isNeedAbsoluteSerialize = false;
        }
    }
}
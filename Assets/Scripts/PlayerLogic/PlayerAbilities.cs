using System;
using System.Collections;
using UnityEngine;

public class PlayerAbilities : MonoBehaviour
{
    private PlayerController _playerController;
    private Rigidbody _rigid;
    private PlayerInput _playerInput;

    private bool isTurboPreparing = false;

    public float attackDelay = 1.0f;
    public float turboDuration = 3.0f;
    public float turboDelay = 6.0f;
    public float shieldDuration = 3.0f;
    public float shieldDelay = 6.0f;

    public bool IsAttack { get; set; } = false;
    public bool CanAttack { get; set; } = true;
    public bool CanTurbo { get; set; } = true;
    public bool IsTurbo { get; set; } = false;
    public bool IsTurboPreparing {
        get {
            return isTurboPreparing;
        }
        set {
            if (isTurboPreparing != value) _playerController.OnChargeTurbo?.Invoke(value);
            isTurboPreparing = value;
        }
    }
    public bool CanShield { get; set; } = true;
    public bool IsShield { get; set; } = false;
    public bool IsHighSpeed { get; private set; } = false;

    private void Awake() {
        _rigid = GetComponent<Rigidbody>();
        _playerInput = GetComponent<PlayerInput>();
        _playerController = GetComponent<PlayerController>();
    }

    public void Skills() {
        if (_playerInput.attackInput && CanAttack && !IsShield) Attack();
        if (_playerInput.turboInput && CanTurbo && !IsShield) {
            if (_rigid.velocity.magnitude < 1 && !_playerInput.isAxisInput) {
                IsTurboPreparing = true;
                CanTurbo = false;
            }
            else {
                EnableTurbo();
            }
        }
        if (IsTurboPreparing && _rigid.velocity.magnitude >= 0.3) EnableTurbo();
        if (_playerInput.brakingInput && IsTurboPreparing) {
            IsTurboPreparing = false;
            StartCoroutine(TurboRefresh());
        }
        if (_playerInput.brakingInput && IsTurbo) DisableTurbo();
        if (_playerInput.shieldInput && CanShield) EnableShield();
        if (_playerInput.brakingDown) _playerController.OnBreak?.Invoke(true);
        if (_playerInput.brakingUp) _playerController.OnBreak?.Invoke(false);
        if (_playerInput.brakingInput && _rigid.velocity.magnitude < 0.1f) _playerController.OnBreak?.Invoke(false);
        if (_rigid.velocity.magnitude > 13f) {
            if (!IsHighSpeed) {
                IsHighSpeed = true;
                _playerController.OnHigSpeed?.Invoke(true);
            }
        }
        else {
            if (IsHighSpeed) {
                IsHighSpeed = false;
                _playerController.OnHigSpeed?.Invoke(false);
            }
        }
    }

    public void Attack() {
        _playerController.blade.Run();
        CanAttack = false;
        IsAttack = true;
        StartCoroutine(AttackRefresh());
        StartCoroutine(StopAttacking());
    }

    public void StopAttack() {
        IsAttack = false;
        CanAttack = true;
        _playerController.blade.Refresh();
    }

    private IEnumerator AttackRefresh() {
        if (_playerController.PhotonView.IsMine) Messenger<float>.Broadcast(GameEvent.ATTACK_REFRESH, attackDelay);
        yield return new WaitForSeconds(attackDelay);
        CanAttack = true;
    }

    private IEnumerator StopAttacking() {
        yield return new WaitForSeconds(_playerController.blade.duration);
        IsAttack = false;
    }

    public void EnableShield() {
        _playerController.shield.Activate();
        IsShield = true;
        if (_playerController.PhotonView.IsMine) Messenger<bool>.Broadcast(GameEvent.SHIELD_SWITCHED, IsShield);
        _playerController.OnShieldActivation?.Invoke();
        CanShield = false;
        DisableTurbo();
        if (IsTurboPreparing) {
            IsTurboPreparing = false;
            StartCoroutine(TurboRefresh());
        }
        StartCoroutine(ShieldRefresh());
        StartCoroutine(ShieldDelayDeactivating());
    }

    public void DisablaShield() {
        _playerController.shield.Deactivate();
        IsShield = false;
        if (_playerController.PhotonView.IsMine) Messenger<bool>.Broadcast(GameEvent.SHIELD_SWITCHED, IsShield);
    }

    private IEnumerator ShieldRefresh() {
        if (_playerController.PhotonView.IsMine) Messenger<float>.Broadcast(GameEvent.SHIELD_REFRESH, shieldDelay);
        yield return new WaitForSeconds(shieldDelay);
        CanShield = true;
    }

    private IEnumerator ShieldDelayDeactivating() {
        yield return new WaitForSeconds(shieldDuration);
        DisablaShield();
    }

    public void EnableTurbo() {
        IsTurbo = true;
        if (_playerController.PhotonView.IsMine) Messenger<bool>.Broadcast(GameEvent.TURBO_SWITCHED, IsTurbo);
        CanTurbo = false;
        IsTurboPreparing = false;
        StartCoroutine(TurboRefresh());
        StartCoroutine(TurboDelayDeactivating());
        _playerController.OnTurbo?.Invoke(true);
    }

    public void DisableTurbo() {
        IsTurbo = false;
        if (_playerController.PhotonView.IsMine) Messenger<bool>.Broadcast(GameEvent.TURBO_SWITCHED, IsTurbo);
        _playerController.OnTurbo?.Invoke(false);
    }

    private IEnumerator TurboRefresh() {
        if (_playerController.PhotonView.IsMine) Messenger<float>.Broadcast(GameEvent.TURBO_REFRESH, turboDelay);
        yield return new WaitForSeconds(turboDelay);
        CanTurbo = true;
    }

    private IEnumerator TurboDelayDeactivating() {
        yield return new WaitForSeconds(turboDuration);
        DisableTurbo();
    }
}
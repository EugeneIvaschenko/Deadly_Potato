using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPhysics : MonoBehaviour {
    private Rigidbody _rigid;
    private SphereCollider _collider;
    private PhotonView _photonView;
    private PlayerAbilities _abilities;
    private PlayerInput _playerInput;
    private PlayerNetworkSync _playerSync;
    private PlayerController _playerController;

    private float curMaxSpeed = 0f;
    private float targetMaxSpeed = 0f;
    private float curAcceleration = 0f;
    private float curRotationSpeed = 0f;

    private List<ZoneEffectType> effects = new List<ZoneEffectType>();

    public float normalSpeed = 10.0f;
    public float turboSpeed = 20.0f;
    public float shieldSpeed = 7.0f;

    public float acceleration = 10.0f;
    public float deceleration = 5.0f;
    public float rotantionSpeed = 0;

    public float turboAccel = 20.0f;
    public float brakingDecel = 20.0f;

    public float speedUpEffect = 5;
    public float speedDownEffect = 5;
    public float accelerationUpEffect = 5;
    public float accelerationDownEffect = 5;
    public float rotationSpeedUpEffect = 2;
    public float rotationSpeedDownEffect = 1;

    private void Awake() {
        _rigid = GetComponent<Rigidbody>();
        _collider = GetComponent<SphereCollider>();
        _photonView = GetComponent<PhotonView>();
        _abilities = GetComponent<PlayerAbilities>();
        _playerInput = GetComponent<PlayerInput>();
        _playerSync = GetComponent<PlayerNetworkSync>();
        _playerController = GetComponent<PlayerController>();
    }

    public void BallRigidMoving() {
        if (_photonView.IsMine) {
            TryCollision();
            CalcCurrentSpeeds();

            if (!_playerInput.brakingInput && (_abilities.IsTurbo || _playerInput.horInput != 0 || _playerInput.vertInput != 0)) { //Активное движение
                Vector3 targetDirection = new Vector3(_playerInput.horInput, 0, _playerInput.vertInput);
                if (_abilities.IsTurbo) { //Медленный разворот во время турбо
                    float radians = 15f;
                    Vector3 tmpDir = _rigid.velocity;
                    tmpDir.y = 0;
                    float savedY = _rigid.velocity.y;
                    targetDirection = Vector3.RotateTowards(tmpDir, targetDirection, radians * Time.fixedDeltaTime, 0);
                    _rigid.velocity.Set(targetDirection.x, savedY, targetDirection.z);
                }
                //Изменение вектора скорости при активном движении + поворот
                targetDirection = targetDirection.normalized;
                float directionDifference = _abilities.IsTurbo ? 0 : (Vector3.Dot(targetDirection, _rigid.velocity.normalized) * -1 + 1) * 0.5f;
                Vector3 deltaVelocity = targetDirection * Time.fixedDeltaTime * (_abilities.IsTurbo ? turboAccel : curAcceleration + curAcceleration * directionDifference);
                _rigid.velocity += deltaVelocity;
                _rigid.velocity = ClampMagnitudeIgnoreY(_rigid.velocity, curMaxSpeed);
                Vector3 newTargetVelocity = targetDirection * new Vector3(_rigid.velocity.x, 0, _rigid.velocity.z).magnitude;
                newTargetVelocity.y = _rigid.velocity.y;
                _rigid.velocity = Vector3.Lerp(_rigid.velocity, newTargetVelocity, curRotationSpeed * Time.fixedDeltaTime);
            }
            else { //Пасивное замедление или торможение
                Vector3 tmp = _rigid.velocity; //Временный вектор, чтобы не изменить скорость падения в следующих преобразованиях. Координата Y 
                tmp.y = 0;
                Vector3 drag = tmp.normalized;
                Vector3 deltaVelocity = drag * Time.fixedDeltaTime * (_playerInput.brakingInput ? brakingDecel : deceleration);
                if (Vector3.Dot(tmp, tmp - deltaVelocity) > 0)
                    _rigid.velocity -= deltaVelocity;
                else {
                    _rigid.velocity = new Vector3(0, _rigid.velocity.y, 0);
                }
            }
        }
        else {
            _rigid.position = Vector3.MoveTowards(_rigid.position, _playerSync._networkPosition, Time.fixedDeltaTime);
            _rigid.rotation = Quaternion.RotateTowards(_rigid.rotation, _playerSync._networkRotation, Time.fixedDeltaTime * 100.0f);
        }

        if (_playerInput.brakingInput) _rigid.constraints = RigidbodyConstraints.FreezeRotation;
        else _rigid.constraints = RigidbodyConstraints.None;

        if (_abilities.IsTurboPreparing) {
            float degrees = 500 * Time.fixedDeltaTime;
            transform.Rotate(0, degrees, 0, Space.World);
        }
    }

    private Vector3 ClampMagnitudeIgnoreY(Vector3 vector, float maxLength) {
        Vector3 tmp = vector; //Временный вектор, чтобы выполнить функцию ClampMagnitude() без учёта координаты Y, т.е. не влияем на падение
        tmp.y = 0;
        tmp = Vector3.ClampMagnitude(tmp, maxLength);
        tmp.y = vector.y;
        return tmp;
    }

    private void CalcCurrentSpeeds() {
        targetMaxSpeed = _abilities.IsShield ? shieldSpeed : _abilities.IsTurbo ? turboSpeed : normalSpeed;
        if (effects.Contains(ZoneEffectType.SpeedUp)) targetMaxSpeed += speedUpEffect;
        if (effects.Contains(ZoneEffectType.SpeedDown)) targetMaxSpeed -= speedDownEffect;
        if (targetMaxSpeed < curMaxSpeed) curMaxSpeed += (targetMaxSpeed - curMaxSpeed) * Time.fixedDeltaTime;
        else curMaxSpeed = targetMaxSpeed;
        if(effects.Contains(ZoneEffectType.SpeedDown)) curMaxSpeed = targetMaxSpeed;

        curAcceleration = acceleration;
        if (effects.Contains(ZoneEffectType.AccelerationUp)) curAcceleration += accelerationUpEffect;
        if (effects.Contains(ZoneEffectType.AccelerationDown)) curAcceleration -= accelerationDownEffect;

        curRotationSpeed = rotantionSpeed;
        if (effects.Contains(ZoneEffectType.RotationSpeedUp)) curRotationSpeed += rotationSpeedUpEffect;
        if (effects.Contains(ZoneEffectType.RotationSpeedDown)) curRotationSpeed -= rotationSpeedDownEffect;
    }

    private void TryCollision() {
        RaycastHit[] hits = _rigid.SweepTestAll(_rigid.velocity, _rigid.velocity.magnitude * Time.fixedDeltaTime);

        foreach (RaycastHit hit in hits) {
            if (hit.collider != null) {
                if (hit.collider.gameObject.CompareTag("Wall")) {
                    if (_playerController.TryBreakWall(hit.collider)) continue;
                    if (_abilities.IsTurbo) _rigid.velocity = Vector3.Reflect(_rigid.velocity, hit.normal);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (_playerController.TryBreakWall(other)) return;
        _playerController.TryKillPlayer(other);
    }

    public void AddEffect(ZoneEffectType effect) {
        if (!effects.Contains(effect)) effects.Add(effect);
    }

    public void RemoveEffect(ZoneEffectType effect) {
        if (effects.Contains(effect)) effects.Remove(effect);
    }
}
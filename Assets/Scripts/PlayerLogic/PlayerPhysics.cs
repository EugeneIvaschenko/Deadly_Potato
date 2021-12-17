using Photon.Pun;
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

    public float normalSpeed = 10.0f;
    public float turboSpeed = 20.0f;
    public float shieldSpeed = 7.0f;
    public float acceleration = 10.0f;
    public float turboAccel = 20.0f;
    public float deceleration = 5.0f;
    public float brakingDecel = 20.0f;

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

            targetMaxSpeed = _abilities.IsShield ? shieldSpeed : _abilities.IsTurbo ? turboSpeed : normalSpeed;
            if (targetMaxSpeed < curMaxSpeed) curMaxSpeed += (targetMaxSpeed - curMaxSpeed) * Time.fixedDeltaTime * 0.8f;
            else curMaxSpeed = targetMaxSpeed;

            if (!_playerInput.brakingInput && (_abilities.IsTurbo || _playerInput.horInput != 0 || _playerInput.vertInput != 0)) { //�������� ��������
                Vector3 targetDirection = new Vector3(_playerInput.horInput, 0, _playerInput.vertInput);
                if (_abilities.IsTurbo) { //��������� �������� �� ����� �����
                    float radians = 15f;
                    Vector3 tmpDir = _rigid.velocity;
                    tmpDir.y = 0;
                    float savedY = _rigid.velocity.y;
                    targetDirection = Vector3.RotateTowards(tmpDir, targetDirection, radians * Time.fixedDeltaTime, 0);
                    _rigid.velocity.Set(targetDirection.x, savedY, targetDirection.z);
                }
                //��������� ������� �������� ��� �������� ��������
                targetDirection = targetDirection.normalized;
                float directionDifference = _abilities.IsTurbo ? 0 : (Vector3.Dot(targetDirection, _rigid.velocity.normalized) * -1 + 1) * 0.5f;
                Vector3 deltaVelocity = targetDirection * Time.fixedDeltaTime * (_abilities.IsTurbo ? turboAccel : acceleration + acceleration * directionDifference);
                _rigid.velocity += deltaVelocity;
                Vector3 tmp = _rigid.velocity; //��������� ������, ����� ��������� ������� ClampMagnitude() ��� ����� ���������� Y, �.�. �� ������ �� �������
                tmp.y = 0;
                tmp = Vector3.ClampMagnitude(tmp, curMaxSpeed);
                tmp.y = _rigid.velocity.y;
                _rigid.velocity = tmp;
            }
            else { //�������� ���������� ��� ����������
                Vector3 tmp = _rigid.velocity; //��������� ������, ����� �� �������� �������� ������� � ��������� ���������������. ���������� Y 
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
}
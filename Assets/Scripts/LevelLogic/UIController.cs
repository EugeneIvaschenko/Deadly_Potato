using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class UIController : MonoBehaviourPunCallbacks {
    [SerializeField] private Image speedIndicator;
    [SerializeField] private Image attackIndicator;
    [SerializeField] private Image shieldIndicator;
    [SerializeField] private Image turboIndicator;
    [SerializeField] private Image onlinePanel;
    [SerializeField] private Text onlineList;
    [SerializeField] private GameObject nicksLayer;
    [SerializeField] private GameObject nickFollowerPrefab;
    [SerializeField] private Joystick moveStick;
    [SerializeField] private GameObject mobileButtons;

    private PlayerInput _playerInput = null;

    public void AttackOnDown() { _playerInput.AttackOnDown(); }

    public void AttackOnUp() { _playerInput.AttackOnUp(); }

    public void TurboOnDown() { _playerInput.TurboOnDown(); }

    public void TurboOnUp() { _playerInput.TurboOnUp(); }

    public void ShieldOnDown() { _playerInput.ShieldOnDown(); }

    public void ShieldOnUp() { _playerInput.ShieldOnUp(); }

    public void BrakeOnDown() { _playerInput.BrakeOnDown(); }

    public void BrakeOnUp() { _playerInput.BrakeOnUp(); }

    private List<NickFollower> _nickBars = new List<NickFollower>();

    private Color _defaultSpeedColor;
    private Color _brakingSpeedColor;

    private Color _defaultTurboColor;
    private Color _activeTurboColor;

    private Color _defaultShieldColor;
    private Color _activeShieldColor;

    private void Start() {
        _defaultSpeedColor = speedIndicator.color;
        _brakingSpeedColor = new Color(0.7452f, 0.6251f, 0.1652f);

        _defaultTurboColor = turboIndicator.color;
        _activeTurboColor = _defaultTurboColor * 1.5f;

        _defaultShieldColor = shieldIndicator.color;
        _activeShieldColor = _defaultShieldColor * 1.5f;

        onlinePanel.gameObject.SetActive(false);

#if UNITY_STANDALONE || UNITY_WEBGL
        moveStick.gameObject.SetActive(false);
        mobileButtons.SetActive(false);
#endif
    }

    private void Awake() {
        Messenger<float>.AddListener(GameEvent.SPEED_CHANGED, OnSpeedChanged);
        Messenger<float>.AddListener(GameEvent.ATTACK_REFRESH, OnAttackRefresh);
        Messenger<float>.AddListener(GameEvent.TURBO_REFRESH, OnTurboRefresh);
        Messenger<float>.AddListener(GameEvent.SHIELD_REFRESH, OnShieldRefresh);
        Messenger<bool>.AddListener(GameEvent.BRAKING_SWITCHED, OnBrakingSwitching);
        Messenger<bool>.AddListener(GameEvent.SHIELD_SWITCHED, OnShieldSwitching);
        Messenger<bool>.AddListener(GameEvent.TURBO_SWITCHED, OnTurboSwitching);
        Messenger<bool>.AddListener(GameEvent.ONLINE_LIST_VISIBLE, OnOnlineListVisible);
        Messenger<string>.AddListener(GameEvent.ONLINE_LIST_UPDATE, OnOnlineListUpdate);
        Messenger<string, string, Transform>.AddListener(GameEvent.PLAYER_ENTERED_ROOM, OnPlayerEnteredRoom);
        Messenger<string>.AddListener(GameEvent.PLAYER_LEFT_ROOM, OnPlayerLeftRoom);
        Messenger<PlayerInput>.AddListener(GameEvent.GET_MOVESTICK, SetMobileControllers);
    }

    private void OnDestroy() {
        Messenger<float>.RemoveListener(GameEvent.SPEED_CHANGED, OnSpeedChanged);
        Messenger<float>.RemoveListener(GameEvent.ATTACK_REFRESH, OnAttackRefresh);
        Messenger<float>.RemoveListener(GameEvent.TURBO_REFRESH, OnTurboRefresh);
        Messenger<float>.RemoveListener(GameEvent.SHIELD_REFRESH, OnShieldRefresh);
        Messenger<bool>.RemoveListener(GameEvent.BRAKING_SWITCHED, OnBrakingSwitching);
        Messenger<bool>.RemoveListener(GameEvent.SHIELD_SWITCHED, OnShieldSwitching);
        Messenger<bool>.RemoveListener(GameEvent.TURBO_SWITCHED, OnTurboSwitching);
        Messenger<bool>.RemoveListener(GameEvent.ONLINE_LIST_VISIBLE, OnOnlineListVisible);
        Messenger<string>.RemoveListener(GameEvent.ONLINE_LIST_UPDATE, OnOnlineListUpdate);
        Messenger<string, string, Transform>.RemoveListener(GameEvent.PLAYER_ENTERED_ROOM, OnPlayerEnteredRoom);
        Messenger<string>.RemoveListener(GameEvent.PLAYER_LEFT_ROOM, OnPlayerLeftRoom);
        Messenger<PlayerInput>.RemoveListener(GameEvent.GET_MOVESTICK, SetMobileControllers);
    }

    private void SetMobileControllers(PlayerInput input) {
        _playerInput = input;
        _playerInput.SetMoveStick(moveStick);
    }

    private void OnPlayerEnteredRoom(string nickname, string id, Transform target) {
        NickFollower nickBar = Instantiate(nickFollowerPrefab, nicksLayer.transform).GetComponent<NickFollower>();
        nickBar.PlayerId = id;
        nickBar.SetNick(nickname);
        nickBar.Target = target;
        _nickBars.Add(nickBar);
    }

    private void OnPlayerLeftRoom(string id) {
        for(int i = 0; i < _nickBars.Count; i++) {
            if (_nickBars[i].PlayerId == id) {
                Destroy(_nickBars[i].gameObject);
                _nickBars.RemoveAt(i);
                return;
            }
        }
    }

    private void OnSpeedChanged(float value) {
        speedIndicator.fillAmount = value;
    }

    private void OnAttackRefresh(float time) {
        StartCoroutine(AttackIndicator(time));
    }

    private IEnumerator AttackIndicator(float time) {
        float timeLeft = 0;
        attackIndicator.fillAmount = 0;
        while (timeLeft < time) {
            yield return null;
            timeLeft += Time.deltaTime;
            attackIndicator.fillAmount = timeLeft / time;
        }
        attackIndicator.fillAmount = 1;
    }

    private void OnTurboRefresh(float time) {
        StartCoroutine(TurboIndicator(time));
    }

    private IEnumerator TurboIndicator(float time) {
        float timeLeft = 0;
        turboIndicator.fillAmount = 0;
        while (timeLeft < time) {
            yield return null;
            timeLeft += Time.deltaTime;
            turboIndicator.fillAmount = timeLeft / time / 2;
        }
        turboIndicator.fillAmount = 0.5f;
    }

    private void OnShieldRefresh(float time) {
        StartCoroutine(ShieldIndicator(time));
    }

    private IEnumerator ShieldIndicator(float time) {
        float timeLeft = 0;
        shieldIndicator.fillAmount = 0;
        while (timeLeft < time) {
            yield return null;
            timeLeft += Time.deltaTime;
            shieldIndicator.fillAmount = timeLeft / time / 2;
        }
        shieldIndicator.fillAmount = 0.5f;
    }

    private void OnBrakingSwitching(bool isBraking) {
        speedIndicator.color = isBraking ? _brakingSpeedColor : _defaultSpeedColor;
    }

    private void OnTurboSwitching(bool isTurbo) {
        turboIndicator.color = isTurbo ? _activeTurboColor : _defaultTurboColor;
    }

    private void OnShieldSwitching(bool isShield) {
        shieldIndicator.color = isShield ? _activeShieldColor : _defaultShieldColor;
    }

    private void OnOnlineListVisible(bool isVisible) {
        onlinePanel.gameObject.SetActive(isVisible);
    }

    private void OnOnlineListUpdate(string list) {
        onlineList.text = list;
    }
}
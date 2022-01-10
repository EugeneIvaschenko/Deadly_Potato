using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EffectsHandler : MonoBehaviour {
    [SerializeField] private EffectsSO effects;
    private Dictionary<SpecialEffectType, ParticleSystem> effectsPreset;

    private Rigidbody _rigid;
    private PlayerAbilities _abilities;
    private PlayerController _playerController;

    private void Awake() {
        _rigid = GetComponent<Rigidbody>();
        _abilities = GetComponent<PlayerAbilities>();
        _playerController = GetComponent<PlayerController>();
    }

    private void Start() {
        RegisterEvents();
        InstantiateEffects();
    }

    private void LateUpdate() {
        UpdateTurbo();
        UpdateBraking();
        UpdateHighSpeed();
    }

    private void RegisterEvents() {
        _playerController.OnTurbo += PlayOrStopTurbo;
        _playerController.OnShieldBlocked += PlayShieldBlocked;
        _playerController.OnDeath += PlayDeath;
        _playerController.OnRebirth += PlaySpawn;
        _playerController.OnCollShieldShield += PlayCollShieldShield;
        _playerController.OnCollBladeWall += PlayCollBladeWall;
        _playerController.OnCollBallWall += PlayCollBallWall;
        _playerController.OnCollShieldWall += PlayCollShieldWall;
        _playerController.OnBreak += PlayOrStopBraking;
        _playerController.OnHigSpeed += PlayOrStopHigSpeed;
        _playerController.OnChargeTurbo += PlayOrStopChargeTurbo;
        _playerController.OnParry += PlayParry;
        _playerController.OnShieldActivation += PlayShieldActivation;
        _playerController.OnCollBallBall += PlayCollBallBall;
    }

    private void InstantiateEffects() {
        effectsPreset = new Dictionary<SpecialEffectType, ParticleSystem>();
        foreach (SpecialEffectType effectType in Enum.GetValues(typeof(SpecialEffectType))) {
            AddParticleSystem(effectType, PlayerSettings.VFXPlayerCustomizations[effectType]);
        }
    }

    private void PlayCollBallBall(Vector3 pos) => PlayEffect(SpecialEffectType.CollBallBall, pos);

    private void PlayShieldActivation() => PlayEffect(SpecialEffectType.ShieldActivation);

    private void PlayParry(Vector3 pos) => PlayEffect(SpecialEffectType.Parry, pos);

    private void PlayOrStopHigSpeed(bool state) => PlayOrStopEffect(SpecialEffectType.HighSpeed, state);

    private void UpdateHighSpeed() => UpdateEffectTransform(SpecialEffectType.HighSpeed);

    private void PlayOrStopChargeTurbo(bool state) => PlayOrStopEffect(SpecialEffectType.ChargeTurbo, state);

    private void PlayOrStopTurbo(bool state) => PlayOrStopEffect(SpecialEffectType.Turbo, state);

    private void UpdateTurbo() => UpdateEffectTransform(SpecialEffectType.Turbo);

    private void PlayOrStopBraking(bool state) => PlayOrStopEffect(SpecialEffectType.Braking, state);

    private void UpdateBraking() => UpdateEffectTransform(SpecialEffectType.Braking);

    private void PlayShieldBlocked(Vector3 pos) => PlayEffect(SpecialEffectType.ShieldBlock, pos);

    private void PlayDeath() => PlayEffect(SpecialEffectType.Death);

    private void PlaySpawn() => PlayEffect(SpecialEffectType.Spawn);

    private void PlayCollShieldShield(Vector3 pos) => PlayEffect(SpecialEffectType.CollShieldShield, pos);

    private void PlayCollShieldWall(Vector3 pos) => PlayEffect(SpecialEffectType.CollShieldWall, pos);

    private void PlayCollBallWall(Vector3 pos) => PlayEffect(SpecialEffectType.CollBallWall, pos);

    private void PlayCollBladeWall(Vector3 pos) => PlayEffect(SpecialEffectType.CollBladeWall, pos);

    private void PlayEffect(SpecialEffectType type) {
        if (!effectsPreset.ContainsKey(type)) return;
        ParticleSystem effect = effectsPreset[type];
        effect.transform.position = transform.position;
        effect.Play();
    }

    private void PlayEffect(SpecialEffectType type, Vector3 pos) {
        if (!effectsPreset.ContainsKey(type)) return;
        ParticleSystem effect = effectsPreset[type];
        effect.transform.position = pos;
        if (!effect.isPlaying) effect.Play();
    }

    private void PlayOrStopEffect(SpecialEffectType type, bool state) {
        if (!effectsPreset.ContainsKey(type)) return;
        effectsPreset[type].transform.position = transform.position;
        if (state) effectsPreset[type].Play();
        else effectsPreset[type].Stop();
    }

    private void AddParticleSystem(SpecialEffectType type, string uniqID) {
        if (effectsPreset.ContainsKey(type)) return;
        EffectSO particlePrefab = effects.list.Where(ef => ef.type == type).Where(ef => ef.uniqID == uniqID).FirstOrDefault();
        if (particlePrefab == null) {
            Debug.Log("Effect " + type.ToString() + " " + uniqID + " is undefined");
            return;
        }
        ParticleSystem effect = Instantiate(particlePrefab.particle);
        effect.Stop();
        effectsPreset.Add(type, effect);
    }

    private void UpdateEffectTransform(SpecialEffectType type) {
        if (!effectsPreset.ContainsKey(type)) return;
        ParticleSystem effect = effectsPreset[type];
        effect.transform.position = transform.position;
        effect.transform.LookAt(transform.position - _rigid.velocity.normalized);
    }

    private void OnDestroy() {
        foreach(var effect in effectsPreset) {
            effect.Value?.Stop();
        }
    }
}
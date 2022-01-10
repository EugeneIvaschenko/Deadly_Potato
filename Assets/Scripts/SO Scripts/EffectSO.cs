using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEffect", menuName = "ScriptableObjects/EffectSO", order = 3)]
public class EffectSO : ScriptableObject {
    public ParticleSystem particle;
    public SpecialEffectType type;
    public string displayName;
    public string uniqID;
}
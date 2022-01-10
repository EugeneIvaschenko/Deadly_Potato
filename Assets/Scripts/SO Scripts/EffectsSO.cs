using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEffectList", menuName = "ScriptableObjects/EffectList", order = 4)]
public class EffectsSO : ScriptableObject {
    public List<EffectSO> list;
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkinsData", menuName = "ScriptableObjects/SkinsSO", order = 1)]
public class SkinSO : ScriptableObject {
    public Material materials;
    public string displayName;
    public string uniqID;
}
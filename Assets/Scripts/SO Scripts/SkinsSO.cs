using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkinList", menuName = "ScriptableObjects/SkinsList", order = 2)]
public class SkinsSO : ScriptableObject {
    public List<SkinSO> skinList;
}
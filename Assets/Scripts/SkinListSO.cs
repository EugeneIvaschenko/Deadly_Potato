using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkinList", menuName = "ScriptableObjects/SkinsList", order = 2)]
public class SkinListSO : ScriptableObject {
    public List<SkinSO> skinList;
}
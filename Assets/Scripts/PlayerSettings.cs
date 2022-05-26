using System;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerSettings {
    public static string playerNickName = "";
    public static string selectedSkinID = "default";

    private static Dictionary<SpecialEffectType, string> _VFXPlayerCustomizations = new Dictionary<SpecialEffectType, string>();
    public static Dictionary<SpecialEffectType, string> VFXPlayerCustomizations {
        get {
            foreach (SpecialEffectType effectType in Enum.GetValues(typeof(SpecialEffectType))){
                if(!_VFXPlayerCustomizations.ContainsKey(effectType)) _VFXPlayerCustomizations.Add(effectType, "default");
            }
            return _VFXPlayerCustomizations;
        }
        set { _VFXPlayerCustomizations = value; }
    }
}
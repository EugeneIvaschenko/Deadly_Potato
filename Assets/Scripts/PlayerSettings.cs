using System;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerSettings {
    public static string playerNickName = "";
    public static string selectedSkinID = "default";

    //VFX
    public static string turboID = "default";
    public static string shieldBlockedID = "default";
    public static string deathID = "default";
    public static string spawnID = "default";
    public static string collShieldShieldID = "default";
    public static string collBladeWallID = "default";
    public static string collBallWallID = "default";
    public static string collShieldWallID = "default";
    public static string brakingID = "default";
    public static string chargeTurboID = "default";
    public static string highSpeedID = "default";
    public static string parryID = "default";
    public static string shieldActivationID = "default";
    public static string collBallBallID = "default";

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
using System.Runtime.InteropServices;

public class PluginsUtility {
    [DllImport("__Internal")] private static extern bool IsMobile();
    public static bool IsMobileWebGL() {
#if !UNITY_EDITOR && UNITY_WEBGL
             return IsMobile();
#endif
        return false;
    }
}
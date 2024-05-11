using UnityEngine;

namespace CWVR.Assets;

internal static class AssetManager
{
    private static AssetBundle assetBundle;

    public static GameObject Keyboard;
    public static GameObject CaptchaKeyboard;
    public static GameObject VRSettingsTab;
    public static GameObject BooleanSettingCell;
    public static GameObject EnumSettingCell;
    public static GameObject SliderSettingCell;
    public static GameObject ControlSettingCell;
    
    public static Material WhiteMat;

    public static bool LoadAssets()
    {
        assetBundle = AssetBundle.LoadFromMemory(Properties.Resources.contentwarningvr);
        
        if (assetBundle == null)
        {
            Logger.LogError("Failed to load asset bundle!");
            return false;
        }

        Keyboard = assetBundle.LoadAsset<GameObject>("NonNativeKeyboard");
        CaptchaKeyboard = assetBundle.LoadAsset<GameObject>("CaptchaKeyboard");
        VRSettingsTab = assetBundle.LoadAsset<GameObject>("VRSettingsTab");
        BooleanSettingCell = assetBundle.LoadAsset<GameObject>("BooleanSettingCell");
        EnumSettingCell = assetBundle.LoadAsset<GameObject>("EnumSettingCell");
        SliderSettingCell = assetBundle.LoadAsset<GameObject>("SliderSettingCell");
        ControlSettingCell = assetBundle.LoadAsset<GameObject>("ControlSettingCell");
        
        WhiteMat = assetBundle.LoadAsset<Material>("White");
        
        return true;
    }

    public static T Load<T>(string name)
        where T : Object
    {
        return assetBundle.LoadAsset<T>(name);
    }
}
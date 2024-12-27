using System.IO;
using System.Reflection;
using CWVR.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CWVR.Assets;

internal static class AssetManager
{
    private static AssetBundle assetBundle;

    public static InputActionAsset InputActions;
    public static InputActionAsset DefaultXRActions;

    public static GameObject Keyboard;
    public static GameObject CaptchaKeyboard;
    public static GameObject BooleanSettingCell;
    public static GameObject EnumSettingCell;
    public static GameObject SliderSettingCell;
    public static GameObject ControlSettingCell;
    public static GameObject ControlSettingHeaderCell;

    public static RemappableControls RemappableControls;
    
    public static Material WhiteMat;

    public static bool LoadAssets()
    {
        // TODO: This might break lol
        assetBundle = AssetBundle.LoadFromFile(Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "contentwarningvr"));
        
        if (assetBundle == null)
        {
            Logger.LogError("Failed to load asset bundle!");
            return false;
        }

        InputActions = assetBundle.LoadAsset<InputActionAsset>("InputActions");
        DefaultXRActions = assetBundle.LoadAsset<InputActionAsset>("DefaultXRActions");
        
        Keyboard = assetBundle.LoadAsset<GameObject>("NonNativeKeyboard");
        CaptchaKeyboard = assetBundle.LoadAsset<GameObject>("CaptchaKeyboard");
        BooleanSettingCell = assetBundle.LoadAsset<GameObject>("BooleanSettingCell");
        EnumSettingCell = assetBundle.LoadAsset<GameObject>("EnumSettingCell");
        SliderSettingCell = assetBundle.LoadAsset<GameObject>("SliderSettingCell");
        ControlSettingCell = assetBundle.LoadAsset<GameObject>("ControlSettingCell");
        ControlSettingHeaderCell = assetBundle.LoadAsset<GameObject>("ControlSettingHeaderCell");
        
        RemappableControls =
            assetBundle.LoadAsset<GameObject>("Remappable Controls").GetComponent<RemappableControls>();
        
        WhiteMat = assetBundle.LoadAsset<Material>("White");

        if (RemappableControls == null || RemappableControls.controls == null)
        {
            Logger.LogError(
                "Unity failed to deserialize some assets. Are you missing the FixPluginTypesSerialization mod?");
            return false;
        }
        
        return true;
    }
}
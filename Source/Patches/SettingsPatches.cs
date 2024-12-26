using System.Reflection;
using CWVR.UI.Settings;
using HarmonyLib;
using Zorro.Settings;

namespace CWVR.Patches;

/// <summary>
/// Force setting the .Value of a setting to immediately persist these changes.
/// This fixes issues where non-exposed settings must be saved manually every time.
/// </summary>
[CWVRPatch(CWVRPatchTarget.Universal, LoaderTarget.Workshop)]
internal static class ForceApplySettingsPatches
{
    [HarmonyPatch(typeof(BoolSetting), nameof(BoolSetting.Value), MethodType.Setter)]
    [HarmonyPostfix]
    private static void ForceSaveBoolSetting(BoolSetting __instance)
    {
        GameHandler.Instance.SettingsHandler.SaveSetting(__instance);
    }
    
    [HarmonyPatch(typeof(IntSetting), nameof(IntSetting.Value), MethodType.Setter)]
    [HarmonyPostfix]
    private static void ForceSaveIntSetting(IntSetting __instance)
    {
        GameHandler.Instance.SettingsHandler.SaveSetting(__instance);
    }
    
    [HarmonyPatch(typeof(FloatSetting), nameof(FloatSetting.Value), MethodType.Setter)]
    [HarmonyPostfix]
    private static void ForceSaveFloatSetting(FloatSetting __instance)
    {
        GameHandler.Instance.SettingsHandler.SaveSetting(__instance);
    }
    
    [HarmonyPatch(typeof(StringSetting), nameof(StringSetting.Value), MethodType.Setter)]
    [HarmonyPostfix]
    private static void ForceSaveStringSetting(StringSetting __instance)
    {
        GameHandler.Instance.SettingsHandler.SaveSetting(__instance);
    }
}

[CWVRPatch(CWVRPatchTarget.Universal, LoaderTarget.BepInEx)]
internal static class BepInExSettingsPatches
{
    /// <summary>
    /// Remove all the <see cref="ContentWarningSetting" /> settings from this assembly from the game, as we're using BepInEx
    /// </summary>
    [HarmonyPatch(typeof(SettingsHandler), MethodType.Constructor)]
    [HarmonyPostfix]
    private static void RemoveAssemblySettings(SettingsHandler __instance)
    {
        var me = Assembly.GetExecutingAssembly();
        
        __instance.settings.RemoveAll(setting => setting.GetType().Assembly == me);
    }
}
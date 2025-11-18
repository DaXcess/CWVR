using System.Reflection;
using HarmonyLib;
using Zorro.Settings;

namespace CWVR.Patches;

[CWVRPatch(CWVRPatchTarget.Universal)]
internal static class BepInExSettingsPatches
{
    /// <summary>
    /// Remove all the <see cref="ContentWarningSetting" /> settings from this assembly from the game, as we're using BepInEx
    /// </summary>
    [HarmonyPatch(typeof(SettingsHandler), nameof(SettingsHandler.AddSetting))]
    [HarmonyPrefix]
    private static bool RemoveAssemblySettings(Setting setting)
    {
        return setting.GetType().Assembly != Assembly.GetExecutingAssembly();
    }
}
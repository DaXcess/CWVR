using CWVR.Input;
using CWVR.UI.Settings;
using HarmonyLib;
using UnityEngine;

namespace CWVR.Patches.UI;

[CWVRPatch]
internal static class SettingsMenuPatches
{
    /// <summary>
    /// Clean up setting cells before showing new tab
    /// </summary>
    [HarmonyPatch(typeof(SettingsMenu), nameof(SettingsMenu.Show))]
    [HarmonyPrefix]
    private static void BeforeShow()
    {
        RemapManager.Instance.DestroySettings();
    }

    /// <summary>
    /// Show VR controls tab (if selected)
    /// </summary>
    [HarmonyPatch(typeof(SettingsMenu), nameof(SettingsMenu.Show))]
    [HarmonyPostfix]
    private static void OnSettingsMenuShown(SettingsMenu __instance, SettingCategory category)
    {
        if (category != SettingCategory.Controller)
            return;
        
        // Hide layout and sensitivity cells
        Object.Destroy(__instance.m_cells[0].gameObject);
        __instance.m_cells.RemoveAt(0);
        __instance.m_controllerLayout.SetActive(false);
        
        // Create settings
        RemapManager.Instance.DisplaySettings(__instance.m_settingsContainer);
    }
}

[CWVRPatch(CWVRPatchTarget.Universal)]
internal static class UniversalSettingsMenuPatches
{
    /// <summary>
    /// Inject the VR settings manager after game startup
    /// </summary>
    // ContentCombiningSystem is our next best entrypoint after RichPresenceHandler.Initialize
    [HarmonyPatch(typeof(ContentCombiningSystem), nameof(ContentCombiningSystem.Init))]
    [HarmonyPostfix]
    private static void OnGameStarted()
    {
        GameHandler.Instance.gameObject.AddComponent<VRSettingsMenu>();
    }
    
    /// <summary>
    /// Clean up setting cells before showing new tab
    /// </summary>
    [HarmonyPatch(typeof(SettingsMenu), nameof(SettingsMenu.Show))]
    [HarmonyPrefix]
    private static void BeforeShow()
    {
        VRSettingsMenu.Instance.DestroySettings();
    }

    /// <summary>
    /// Inject VR settings into tab (if correct tab is selected)
    /// </summary>
    [HarmonyPatch(typeof(SettingsMenu), nameof(SettingsMenu.Show))]
    [HarmonyPostfix]
    private static void OnSettingsMenuShown(SettingsMenu __instance, SettingCategory category)
    {
        if (category != SettingCategory.Mods) 
            return;
        
        VRSettingsMenu.Instance.DisplayVRSettings(__instance.m_settingsContainer);
    }
    
    /// <summary>
    /// Force the `mods` tab to be visible, as it may be hidden if there are no other mods installed (BepInEx only)
    /// </summary>
    [HarmonyPatch(typeof(SettingsMenu), nameof(SettingsMenu.OnEnable))]
    [HarmonyPostfix]
    private static void ForceModTabVisible(SettingsMenu __instance)
    {
        __instance.modsTab.gameObject.SetActive(true);
    }
}
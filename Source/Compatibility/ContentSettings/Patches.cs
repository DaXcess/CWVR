using System.Diagnostics;
using ContentSettings.API;
using ContentSettings.API.Settings.UI;
using CWVR.Patches;
using CWVR.Player;
using HarmonyLib;
using UnityEngine;

namespace CWVR.Compatibility.ContentSettings;

[CWVRPatch(CWVRPatchTarget.Universal, "ContentSettings")]
[HarmonyPatch]
internal static class ContentSettingsPatches
{
    [HarmonyPatch(typeof(SettingsTab), nameof(SettingsTab.Show))]
    [HarmonyPostfix]
    private static void OnShow(SettingsTab __instance)
    {
        var settingsMenu = __instance.GetSettingsMenu();
        
        switch (__instance.Name)
        {
            case "VR SETTINGS":
            {
                foreach (var cell in settingsMenu.m_cells)
                    Object.Destroy(cell.gameObject);
            
                settingsMenu.m_cells.Clear();
            
                Object.FindObjectOfType<UI.Settings.SettingsMenu>().DisplayVRSettings(settingsMenu.m_settingsContainer);
                break;
            }
            case "CONTROLS" when VRSession.InVR:
            {
                foreach (var cell in settingsMenu.m_cells)
                    Object.Destroy(cell.gameObject);
            
                settingsMenu.m_cells.Clear();
            
                Object.FindObjectOfType<UI.Settings.SettingsMenu>().DisplayControlsSettings(settingsMenu.m_settingsContainer);
                break;
            }
        }
    }
    
    [HarmonyPatch(typeof(SettingsLoader), "CreateSettingsMenu")]
    [HarmonyPrefix]
    private static void BeforeCreateSettingsMenu(out bool __state)
    {
        #nullable enable
        var settingsNavigationNullable = (SettingsNavigation?)AccessTools.PropertyGetter(typeof(SettingsLoader), "SettingsNavigation").Invoke(null, []);

        __state = settingsNavigationNullable == null;
    }

    [HarmonyPatch(typeof(SettingsLoader), "CreateSettingsMenu")]
    [HarmonyPostfix]
    private static void OnCreateSettingsMenu(ref bool __state)
    {
        if (!__state)
            return;

        var settingsNavigationNullable =
            (SettingsNavigation?)AccessTools.PropertyGetter(typeof(SettingsLoader), "SettingsNavigation")
                .Invoke(null, []);
        settingsNavigationNullable?.Create("VR SETTINGS");

        if (VRSession.Instance != null && VRSession.InVR)
            settingsNavigationNullable?.transform.parent.parent.parent.gameObject.SetLayerRecursive(6);
    }
}
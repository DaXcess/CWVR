using System.Collections;
using CWVR.Player;
using HarmonyLib;
using UnityEngine;

namespace CWVR.Patches.UI;

[CWVRPatch]
[HarmonyPatch]
internal static class EscapeMenuPatches
{
    /// <summary>
    /// Detect and handle opening/closing the pause menu using VR controllers instead of Escape
    /// </summary>
    [HarmonyPatch(typeof(EscapeMenu), nameof(EscapeMenu.LateUpdate))]
    [HarmonyPrefix]
    private static bool OnLateUpdate(EscapeMenu __instance)
    {
        if (!VRSession.Instance.Controls.Menu.PressedDown())
            return false;

        __instance.Toggle();
        if (__instance.Open)
            VRSession.Instance.HUD.PauseMenu.OnOpen();
        else
            VRSession.Instance.HUD.PauseMenu.OnClose();

        return false;
    }

    /// <summary>
    /// Make sure that the settings page is also rendered on top and interactable
    /// </summary>
    [HarmonyPatch(typeof(SettingsCell), nameof(SettingsCell.Setup))]
    [HarmonyPostfix]
    private static void OnSettingsCellSetup(SettingsCell __instance)
    {
        if (VRSession.Instance is null)
            return;

        __instance.gameObject.SetLayerRecursive(6);
    }
}
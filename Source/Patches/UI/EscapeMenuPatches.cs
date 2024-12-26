using CWVR.Input;
using CWVR.Player;
using HarmonyLib;

namespace CWVR.Patches.UI;

[CWVRPatch]
internal static class EscapeMenuPatches
{
    /// <summary>
    /// Detect and handle opening/closing the pause menu using VR controllers instead of Escape
    /// </summary>
    [HarmonyPatch(typeof(EscapeMenu), nameof(EscapeMenu.LateUpdate))]
    [HarmonyPrefix]
    private static bool OnLateUpdate(EscapeMenu __instance)
    {
        if (!Actions.Instance["OpenMenu"].WasPressedThisFrame())
            return false;

        __instance.Toggle();
        if (__instance.Open)
            VRSession.Instance.HUD.PauseMenu.OnOpen();
        else
            VRSession.Instance.HUD.PauseMenu.OnClose();

        return false;
    }

    /// <summary>
    /// Detect when the pause menu was closed via the RESUME button
    /// </summary>
    [HarmonyPatch(typeof(EscapeMenuMainPage), nameof(EscapeMenuMainPage.OnResumeButtonClicked))]
    [HarmonyPostfix]
    private static void OnClosePauseMenu()
    {
        VRSession.Instance.HUD.PauseMenu.OnClose();
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

    /// <summary>
    /// Make sure the player list is interactable in VR
    /// </summary>
    [HarmonyPatch(typeof(EscapePlayerCellUI), nameof(EscapePlayerCellUI.Setup))]
    [HarmonyPostfix]
    private static void OnSetupPlayerSlider(EscapePlayerCellUI __instance)
    {
        __instance.gameObject.SetLayerRecursive(6);
    }
}
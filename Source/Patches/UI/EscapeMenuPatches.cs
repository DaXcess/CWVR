using CWVR.Player;
using HarmonyLib;
using UnityEngine.InputSystem;
using Zorro.ControllerSupport;

namespace CWVR.Patches.UI;

[CWVRPatch]
internal static class EscapeMenuPatches
{
    /// <summary>
    /// Re-assign the open menu button to our VR remapped action
    /// </summary>
    [HarmonyPatch(typeof(EscapeMenu), nameof(EscapeMenu.OnEnable))]
    [HarmonyPrefix]
    private static void OnEscapeMenuCreated(EscapeMenu __instance)
    {
        var playerInput = InputHandler.Instance.m_playerInput;

        __instance.OpenMenuAction = InputActionReference.Create(playerInput.actions["Player/OpenMenu"]);
    }

    /// <summary>
    /// Detect and handle opening/closing the pause menu using VR controllers instead of Escape
    /// </summary>
    [HarmonyPatch(typeof(EscapeMenu), nameof(EscapeMenu.Toggle), [])]
    [HarmonyPostfix]
    private static void OnToggle(EscapeMenu __instance)
    {
        if (__instance.Open)
            VRSession.Instance.HUD.PauseMenu.OnOpen();
        else
            VRSession.Instance.HUD.PauseMenu.OnClose();
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
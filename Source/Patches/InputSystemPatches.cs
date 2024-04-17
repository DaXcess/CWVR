using HarmonyLib;
using UnityEngine.InputSystem;

namespace CWVR.Patches;

[CWVRPatch(CWVRPatchTarget.Universal)]
[HarmonyPatch]
internal static class InputSystemPatches
{
    /// <summary>
    /// This patch prevents the new InputSystem from detecting devices, removing some benign errors from the logs
    /// </summary>
    [HarmonyPatch(typeof(InputManager), nameof(InputManager.OnNativeDeviceDiscovered))]
    [HarmonyPrefix]
    private static bool DisableInputSystem()
    {
        return false;
    }
}
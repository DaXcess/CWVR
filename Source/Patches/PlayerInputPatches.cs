using CWVR.Player;
using HarmonyLib;

namespace CWVR.Patches;

[CWVRPatch]
[HarmonyPatch]
internal static class PlayerInputPatches
{
    /// <summary>
    /// Sample player input from the VR controllers instead of M+K
    /// </summary>
    [HarmonyPatch(typeof(global::Player.PlayerInput), nameof(global::Player.PlayerInput.SampeInput))]
    [HarmonyPrefix]
    private static bool SamplePlayerInput(global::Player.PlayerInput __instance, global::Player.PlayerData data,
        global::Player player)
    {
        // Allow M+K while VR is being initialized
        if (VRSession.Instance is null)
            return true;
        
        VRSession.Instance.Controls.SampleInput(__instance, data, player);
        
        return false;
    }
}
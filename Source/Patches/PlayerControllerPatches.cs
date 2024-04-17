using CWVR.Player;
using HarmonyLib;
using UnityEngine;

namespace CWVR.Patches;

[CWVRPatch]
[HarmonyPatch]
internal static class PlayerControllerPatches
{
    [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.SetRotations))]
    [HarmonyPrefix]
    private static bool OnSetRotations(PlayerController __instance)
    {
        // Skip if not local player or VR has not yet been initialized
        if (!VRSession.Instance || !__instance.player.IsLocal)
            return true;

        // Don't rotate player if in bed or dead
        if (__instance.player.data.dead || __instance.player.data.bed is not null)
            return true;
        
        var camera = VRSession.Instance.MainCamera.transform;
        
        __instance.player.data.lookDirection = camera.forward;
        __instance.player.data.lookDirectionRight = camera.right;
        __instance.player.data.lookDirectionUp = camera.up;
        
        return false;
    }
}
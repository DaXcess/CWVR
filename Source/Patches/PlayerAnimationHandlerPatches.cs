using CWVR.Player;
using HarmonyLib;
using UnityEngine;

namespace CWVR.Patches;

[CWVRPatch]
[HarmonyPatch]
internal static class PlayerAnimationHandlerPatches
{
    // TODO: Does this even do anything?
    [HarmonyPatch(typeof(PlayerAnimationHandler), nameof(PlayerAnimationHandler.SetAnimatorValues))]
    [HarmonyPostfix]
    private static void OnSetAnimatorValues(PlayerAnimationHandler __instance)
    {
        if (VRSession.Instance is null || VRSession.Instance.LocalPlayer is null)
            return;

        // This doesn't work
        // var offset = VRSession.Instance.MainCamera.transform.position.y - VRSession.Instance.LocalPlayer.Rig.DesiredCameraPosition.y;
        // var stance = Mathf.Clamp(1 - offset, 0, 1);
        //
        // __instance.animator.SetFloat("StanceType", stance);
    }
}
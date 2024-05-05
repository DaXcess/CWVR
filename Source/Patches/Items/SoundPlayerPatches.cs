using CWVR.Player;
using HarmonyLib;
using UnityEngine;

namespace CWVR.Patches.Items;

[CWVRPatch]
[HarmonyPatch]
internal static class SoundPlayerPatches
{
    /// <summary>
    /// Make the sound player scrollable in VR
    /// </summary>
    [HarmonyPatch(typeof(SoundPlayerItem), nameof(SoundPlayerItem.Update))]
    [HarmonyPrefix]
    private static void BeforeUpdate(SoundPlayerItem __instance)
    {
        if (!VRSession.Instance)
            return;

        if (__instance.counter <= 0.25f || !__instance.isHeldByMe || global::Player.localPlayer.HasLockedInput() ||
            !GlobalInputHandler.CanTakeInput())
            return;

        var controls = VRSession.Instance.Controls;
        
        switch (controls.ZoomIn.PressedDown(), controls.ZoomOut.PressedDown())
        {
            case (true, _):
                __instance.selectionEntry.selectedValue--;
                __instance.selectionEntry.selectedValue = Mathf.Clamp(__instance.selectionEntry.selectedValue, 0,
                    __instance.selectionEntry.maxValue - 1);
                __instance.selectionEntry.SetDirty();
                break;
                
            case (_, true):
                __instance.selectionEntry.selectedValue++;
                __instance.selectionEntry.selectedValue = Mathf.Clamp(__instance.selectionEntry.selectedValue, 0,
                    __instance.selectionEntry.maxValue - 1);
                __instance.selectionEntry.SetDirty();
                break;
        };
    }
}
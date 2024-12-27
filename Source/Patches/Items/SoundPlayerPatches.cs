using CWVR.Input;
using CWVR.Player;
using HarmonyLib;
using UnityEngine;

namespace CWVR.Patches.Items;

[CWVRPatch]
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
        
        if (Plugin.Config.InteractToZoom.Value && !Actions.Instance["Interact"].IsPressed())
            return;

        switch (Actions.Instance["Zoom - Swap"].ReadFloatThisFrame())
        {
            case > 0:
                __instance.selectionEntry.selectedValue--;
                __instance.selectionEntry.selectedValue = Mathf.Clamp(__instance.selectionEntry.selectedValue, 0,
                    __instance.selectionEntry.maxValue - 1);
                __instance.selectionEntry.SetDirty();
                break;
                
            case < 0:
                __instance.selectionEntry.selectedValue++;
                __instance.selectionEntry.selectedValue = Mathf.Clamp(__instance.selectionEntry.selectedValue, 0,
                    __instance.selectionEntry.maxValue - 1);
                __instance.selectionEntry.SetDirty();
                break;
        };
    }
}
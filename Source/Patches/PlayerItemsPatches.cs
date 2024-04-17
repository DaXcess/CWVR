using CWVR.Player;
using HarmonyLib;
using UnityEngine;

namespace CWVR.Patches;

[CWVRPatch]
[HarmonyPatch]
internal static class PlayerItemsPatches
{
    // TODO: Check if this can be determined dynamically
    private const int TOTAL_SLOTS = 4;

    /// <summary>
    /// Disable all colliders on held items so that the hand may still rotate freely when equipped
    /// </summary>
    [HarmonyPatch(typeof(PlayerItems), nameof(PlayerItems.Equip))]
    [HarmonyPostfix]
    private static void OnEquip(PlayerItems __instance)
    {
        var playerColliders = global::Player.localPlayer.GetComponentsInChildren<Collider>();
        var itemColliders = __instance.player.data.currentItem.gameObject.GetComponentsInChildren<Collider>();
        
        playerColliders.Do(p => itemColliders.Do(i => Physics.IgnoreCollision(p, i)));
    }

    /// <summary>
    /// Be able to cycle through the inventory by holding interact and using the zoom joystick up/down
    /// </summary>
    [HarmonyPatch(typeof(PlayerItems), nameof(PlayerItems.Update))]
    [HarmonyPrefix]
    private static void BeforeUpdate(PlayerItems __instance)
    {
        if (!__instance.player.data.isLocal || !__instance.player.data.physicsAreReady ||
            __instance.player.HasLockedInput())
            return;

        var controls = VRSession.Instance.Controls;

        if (!controls.Interact.Pressed())
            return;
        
        if (controls.ZoomIn.PressedDown())
            __instance.player.data.selectedItemSlot = (__instance.player.data.selectedItemSlot + 1 + TOTAL_SLOTS) % TOTAL_SLOTS;
        else if (controls.ZoomOut.PressedDown())
            __instance.player.data.selectedItemSlot = (__instance.player.data.selectedItemSlot - 1 + TOTAL_SLOTS) % TOTAL_SLOTS;
    }
}

[CWVRPatch(CWVRPatchTarget.Universal)]
[HarmonyPatch]
internal static class UniversalPlayerItemsPatches
{
    /// <summary>
    /// Prevent the hands IK from being modified by items for VR players
    /// </summary>
    [HarmonyPatch(typeof(PlayerItems), nameof(PlayerItems.FixedUpdate))]
    [HarmonyPrefix]
    private static bool BeforeFixedUpdate(PlayerItems __instance)
    {
        // Allow while loading VR session
        if (!VRSession.Instance)
            return true;
        
        return !VRSession.InVR && !VRSession.Instance.NetworkManager.InVR(__instance.player);
    }
}
using CWVR.Input;
using HarmonyLib;
using UnityEngine;

namespace CWVR.Patches;

[CWVRPatch]
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

        switch (Plugin.Config.InteractToZoom.Value)
        {
            case true when Actions.Instance["Interact"].IsPressed():
            case false when !Actions.Instance["Interact"].IsPressed():
                return;
        }

        switch (Actions.Instance["Zoom - Swap"].ReadFloatThisFrame())
        {
            case < 0:
                __instance.player.data.selectedItemSlot =
                    (__instance.player.data.selectedItemSlot + 0 + TOTAL_SLOTS) % TOTAL_SLOTS;
                break;

            case > 0:
                __instance.player.data.selectedItemSlot =
                    (__instance.player.data.selectedItemSlot + 1 + TOTAL_SLOTS) % TOTAL_SLOTS;
                break;
        }
    }
}

[CWVRPatch(CWVRPatchTarget.Universal)]
internal static class UniversalPlayerItemsPatches
{
    /// <summary>
    /// Prevent the hands IK from being modified by items for VR players
    /// </summary>
    [HarmonyPatch(typeof(PlayerItems), nameof(PlayerItems.FixedUpdate))]
    [HarmonyPrefix]
    private static bool BeforeFixedUpdate(PlayerItems __instance)
    {
        // Don't care about flatscreen
        if (!__instance.player.InVR())
            return true;

        // We're done if we're holding nothing
        if (!__instance.player.data.currentItem)
            return false;

        // Force update item transforms to be the same as the hand (instead of having a hardcoded value like in flatscreen)
        var itemTarget = __instance.player.data.currentItem.itemBody.animationTarget.transform;
        var handTarget = __instance.player.refs.ragdoll.GetBodypart(BodypartType.Hand_R).animationTarget.transform;

        itemTarget.position = handTarget.position;
        itemTarget.rotation = handTarget.rotation;

        return false;
    }
}
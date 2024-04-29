using CWVR.Player;
using HarmonyLib;

namespace CWVR.Patches;

[CWVRPatch]
[HarmonyPatch]
internal static class BedPatches
{
    [HarmonyPatch(typeof(Bed), nameof(Bed.RPCA_AcceptSleep))]
    [HarmonyPostfix]
    private static void OnSleep(int playerID)
    {
        var player = PlayerHandler.instance.TryGetPlayerFromViewID(playerID);
        if (player != global::Player.localPlayer)
            return;

        var rig = VRSession.Instance.LocalPlayer.Rig;
        
        rig.ResetHeight(0);
    }
    
    /// <summary>
    /// Allow the use of VR controllers to leave the bed
    /// </summary>
    [HarmonyPatch(typeof(Bed), nameof(Bed.Update))]
    [HarmonyPostfix]
    private static void OnUpdate(Bed __instance)
    {
        if (__instance.playerInBed is null || global::Player.localPlayer != __instance.playerInBed)
            return;

        var controls = VRSession.Instance.Controls;
        if (controls.Jump.PressedDown() || controls.Interact.PressedDown() || controls.Menu.PressedDown() ||
            controls.Sprint.PressedDown())
        {
            __instance.LeaveBed();
            VRSession.Instance.LocalPlayer.Rig.ResetHeight();
        }
    }
}
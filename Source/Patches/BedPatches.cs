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
        
        VRSession.Instance.LocalPlayer.Rig.ResetHeight(0);
    }
    
    /// <summary>
    /// Reset height when leaving the bed
    /// </summary>
    [HarmonyPatch(typeof(Bed), nameof(Bed.LeaveBed))]
    [HarmonyPostfix]
    private static void OnLeaveBed()
    {
        VRSession.Instance.LocalPlayer.Rig.ResetHeight();
    }
}
using CWVR.Patches;
using HarmonyLib;

namespace CWVR.Experiments;

#if DEBUG
[CWVRPatch(CWVRPatchTarget.Universal)]
[HarmonyPatch]
internal static class Patches
{
    /// <summary>
    /// Infinite money glitch :OO!@#!@)?!?@#
    /// </summary>
    [HarmonyPatch(typeof(RoomStatsHolder), nameof(RoomStatsHolder.CanAfford))]
    [HarmonyPrefix]
    private static bool BeforeCanAfford(ref bool __result)
    {
        __result = true;
        return false;
    }
}
#endif
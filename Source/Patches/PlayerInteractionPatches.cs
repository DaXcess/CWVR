using System.Collections.Generic;
using System.Reflection.Emit;
using CWVR.Player;
using HarmonyLib;

namespace CWVR.Patches;

[CWVRPatch]
internal static class PlayerInteractionPatches
{
    /// <summary>
    /// Make sure the interactions are based on the primary interactor (controller) instead of the head (main camera)
    /// </summary>
    [HarmonyPatch(typeof(PlayerInteraction), nameof(PlayerInteraction.FindBestInteractible))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> HandFindBestInteractablePatch(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .Start()
            .RemoveInstructions(14)
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(VRSession), nameof(VRSession.Instance))),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(VRSession), nameof(VRSession.LocalPlayer))),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.PropertyGetter(typeof(VRPlayer), nameof(VRPlayer.PrimaryInteractor))),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Interactor), nameof(Interactor.GetRaycastHits)))
            )
            .InstructionEnumeration();
    }
}
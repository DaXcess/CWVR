using System.Collections.Generic;
using System.Reflection.Emit;
using CWVR.Player;
using HarmonyLib;
using UnityEngine;
using static HarmonyLib.AccessTools;

namespace CWVR.Patches.UI;

[CWVRPatch]
internal static class CursorVisualPatches
{
    /// <summary>
    /// Makes the cursor on the TV screen and shop follow the hand movements instead of the head movement
    /// </summary>
    [HarmonyPatch(typeof(CursorVisual), nameof(CursorVisual.LateUpdate))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> CursorFollowInteractor(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .Advance(1)
            .RemoveInstructions(8)
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(VRSession), nameof(VRSession.Instance))),
                new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(VRSession), nameof(VRSession.LocalPlayer))),
                new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(VRPlayer), nameof(VRPlayer.PrimaryInteractor))),
                new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(Component), nameof(Component.transform))),
                new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(Transform), nameof(Transform.position))),
                new CodeInstruction(OpCodes.Stloc_0),
                
                new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(VRSession), nameof(VRSession.Instance))),
                new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(VRSession), nameof(VRSession.LocalPlayer))),
                new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(VRPlayer), nameof(VRPlayer.PrimaryInteractor))),
                new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(Component), nameof(Component.transform))),
                new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(Transform), nameof(Transform.up))),
                new CodeInstruction(OpCodes.Stloc_1)
            )
            .InstructionEnumeration();
    }
}
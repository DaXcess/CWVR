using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace CWVR.Patches.Items;

[CWVRPatch(CWVRPatchTarget.Universal)]
[HarmonyPatch]
internal static class DefibPatches
{
    [HarmonyPatch(typeof(Defib), nameof(Defib.Update))]
    [HarmonyDebug]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> FixDefibControllerInput(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false, [new CodeMatch(OpCodes.Ldc_I4_0)])
            .RemoveInstructions(2)
            .InsertAndAdvance([
                new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(global::Player), nameof(global::Player.localPlayer))),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(global::Player), nameof(global::Player.input))),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(global::Player.PlayerInput), nameof(global::Player.PlayerInput.clickWasPressed)))
            ])
            .InstructionEnumeration();
    }
}
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using static HarmonyLib.AccessTools;

namespace CWVR.Patches.Items;

[CWVRPatch]
[HarmonyPatch]
internal static class DefibPatches
{
    [HarmonyPatch(typeof(Defib), nameof(Defib.Update))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> FixDefibControllerInput(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false, [new CodeMatch(OpCodes.Ldc_I4_0)])
            .RemoveInstructions(2)
            .InsertAndAdvance([
                new CodeInstruction(OpCodes.Ldsfld, Field(typeof(global::Player), nameof(global::Player.localPlayer))),
                new CodeInstruction(OpCodes.Ldfld, Field(typeof(global::Player), nameof(global::Player.input))),
                new CodeInstruction(OpCodes.Ldfld,
                    Field(typeof(global::Player.PlayerInput), nameof(global::Player.PlayerInput.clickWasPressed)))
            ])
            .InstructionEnumeration();
    }
}
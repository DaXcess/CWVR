using System.Collections.Generic;
using System.IO;
using System.Reflection.Emit;
using HarmonyLib;

namespace CWVR.Patches;

[CWVRPatch(CWVRPatchTarget.Universal)]
[HarmonyPatch]
internal static class GameHandlerPatches
{
    /// <summary>
    /// Patches the game's plugin checker so that the RuntimeDeps don't count towards the loaded plugins
    /// </summary>
    [HarmonyPatch(typeof(GameHandler), nameof(GameHandler.CheckPlugins))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> CheckPluginsNoRuntimeDeps(IEnumerable<CodeInstruction> instructions,
        ILGenerator generator)
    {
        return new CodeMatcher(instructions, generator)
            .MatchForward(false, new CodeMatch(OpCodes.Initobj, typeof(GameHandler.PluginInfo)))
            .Advance(-1)
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldloc_S, (byte)15),
                new CodeInstruction(OpCodes.Callvirt,
                    AccessTools.PropertyGetter(typeof(FileInfo), nameof(FileInfo.DirectoryName))),
                new CodeInstruction(OpCodes.Ldstr, "\\RuntimeDeps"),
                new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(string), nameof(string.EndsWith), [typeof(string)]))
            )
            .InsertBranchAndAdvance(OpCodes.Brtrue, 192)
            .InstructionEnumeration();
    }
}
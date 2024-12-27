using System;
using System.Collections.Generic;
using HarmonyLib;

namespace CWVR.Patches;

/// <summary>
/// A system of patches to allow for executing actions during the GameHandler update loop
/// This allows enqueueing actions during BepInEx initialization
/// </summary>
[CWVRPatch(CWVRPatchTarget.Universal)]
internal static class GameHandlerPatches
{
    private static readonly Queue<Action> actions = [];
    
    [HarmonyPatch(typeof(GameHandler), nameof(GameHandler.LateUpdate))]
    [HarmonyPostfix]
    private static void ExecuteEnqueuedActions()
    {
        if (actions.TryDequeue(out var action))
            action();
    }

    public static void EnqueueAction(Action action)
    {
        actions.Enqueue(action);
    }
}
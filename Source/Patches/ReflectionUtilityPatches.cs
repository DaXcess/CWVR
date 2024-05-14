using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Zorro.Core;
using Zorro.Core.CLI;
using static HarmonyLib.AccessTools;

namespace CWVR.Patches;

[CWVRPatch(CWVRPatchTarget.Universal)]
[HarmonyPatch]
internal static class ReflectionUtilityPatches
{
    /// <summary>
    /// Hook the `GetMethodsWithAttribute` call in the `Initialize` method to our own, which ignores soft dependency errors
    /// </summary>
    [HarmonyPatch(typeof(ConsoleHandler), nameof(ConsoleHandler.Initialize))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> FixSoftDependencies(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
            [
                new CodeMatch(OpCodes.Call,
                    Method(typeof(ReflectionUtility), nameof(ReflectionUtility.GetMethodsWithAttribute))
                        .MakeGenericMethod([typeof(ConsoleCommandAttribute)]))
            ])
            .SetOperandAndAdvance(Method(typeof(ReflectionUtilityPatches), nameof(GetMethodsWithAttribute))
                .MakeGenericMethod([typeof(ConsoleCommandAttribute)]))
            .InstructionEnumeration();
    }

    private static ValueTuple<MethodInfo, Attribute>[] GetMethodsWithAttribute<T>() where T : Attribute
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var list = new List<ValueTuple<MethodInfo, Attribute>>();
        foreach (var asm in assemblies)
        {
            var types = asm.GetTypes();
            foreach (var t in types)
            {
                foreach (var methodInfo in t.GetMethods())
                {
                    try
                    {
                        var customAttribute = methodInfo.GetCustomAttribute<T>();
                        if (customAttribute != null)
                        {
                            list.Add(new ValueTuple<MethodInfo, Attribute>(methodInfo, customAttribute));
                        }
                    }
                    catch (Exception _)
                    {
                        // ignored
                    }
                }
            }
        }

        return list.ToArray();
    }
}
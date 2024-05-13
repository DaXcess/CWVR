using System;
using System.Reflection;
using HarmonyLib;

namespace CWVR.Patches;

internal static class HarmonyPatcher
{
    private static readonly Harmony vrPatcher = new("io.daxcess.cwvr");
    private static readonly Harmony universalPatcher = new("io.daxcess.cwvr-universal");

    public static void PatchUniversal()
    {
        universalPatcher.Patch(CWVRPatchTarget.Universal);
    }

    public static void PatchVR()
    {
        vrPatcher.Patch(CWVRPatchTarget.VROnly);
    }

    private static void Patch(this Harmony patcher, CWVRPatchTarget target)
    {
        AccessTools.GetTypesFromAssembly(Assembly.GetExecutingAssembly()).Do((type) =>
        {
            try
            {
                var attribute = (CWVRPatchAttribute)Attribute.GetCustomAttribute(type, typeof(CWVRPatchAttribute));
                if (attribute is null)
                    return;

                if (attribute.Dependency != null && !Plugin.Compatibility.IsLoaded(attribute.Dependency))
                    return;

                if (attribute.Target == target)
                    patcher.CreateClassProcessor(type).Patch();
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to apply patches from {type}: {e.Message}");
            }
        });
    }
}

internal class CWVRPatchAttribute(CWVRPatchTarget target = CWVRPatchTarget.VROnly, string dependency = null) : Attribute
{
    public CWVRPatchTarget Target { get; } = target;
    public string Dependency { get; } = dependency;
}

internal enum CWVRPatchTarget
{
    Universal,
    VROnly
}
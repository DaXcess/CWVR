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

    public static void UnpatchVR()
    {
        vrPatcher.UnpatchSelf();
    }

    private static void Patch(this Harmony patcher, CWVRPatchTarget target)
    {
        AccessTools.GetTypesFromAssembly(Assembly.GetExecutingAssembly()).Do(type =>
        {
            try
            {
                var attribute = (CWVRPatchAttribute)Attribute.GetCustomAttribute(type, typeof(CWVRPatchAttribute));
                if (attribute is null)
                    return;

                if (attribute.Target != target)
                    return;

                Logger.LogDebug($"Applying patches from: {type.FullName}");

                patcher.CreateClassProcessor(type, true).Patch();
            }
            catch (Exception e)
            {
                Logger.LogError($"Failed to apply patches from {type}: {e.Message}, {e.InnerException}");
            }
        });
    }
}

internal class CWVRPatchAttribute(
    CWVRPatchTarget target = CWVRPatchTarget.VROnly) : Attribute
{
    public CWVRPatchTarget Target { get; } = target;
}

internal enum CWVRPatchTarget
{
    Universal,
    VROnly
}
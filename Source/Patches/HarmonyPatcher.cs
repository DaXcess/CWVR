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

                if (!(attribute.Loader == LoaderTarget.Both ||
                      (attribute.Loader == LoaderTarget.BepInEx && Plugin.Loader == Loader.BepInEx) ||
                      (attribute.Loader == LoaderTarget.Native && Plugin.Loader == Loader.Native)))
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
    CWVRPatchTarget target = CWVRPatchTarget.VROnly,
    LoaderTarget loader = LoaderTarget.Both) : Attribute
{
    public CWVRPatchTarget Target { get; } = target;
    public LoaderTarget Loader { get; } = loader;
}

internal enum CWVRPatchTarget
{
    Universal,
    VROnly
}

internal enum LoaderTarget
{
    BepInEx,
    Native,
    Both
}
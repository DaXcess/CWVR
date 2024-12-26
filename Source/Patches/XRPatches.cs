using HarmonyLib;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.InputSystem.XR;

namespace CWVR.Patches;

[CWVRPatch]
internal static class XRPatches
{
    internal static bool EnableOcclusionMesh = true;
    
    /// <summary>
    /// Funny Non-NVIDIA BepInEx Entrypoint quick fix
    /// </summary>
    [HarmonyPatch(typeof(XRSupport), nameof(XRSupport.Initialize))]
    [HarmonyPrefix]
    private static bool OnBeforeInitialize()
    {
        return false;
    }
    
    /// <summary>
    /// Force occlusion mesh scale (will always be 0 otherwise for some reason)
    /// </summary>
    [HarmonyPatch(typeof(XRSystem), nameof(XRSystem.BuildPass))]
    [HarmonyPostfix]
    private static void OnXRSystemInitialize(ref XRPassCreateInfo __result)
    {
        __result.occlusionMeshScale = EnableOcclusionMesh ? 1f : 0f;
    }
}
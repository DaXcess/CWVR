using System.Collections;
using HarmonyLib;
using HorizonBasedAmbientOcclusion.Universal;
using UnityEngine;

namespace CWVR.Patches.Rendering;

[CWVRPatch]
internal static class PostVolumeHandlerPatches
{
    /// <summary>
    /// Fix double-vision issues when down in the depths
    /// </summary>
    [HarmonyPatch(typeof(PostVolumeHandler), nameof(PostVolumeHandler.Start))]
    [HarmonyPostfix]
    private static void OnEnable(PostVolumeHandler __instance)
    {
        __instance.StartCoroutine(delayedFixPixelNormals(__instance));
    }

    private static IEnumerator delayedFixPixelNormals(PostVolumeHandler pvh)
    {
        yield return new WaitUntil(() => pvh.m_hbao is not null);
        pvh.m_hbao.SetAoPerPixelNormals(HBAO.PerPixelNormals.Camera);
    }
}
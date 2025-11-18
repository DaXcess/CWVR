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

    [HarmonyPatch(typeof(PostVolumeHandler), nameof(PostVolumeHandler.LateUpdate))]
    [HarmonyPostfix]
    private static void OnUpdatePostVolume(PostVolumeHandler __instance)
    {
        __instance.m_edgeDetectionSetting.Value = __instance.m_hbao.active ? 1 : 0;
    }

    private static IEnumerator delayedFixPixelNormals(PostVolumeHandler pvh)
    {
        yield return new WaitUntil(() => pvh.m_hbao is not null);
        pvh.m_hbao.SetAoPerPixelNormals(HBAO.PerPixelNormals.Camera);
    }
}
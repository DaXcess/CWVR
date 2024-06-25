using CWVR.Player;
using HarmonyLib;
using TMPro;
using UnityEngine;

namespace CWVR.Patches.Items;

[CWVRPatch]
[HarmonyPatch]
internal static class VideoCameraPatches
{
    private static TextMeshProUGUI filmText;
    
    /// <summary>
    /// Set up video camera for usage in VR
    /// </summary>
    [HarmonyPatch(typeof(VideoCamera), nameof(VideoCamera.Start))]
    [HarmonyPostfix]
    private static void OnStart(VideoCamera __instance)
    {
        // Prevent the video camera from capturing on-top UI
        __instance.m_camera.cullingMask &= ~(1 << 6);

        // Add text component so that the % film left is visible in VR
        var canvas = __instance.m_cameraUI.transform;
        var filmGroup = new GameObject("Film").AddComponent<CanvasGroup>();
        filmGroup.transform.SetParent(canvas, false);
        filmGroup.transform.localPosition = new Vector3(-250, -400, 0);
        filmGroup.transform.localScale = Vector3.one * 1.5f;
        
        filmText = new GameObject("Text").AddComponent<TextMeshProUGUI>();
        filmText.transform.SetParent(filmGroup.transform, false);

        filmText.text = $"{Mathf.CeilToInt(__instance.m_recorderInfoEntry.GetPercentage() * 100)}%";
    }

    /// <summary>
    /// Zoom the camera using VR controllers
    /// </summary>
    [HarmonyPatch(typeof(VideoCamera), nameof(VideoCamera.Update))]
    [HarmonyPrefix]
    private static void BeforeUpdate(VideoCamera __instance)
    {
        var controls = VRSession.Instance.Controls;
        
        if (!__instance.isHeldByMe || !GlobalInputHandler.CanTakeInput())
            return;
        
        var sign = (controls.ZoomIn.Pressed(), controls.ZoomOut.Pressed()) switch
        {
            (true, _) => 1,
            (_, true) => -1,
            (false, false) => 0,
        };

        if (Plugin.Config.InteractToZoom.Value && !controls.Interact.Pressed())
            sign = 0;

        __instance.m_zoomLevel += Time.deltaTime * sign;
    }

    /// <summary>
    /// Update percentage on the film text
    /// </summary>
    [HarmonyPatch(typeof(VideoCamera), nameof(VideoCamera.Update))]
    [HarmonyPostfix]
    private static void OnUpdate(VideoCamera __instance)
    {
        filmText.text = $"{Mathf.CeilToInt(__instance.m_recorderInfoEntry.GetPercentage() * 100)}%";
    }
}
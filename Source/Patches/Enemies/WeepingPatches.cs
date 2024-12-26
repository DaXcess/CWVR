using System;
using CWVR.Assets;
using CWVR.Player;
using HarmonyLib;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Object = UnityEngine.Object;

namespace CWVR.Patches.Enemies;

[CWVRPatch]
internal static class WeepingPatches
{
    private static NonNativeKeyboard keyboard;
    private static Bot_Weeping bot;

    private static GameObject leftHandInteractor;
    private static GameObject rightHandInteractor;

    [HarmonyPatch(typeof(Bot_Weeping), nameof(Bot_Weeping.RPCA_JoinCaptchaGame))]
    [HarmonyPostfix]
    private static void OnStartCaptchaGame(Bot_Weeping __instance)
    {
        if (!__instance.playerInCaptchaGame.refs.view.IsMine)
            return;

        keyboard = Object.Instantiate(AssetManager.CaptchaKeyboard,
                __instance.captchaGame.terminalCavnas.transform.parent)
            .GetComponent<NonNativeKeyboard>();

        keyboard.SubmitOnEnter = false;
        keyboard.CloseOnEnter = false;
        keyboard.DisableLayouts = false;

        keyboard.transform.localPosition = new Vector3(0, -1, 0);
        keyboard.transform.localEulerAngles = Vector3.zero;
        keyboard.transform.localScale = new Vector3(0.001f, 0.002f, 0.002f);

        keyboard.OnKeyboardValueKeyPressed += OnKeyPressed;
        keyboard.OnClosed += OnKeyboardClosed;

        keyboard.gameObject.SetLayerRecursive(6);
        keyboard.PresentKeyboard();

        if (!leftHandInteractor)
            leftHandInteractor = CreateInteractorController(XRNode.LeftHand);
        else
            leftHandInteractor.SetActive(true);

        if (!rightHandInteractor)
            rightHandInteractor = CreateInteractorController(XRNode.RightHand);
        else
            rightHandInteractor.SetActive(true);

        bot = __instance;
    }

    [HarmonyPatch(typeof(Bot_Weeping), nameof(Bot_Weeping.RPCA_LeaveCaptchaGame))]
    [HarmonyPrefix]
    private static void OnEndGame(Bot_Weeping __instance)
    {
        if (!__instance.playerInCaptchaGame.refs.view.IsMine)
            return;
        
        CleanupKeyboard();
    }

    private static void CleanupKeyboard()
    {
        leftHandInteractor.SetActive(false);
        rightHandInteractor.SetActive(false);
        
        if (!keyboard)
            return;

        keyboard.OnKeyboardValueKeyPressed -= OnKeyPressed;
        keyboard.OnClosed -= OnKeyboardClosed;

        keyboard.enabled = false;
        
        Object.Destroy(keyboard.gameObject);

        keyboard = null;
    }

    private static void OnKeyPressed(KeyboardValueKey obj)
    {
        if (bot == null)
        {
            CleanupKeyboard();
            return;
        }

        var value = keyboard.IsShifted && !string.IsNullOrEmpty(obj.ShiftValue) ? obj.ShiftValue : obj.Value;
        if (value.Length < 1 || value.ToLowerInvariant() is "e" or "w" or "a" or "s" or "d")
            return;
        
        bot.view.RPC("RPCA_InputCharToCaptcha", RpcTarget.All, value[0].ToString());
    }
    
    private static void OnKeyboardClosed(object sender, EventArgs e)
    {
        if (bot && bot.playerInCaptchaGame && bot.playerInCaptchaGame.refs.view.IsMine)
            bot.view.RPC("RPCA_LeaveCaptchaGame", RpcTarget.All);
    }
    
    private static GameObject CreateInteractorController(XRNode node)
    {
        var go = new GameObject($"{node} Controller (Weeping Angel)");
        go.transform.SetParent(VRSession.Instance.LocalPlayer.Rig.transform, false);
        go.layer = 6;
        
        var controller = go.AddComponent<XRController>();
        var interactor = go.AddComponent<XRRayInteractor>();
        var visual = go.AddComponent<XRInteractorLineVisual>();
        var renderer = go.GetComponent<LineRenderer>();
        var sortingGroup = go.AddComponent<SortingGroup>();

        sortingGroup.sortingOrder = 5;
        
        interactor.rayOriginTransform.localEulerAngles = node switch
        {
            XRNode.LeftHand => new Vector3(60, 347, 90),
            XRNode.RightHand => new Vector3(60, 347, 270),
            _ => throw new ArgumentOutOfRangeException(nameof(node), node, null)
        };
        interactor.raycastMask = 1 << 6;
        interactor.enabled = true;

        visual.lineBendRatio = 1;
        visual.invalidColorGradient = new Gradient()
        {
            mode = GradientMode.Blend,
            alphaKeys =
            [
                new GradientAlphaKey(0.1f, 0), new GradientAlphaKey(0.1f, 1)
            ],
            colorKeys = [
                new GradientColorKey(Color.white, 0),
                new GradientColorKey(Color.white, 1)
            ]
        };
        visual.enabled = true;

        renderer.material = AssetManager.WhiteMat;
        controller.controllerNode = node;

        return go;
    }
}
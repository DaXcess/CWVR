using System;
using CWVR.Assets;
using CWVR.Patches;
using CWVR.Player;
using HarmonyLib;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace CWVR.UI;

public class Customizer : MonoBehaviour
{
    private NonNativeKeyboard keyboard;
    private PlayerCustomizer customizer;
    
    private XRRayInteractor leftHandInteractor;
    private XRRayInteractor rightHandInteractor;

    private XRInteractorLineVisual leftHandVisual;
    private XRInteractorLineVisual rightHandVisual;

    private bool leftTerminal;
    
    private void Awake()
    {
        customizer = GetComponent<PlayerCustomizer>();
        
        // Interactable canvas
        var canvas = GetComponentInChildren<Canvas>();
        canvas.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
        
        // Interactors
        (leftHandInteractor, leftHandVisual) = CreateInteractorController(XRNode.LeftHand);
        (rightHandInteractor, rightHandVisual) = CreateInteractorController(XRNode.RightHand);
        
        leftHandInteractor.enabled = false;
        leftHandVisual.enabled = false;
        
        rightHandInteractor.enabled = false;
        rightHandVisual.enabled = false;
        
        // Keyboard
        keyboard = Instantiate(AssetManager.Keyboard, transform).GetComponent<NonNativeKeyboard>();
        keyboard.transform.localPosition = new Vector3(0, -0.85f, 0.25f);
        keyboard.transform.localEulerAngles = new Vector3(12, 180, 0);
        keyboard.transform.localScale = Vector3.one * 0.0015f;
        
        keyboard.OnKeyboardValueKeyPressed += OnKeyPressed;
        keyboard.OnKeyboardFunctionKeyPressed += OnFnKeyPressed;
        keyboard.OnClosed += KeyboardOnOnClosed;
    }

    private void KeyboardOnOnClosed(object sender, EventArgs e)
    {
        if (leftTerminal)
            return;
        
        customizer.backSound.Play(transform.position);
        customizer.view_g.RPC("RPCA_PlayerLeftTerminal", RpcTarget.All, false);
    }

    private void OnFnKeyPressed(KeyboardKeyFunc obj)
    {
        switch (obj.ButtonFunction)
        {
            case KeyboardKeyFunc.Function.Backspace:
                customizer.backSound.Play(transform.position);
                if (customizer.faceText.text.Length == 0)
                    return;
        
                customizer.view_g.RPC("SetFaceText", RpcTarget.All, customizer.faceText.text[..^1]);
                return;
            
            case KeyboardKeyFunc.Function.Enter:
                customizer.view_g.RPC("RPCA_PlayerLeftTerminal", RpcTarget.All, true);
                customizer.applySound.Play(transform.position);
                return;
        }
    }
    
    private void OnKeyPressed(KeyboardValueKey obj)
    {
        var value = keyboard.IsShifted && !string.IsNullOrEmpty(obj.ShiftValue) ? obj.ShiftValue : obj.Value;
        
        if (value.Length < 1)
            return;

        if (customizer.faceText.text.Length >= 3)
            return;
        
        customizer.typeSound.Play(transform.position);
        customizer.view_g.RPC("SetFaceText", RpcTarget.All, customizer.faceText.text + value[0]);
    }

    internal void OnEnter()
    {
        leftTerminal = false;
        
        leftHandInteractor.enabled = true;
        leftHandVisual.enabled = true;
        
        rightHandInteractor.enabled = true;
        rightHandVisual.enabled = true;
        
        keyboard.PresentKeyboard();
    }

    internal void OnLeave()
    {
        leftTerminal = true;
        
        leftHandInteractor.enabled = false;
        leftHandVisual.enabled = false;
        
        rightHandInteractor.enabled = false;
        rightHandVisual.enabled = false;
        
        keyboard.Close();
    }
    
    private static (XRRayInteractor, XRInteractorLineVisual) CreateInteractorController(XRNode node)
    {
        var go = new GameObject($"{node} Controller (Player Customizer)");
        go.transform.SetParent(VRSession.Instance.LocalPlayer.Rig.transform, false);
        
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
        interactor.raycastMask = 1 << 5;

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

        return (interactor, visual);
    }
}

[CWVRPatch]
[HarmonyPatch]
internal static class PlayerCustomizerPatches
{
    [HarmonyPatch(typeof(PlayerCustomizer), nameof(PlayerCustomizer.RPCA_EnterTerminal))]
    [HarmonyPostfix]
    private static void OnEnterTerminal(PlayerCustomizer __instance)
    {
        if (!__instance.playerInTerminal.refs.view.IsMine)
            return;
        
        __instance.GetComponent<Customizer>().OnEnter();
    }

    [HarmonyPatch(typeof(PlayerCustomizer), nameof(PlayerCustomizer.RPCA_PlayerLeftTerminal))]
    [HarmonyPrefix]
    private static void OnLeaveTerminal(PlayerCustomizer __instance)
    {
        if (!__instance.playerInTerminal.refs.view.IsMine)
            return;
        
        __instance.GetComponent<Customizer>().OnLeave();
    }
}
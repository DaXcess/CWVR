using System;
using CWVR.Assets;
using CWVR.Player;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace CWVR.UI;

public class PauseMenu : MonoBehaviour
{
    private XRRayInteractor leftHandInteractor;
    private XRRayInteractor rightHandInteractor;

    private XRInteractorLineVisual leftHandVisual;
    private XRInteractorLineVisual rightHandVisual;

    private NonNativeKeyboard keyboard;
    
    private void Awake()
    {
        var canvas = GetComponent<Canvas>();
        canvas.worldCamera = VRSession.Instance.MainCamera;
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.transform.localScale = Vector3.one * 0.0025f;
        canvas.gameObject.SetLayerRecursive(6);

        canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(1920, 1080);

        var bgFront = canvas.transform.Find("Background (1)");
        bgFront.localScale = Vector3.one * 10;

        var bgBack = Instantiate(bgFront.gameObject, bgFront.transform.parent);
        bgBack.transform.position += bgFront.transform.forward * -6;
        
        var bgLeft = Instantiate(bgFront.gameObject, bgFront.transform.parent);
        bgLeft.transform.position += bgLeft.transform.forward * -3 + bgLeft.transform.right * -24;
        bgLeft.transform.localScale = new Vector3(1.25f, 10, 10);
        bgLeft.transform.Rotate(0, 90, 0);
        
        var bgRight = Instantiate(bgFront.gameObject, bgFront.transform.parent);
        bgRight.transform.position += bgRight.transform.forward * -3 + bgRight.transform.right * 24;
        bgRight.transform.localScale = new Vector3(1.25f, 10, 10);
        bgRight.transform.Rotate(0, -90, 0);

        var bgTop = Instantiate(bgFront.gameObject, bgFront.transform.parent);
        bgTop.transform.position += bgTop.transform.forward * -3 + bgTop.transform.up * 13.5f;
        bgTop.transform.localScale = new Vector3(1.25f, 17.77777f, 10);
        bgTop.transform.Rotate(90, 90, 0);
        
        var bgBottom = Instantiate(bgFront.gameObject, bgFront.transform.parent);
        bgBottom.transform.position += bgBottom.transform.forward * -3 + bgBottom.transform.up * -13.5f;
        bgBottom.transform.localScale = new Vector3(1.25f, 17.77777f, 10);
        bgBottom.transform.Rotate(90, 90, 0);

        var rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(1920, 1080);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();

        keyboard = Instantiate(AssetManager.Keyboard, GameAPI.instance.transform).GetComponent<NonNativeKeyboard>();
        keyboard.SubmitOnEnter = true;
        keyboard.gameObject.SetLayerRecursive(6);
        keyboard.transform.localScale = Vector3.one * 0.004f;
        keyboard.GetComponent<Canvas>().sortingOrder = 1;
        
        var autoKeyboard = gameObject.AddComponent<AutoKeyboard>();
        autoKeyboard.keyboard = keyboard;

        (leftHandInteractor, leftHandVisual) = CreateInteractorController(XRNode.LeftHand);
        (rightHandInteractor, rightHandVisual) = CreateInteractorController(XRNode.RightHand);
    }

    public void OnOpen()
    {
        leftHandInteractor.enabled = true;
        leftHandVisual.enabled = true;
        
        rightHandInteractor.enabled = true;
        rightHandVisual.enabled = true;
        
        ResetPosition();
        
        keyboard.transform.position = transform.position + Vector3.up * -2.3f + transform.forward * -0.4f;
        keyboard.transform.rotation = transform.rotation * Quaternion.Euler(20, 0, 0);
    }

    public void OnClose()
    {
        leftHandInteractor.enabled = false;
        leftHandVisual.enabled = false;
        
        rightHandInteractor.enabled = false;
        rightHandVisual.enabled = false;
        
        keyboard.Close();
    }

    private void ResetPosition()
    {
        var camTransform = VRSession.Instance.MainCamera.transform;

        transform.position = camTransform.position + camTransform.forward * 3f;
        transform.position = new Vector3(transform.position.x, camTransform.position.y, transform.position.z);
        transform.LookAt(camTransform);
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y + 180, 0);
    }

    private static (XRRayInteractor, XRInteractorLineVisual) CreateInteractorController(XRNode node)
    {
        var go = new GameObject($"{node} Controller");
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
        interactor.raycastMask = 1 << 6;

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
        
        go.SetLayerRecursive(6);

        return (interactor, visual);
    }
}
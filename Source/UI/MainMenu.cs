using System;
using System.Collections;
using CWVR.Assets;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SpatialTracking;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace CWVR.UI;

public class MainMenu : MonoBehaviour
{
    private Transform xrOrigin;
    private TrackedPoseDriver cameraTracker;
    
    private void Awake()
    {
        // Setup VR camera
        var camera = transform.Find("Main Camera").gameObject;

        xrOrigin = new GameObject("XR Origin").transform;
        xrOrigin.position = camera.transform.position;
        xrOrigin.eulerAngles = new Vector3(0, 180, 0);
        
        camera.transform.SetParent(xrOrigin, false);
        
        cameraTracker = camera.AddComponent<TrackedPoseDriver>();
        cameraTracker.deviceType = TrackedPoseDriver.DeviceType.GenericXRDevice;
        cameraTracker.poseSource = TrackedPoseDriver.TrackedPose.Center;
        cameraTracker.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
        
        // Setup interactors
        CreateInteractorController(XRNode.LeftHand);
        CreateInteractorController(XRNode.RightHand);
        
        // Set up canvasses
        var introCanvas = FindObjectOfType<IntroScreenAnimator>().GetComponent<Canvas>();
        var mainMenuCanvas = MainMenuHandler.Instance.UIHandler.GetComponent<Canvas>();
        var modalCanvas = Modal.Instance.GetComponent<Canvas>();
        
        introCanvas.renderMode = mainMenuCanvas.renderMode = modalCanvas.renderMode = RenderMode.WorldSpace;
        introCanvas.worldCamera = mainMenuCanvas.worldCamera = modalCanvas.worldCamera = camera.GetComponent<Camera>();
        introCanvas.transform.localScale = mainMenuCanvas.transform.localScale =
            modalCanvas.transform.localScale = Vector3.one * 0.0033f;

        introCanvas.transform.position = xrOrigin.position + xrOrigin.transform.forward * 3f + new Vector3(0, 1f, 0);
        introCanvas.transform.LookAt(xrOrigin);
        introCanvas.transform.eulerAngles = new Vector3(0, introCanvas.transform.eulerAngles.y + 180, 0);
        
        // Copy transforms to all canvasses
        mainMenuCanvas.transform.position = modalCanvas.transform.position = introCanvas.transform.position;
        mainMenuCanvas.transform.rotation = modalCanvas.transform.rotation = introCanvas.transform.rotation;
        
        // Set resolution on all canvasses
        introCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(1920, 1080);
        mainMenuCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(1920, 1080);
        modalCanvas.GetComponent<RectTransform>().sizeDelta = new Vector2(1920, 1080);
        
        // Revert background scale to default
        modalCanvas.transform.Find("Image").localScale = Vector3.one;
        
        // Required for VR UI interactions
        mainMenuCanvas.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();
        modalCanvas.gameObject.AddComponent<TrackedDeviceGraphicRaycaster>();

        // Create keyboard
        var keyboard = Instantiate(AssetManager.Keyboard).GetComponent<NonNativeKeyboard>();
        keyboard.SubmitOnEnter = true;
        keyboard.transform.position = new Vector3(-20.0352f, 4.4452f, 34.5683f);
        keyboard.transform.eulerAngles = new Vector3(20, 180, 0);
        keyboard.transform.localScale = Vector3.one * 0.005f;

        var autoKeyboard = mainMenuCanvas.gameObject.AddComponent<AutoKeyboard>();
        autoKeyboard.keyboard = keyboard;
        
        if (Plugin.Config.FirstTimeLaunch.Value)
        {
            Modal.Show("Welcome to CW // VR", "Due to the nature of this game, the VR mod might cause more severe motion sickness than usual in some individuals.\nIf you suffer from motion sickness easily, it is recommended to enable \"Reduced Motion Sickness\".\nGoing for the full experience means that the camera can rotate on its own during ragdolls, death, sleeping and certain enemy attacks.\n(You can always change this setting in the VR Settings later).\n\nI wish you happy travels in trying to get SpöökFamous in VR!\n- DaXcess", [
                new ModalOption("Full Experience", () => Plugin.Config.DisableRagdollCamera.Value = false),
                new ModalOption("Reduced Motion Sickness", () => Plugin.Config.DisableRagdollCamera.Value = true)
            ], () => Plugin.Config.FirstTimeLaunch.Value = false);
        }

        StartCoroutine(DisableDoF());
        StartCoroutine(AutoRotate());
    }

    private void CreateInteractorController(XRNode node)
    {
        var go = new GameObject($"{node} Controller");
        go.transform.SetParent(xrOrigin, false);
        
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
    }

    /// <summary>
    /// Disable Depth of Field since it incorrectly blurs text on the world canvas.
    /// I would like to be able to keep this though.
    /// </summary>
    /// <returns></returns>
    private IEnumerator DisableDoF()
    {
        PostVolumeHandler pvh;
        while ((pvh = FindObjectOfType<PostVolumeHandler>()) is null)
            yield return null;

        yield return new WaitUntil(() => pvh.m_volume is not null);
        yield return new WaitUntil(() => pvh.m_volume.profile is not null);
        
        if (pvh.m_volume.profile.TryGet<DepthOfField>(out var dof))
            dof.active = false;
    }

    private IEnumerator AutoRotate()
    {
        Pose pose;
        while ((pose = cameraTracker.GetPoseData()).rotation.eulerAngles == Vector3.zero)
            yield return null;
        
        xrOrigin.eulerAngles = new Vector3(0, 180 - pose.rotation.eulerAngles.y, 0);
    }
}
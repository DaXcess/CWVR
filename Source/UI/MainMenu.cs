using System;
using System.Collections;
using CWVR.Assets;
using CWVR.Input;
using CWVR.Player;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.XR;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
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
        cameraTracker.positionAction = Actions.Instance.HeadPosition;
        cameraTracker.rotationAction = Actions.Instance.HeadRotation;
        cameraTracker.trackingStateInput = new InputActionProperty(Actions.Instance.HeadTrackingState);

        // Setup interactors
        CreateInteractorController(XRNode.LeftHand);
        CreateInteractorController(XRNode.RightHand);
        
        // Disable default input module
        FindObjectOfType<InputSystemUIInputModule>().enabled = false;

        // Set up canvasses
        var introCanvas = FindObjectOfType<IntroScreenAnimator>(true).GetComponent<Canvas>();
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
            Modal.Show("Welcome to CW // VR",
                "Due to the nature of this game, the VR mod might cause more severe motion sickness than usual in some individuals.\nIf you suffer from motion sickness easily, it is recommended to enable \"Reduced Motion Sickness\".\nGoing for the full experience means that the camera can rotate on its own during ragdolls, death, sleeping and certain enemy attacks.\n(You can always change this setting in the VR Settings later).\n\nI wish you happy travels in trying to get SpöökFamous in VR!\n- DaXcess",
                [
                    new ModalOption("Full Experience", () => Plugin.Config.DisableRagdollCamera.Value = false),
                    new ModalOption("Reduced Motion Sickness", () => Plugin.Config.DisableRagdollCamera.Value = true)
                ], () => Plugin.Config.FirstTimeLaunch.Value = false);
        }

        StartCoroutine(ToggleDoF(false));
        StartCoroutine(AutoRotate());
    }

    private void OnDestroy()
    {
        if (VRSession.InVR) // If we're still in VR we just went into a game, destroying will be done for us by the engine
            return;
        
        // Revert camera
        var camera = cameraTracker.gameObject;
        Destroy(cameraTracker);

        camera.transform.SetParent(transform);
        camera.transform.localPosition = new Vector3(33.13f, 4.67f, 70.56f);
        camera.transform.localEulerAngles = new Vector3(0, 162.6179f, 0);
        
        // Destroy XR origin
        Destroy(xrOrigin.gameObject);
        
        // Re-enable default input system
        FindObjectOfType<InputSystemUIInputModule>().enabled = false;
        
        // Revert canvasses
        var introCanvas = FindObjectOfType<IntroScreenAnimator>(true).GetComponent<Canvas>();
        var mainMenuCanvas = MainMenuHandler.Instance.UIHandler.GetComponent<Canvas>();
        var modalCanvas = Modal.Instance.GetComponent<Canvas>();

        introCanvas.renderMode = mainMenuCanvas.renderMode = modalCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        // Destroy keyboard
        Destroy(FindObjectOfType<AutoKeyboard>());
        Destroy(FindObjectOfType<NonNativeKeyboard>(true).gameObject);
        
        // Re-enable Depth of Field
        StartCoroutine(ToggleDoF(true));
    }

    private void CreateInteractorController(XRNode node)
    {
        var go = new GameObject($"{node} Controller");
        go.transform.SetParent(xrOrigin, false);
        
        var controller = go.AddComponent<ActionBasedController>();
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
        visual.invalidColorGradient = new Gradient
        {
            mode = GradientMode.Blend,
            alphaKeys =
            [
                new GradientAlphaKey(0.1f, 0), new GradientAlphaKey(0.1f, 1)
            ],
            colorKeys =
            [
                new GradientColorKey(Color.white, 0),
                new GradientColorKey(Color.white, 1)
            ]
        };
        visual.enabled = true;

        renderer.material = AssetManager.WhiteMat;

        controller.enableInputTracking = true;
        controller.enableInputActions = true;

        switch (node)
        {
            case XRNode.LeftHand:
                controller.positionAction = new InputActionProperty(Actions.Instance.LeftHandPosition);
                controller.rotationAction = new InputActionProperty(Actions.Instance.LeftHandRotation);
                controller.trackingStateAction = new InputActionProperty(Actions.Instance.LeftHandTrackingState);

                controller.selectAction =
                    new InputActionProperty(AssetManager.DefaultXRActions.FindAction("Actions - Left Hand/Select"));
                controller.selectActionValue =
                    new InputActionProperty(
                        AssetManager.DefaultXRActions.FindAction("Actions - Left Hand/Select Value"));
                controller.activateAction =
                    new InputActionProperty(AssetManager.DefaultXRActions.FindAction("Actions - Left Hand/Activate"));
                controller.activateActionValue =
                    new InputActionProperty(
                        AssetManager.DefaultXRActions.FindAction("Actions - Left Hand/Activate Value"));
                controller.uiPressAction =
                    new InputActionProperty(AssetManager.DefaultXRActions.FindAction("Actions - Left Hand/UI Press"));
                controller.uiPressActionValue =
                    new InputActionProperty(
                        AssetManager.DefaultXRActions.FindAction("Actions - Left Hand/UI Press Value"));
                controller.uiScrollAction =
                    new InputActionProperty(AssetManager.DefaultXRActions.FindAction("Actions - Left Hand/UI Scroll"));
                controller.rotateAnchorAction =
                    new InputActionProperty(
                        AssetManager.DefaultXRActions.FindAction("Actions - Left Hand/Rotate Anchor"));
                controller.translateAnchorAction =
                    new InputActionProperty(
                        AssetManager.DefaultXRActions.FindAction("Actions - Left Hand/Translate Anchor"));
                controller.scaleToggleAction =
                    new InputActionProperty(
                        AssetManager.DefaultXRActions.FindAction("Actions - Left Hand/Scale Toggle"));
                controller.scaleDeltaAction =
                    new InputActionProperty(
                        AssetManager.DefaultXRActions.FindAction("Actions - Left Hand/Scale Delta"));
                break;

            case XRNode.RightHand:
                controller.positionAction = new InputActionProperty(Actions.Instance.RightHandPosition);
                controller.rotationAction = new InputActionProperty(Actions.Instance.RightHandRotation);
                controller.trackingStateAction = new InputActionProperty(Actions.Instance.RightHandTrackingState);

                controller.selectAction =
                    new InputActionProperty(AssetManager.DefaultXRActions.FindAction("Actions - Right Hand/Select"));
                controller.selectActionValue =
                    new InputActionProperty(
                        AssetManager.DefaultXRActions.FindAction("Actions - Right Hand/Select Value"));
                controller.activateAction =
                    new InputActionProperty(AssetManager.DefaultXRActions.FindAction("Actions - Right Hand/Activate"));
                controller.activateActionValue =
                    new InputActionProperty(
                        AssetManager.DefaultXRActions.FindAction("Actions - Right Hand/Activate Value"));
                controller.uiPressAction =
                    new InputActionProperty(AssetManager.DefaultXRActions.FindAction("Actions - Right Hand/UI Press"));
                controller.uiPressActionValue =
                    new InputActionProperty(
                        AssetManager.DefaultXRActions.FindAction("Actions - Right Hand/UI Press Value"));
                controller.uiScrollAction =
                    new InputActionProperty(AssetManager.DefaultXRActions.FindAction("Actions - Right Hand/UI Scroll"));
                controller.rotateAnchorAction =
                    new InputActionProperty(
                        AssetManager.DefaultXRActions.FindAction("Actions - Right Hand/Rotate Anchor"));
                controller.translateAnchorAction =
                    new InputActionProperty(
                        AssetManager.DefaultXRActions.FindAction("Actions - Right Hand/Translate Anchor"));
                controller.scaleToggleAction =
                    new InputActionProperty(
                        AssetManager.DefaultXRActions.FindAction("Actions - Right Hand/Scale Toggle"));
                controller.scaleDeltaAction =
                    new InputActionProperty(
                        AssetManager.DefaultXRActions.FindAction("Actions - Right Hand/Scale Delta"));
                break;

            default:
                throw new ArgumentException("Must be either a left hand or right hand node", nameof(node));
        }
    }

    /// <summary>
    /// Disable Depth of Field since it incorrectly blurs text on the world canvas.
    /// </summary>
    private static IEnumerator ToggleDoF(bool active)
    {
        PostVolumeHandler pvh;
        while ((pvh = FindObjectOfType<PostVolumeHandler>()) is null)
            yield return null;

        yield return new WaitUntil(() => pvh.m_volume is not null);
        yield return new WaitUntil(() => pvh.m_volume.profile is not null);
        
        if (pvh.m_volume.profile.TryGet<DepthOfField>(out var dof))
            dof.active = active;
    }

    private IEnumerator AutoRotate()
    {
        var action = Actions.Instance.HeadRotation;
        
        while (action.ReadValue<Quaternion>().eulerAngles == Vector3.zero)
            yield return null;
        
        xrOrigin.eulerAngles = new Vector3(0, 180 - action.ReadValue<Quaternion>().eulerAngles.y, 0);
    }
}
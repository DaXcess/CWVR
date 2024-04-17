using System;
using CurvedUI;
using CWVR.Player;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CWVR.UI;

public class HUD : MonoBehaviour
{
    public Camera UICamera { get; private set; }
    
    public Canvas WorldInteractionCanvas { get; private set; }
    public Canvas LeftHandCanvas { get; private set; }
    public Canvas RightHandCanvas { get; private set; }

    public PauseMenu PauseMenu { get; private set; }
    
    private CurvedUISettings curvedUISettings;
    private Canvas modalCanvas;
    
    private void Awake()
    {
        var rigRoot = global::Player.localPlayer.refs.rigRoot.transform;

        var ui = FindObjectOfType<UserInterface>();
        var leftHand = rigRoot.Find("Rig/Armature/Hip/Torso/Arm_L/Elbow_L/Hand_L");
        var rightHand = rigRoot.Find("Rig/Armature/Hip/Torso/Arm_R/Elbow_R/Hand_R");
        var uiSource = ui.transform.Find("Pivot/Others");

        // Create new Curved UI settings without any curve
        var uiSettingsJson = JsonUtility.ToJson(ui.GetComponent<CurvedUISettings>());
        curvedUISettings = gameObject.AddComponent<CurvedUISettings>();
        JsonUtility.FromJsonOverwrite(uiSettingsJson, curvedUISettings);

        curvedUISettings.Angle = 0;

        WorldInteractionCanvas = new GameObject("World Interaction Canvas").AddComponent<Canvas>();
        WorldInteractionCanvas.worldCamera = VRSession.Instance.MainCamera;
        WorldInteractionCanvas.renderMode = RenderMode.WorldSpace;
        WorldInteractionCanvas.transform.localScale = Vector3.one * 0.0066f;
        WorldInteractionCanvas.gameObject.layer = 6;
        
        LeftHandCanvas = new GameObject("Left Hand VR Canvas").AddComponent<Canvas>();
        LeftHandCanvas.worldCamera = VRSession.Instance.MainCamera;
        LeftHandCanvas.renderMode = RenderMode.WorldSpace;
        LeftHandCanvas.transform.localScale = Vector3.one * 0.001f;
        LeftHandCanvas.gameObject.layer = LayerMask.NameToLayer("UI");
        LeftHandCanvas.transform.SetParent(leftHand, false);
        LeftHandCanvas.transform.localPosition = Vector3.zero;
        LeftHandCanvas.transform.localEulerAngles = Vector3.zero;
        
        RightHandCanvas = new GameObject("Right Hand VR Canvas").AddComponent<Canvas>();
        RightHandCanvas.worldCamera = VRSession.Instance.MainCamera;
        RightHandCanvas.renderMode = RenderMode.WorldSpace;
        RightHandCanvas.transform.localScale = Vector3.one * 0.001f;
        RightHandCanvas.gameObject.layer = LayerMask.NameToLayer("UI");
        RightHandCanvas.transform.SetParent(rightHand, false);
        RightHandCanvas.transform.localPosition = Vector3.zero;
        RightHandCanvas.transform.localEulerAngles = Vector3.zero;
        
        // Interaction UI
        var interactionUi = uiSource.Find("InteractionUI");
        interactionUi.transform.SetParent(WorldInteractionCanvas.transform, false);

        foreach (var text in interactionUi.GetComponentsInChildren<TextMeshProUGUI>())
        {
            // Very cool epic feature in TMPro makes it that if you don't do this then a lot more text can suddenly
            // become always on top
            text.fontSharedMaterial = new Material(text.fontSharedMaterial);
            text.isOverlay = true;
        }
        
        // Disable Key in Interaction UI
        interactionUi.Find("KEY").gameObject.SetActive(false);
        
        // Stamina
        var stamina = uiSource.Find("Stamina");
        
        stamina.transform.SetParent(LeftHandCanvas.transform, false);
        stamina.localPosition = new Vector3(0, 450, -170);
        stamina.localEulerAngles = Vector3.zero;
        stamina.localScale = new Vector3(1.75f, 2.5f, 1.75f);
        
        // O2
        var oxygen = uiSource.Find("O2");
        
        oxygen.transform.SetParent(LeftHandCanvas.transform, false);
        oxygen.localPosition = new Vector3(-55, 610, -170);
        oxygen.localEulerAngles = Vector3.zero;
        oxygen.localScale = Vector3.one * 3.5f;
        
        // Health
        var health = uiSource.Find("Health");
        
        health.transform.SetParent(LeftHandCanvas.transform, false);
        health.localPosition = new Vector3(0, 520, -170);
        health.localEulerAngles = Vector3.zero;
        health.localScale = Vector3.one * 2.5f;
        
        // Hotbar
        var hotbar = uiSource.Find("Hotbar");
        
        hotbar.transform.SetParent(RightHandCanvas.transform, false);
        hotbar.localPosition = new Vector3(-100, 800, -200);
        hotbar.localEulerAngles = new Vector3(0, 0, 270);
        hotbar.localScale = Vector3.one * 2.5f;
        
        // Modal
        modalCanvas = Modal.Instance.GetComponent<Canvas>();
        modalCanvas.worldCamera = VRSession.Instance.MainCamera;
        modalCanvas.renderMode = RenderMode.WorldSpace;
        modalCanvas.transform.localScale = Vector3.one * 0.001f;
        modalCanvas.gameObject.SetLayerRecursive(6);
        modalCanvas.transform.Find("Image").localScale = Vector3.one * 10;
        
        // Pause menu
        PauseMenu = EscapeMenu.Instance.m_menu.gameObject.AddComponent<PauseMenu>();
        
        ReplaceSettings(interactionUi, stamina, oxygen, health, hotbar);
        CreateUICamera();
    }

    private void LateUpdate()
    {
        // Update modal canvas
        var mainCamera = VRSession.Instance.MainCamera.transform;

        modalCanvas.transform.position = mainCamera.transform.position + mainCamera.transform.forward * 1.5f;
        modalCanvas.transform.rotation = mainCamera.rotation;

        // Update world interaction UI
        if (UserInterface.Instance.interactionUI.m_currentInteractable)
            WorldInteractionCanvas.transform.position =
                UserInterface.Instance.interactionUI.m_currentInteractable.transform.position;

        WorldInteractionCanvas.transform.rotation =
            Quaternion.LookRotation(WorldInteractionCanvas.transform.position - mainCamera.position);
        WorldInteractionCanvas.transform.position += WorldInteractionCanvas.transform.forward * -0.2f;
    }

    private void CreateUICamera()
    {
        var mainCamera = VRSession.Instance.MainCamera;
        
        // Create UI Camera
        UICamera = new GameObject("VR UI Camera").AddComponent<Camera>();
        UICamera.transform.SetParent(mainCamera.transform, false);
        UICamera.transform.localPosition = Vector3.zero;
        UICamera.transform.localEulerAngles = Vector3.zero;
        UICamera.clearFlags = CameraClearFlags.Depth;
        UICamera.depth = 10;

        // Update culling masks
        UICamera.cullingMask = 1 << 6;
        mainCamera.cullingMask &= ~(1 << 6);

        // Set UI Camera render type
        UICamera.GetUniversalAdditionalCameraData().renderType = CameraRenderType.Overlay;
        
        // Add to main stack
        var cameraData = mainCamera.GetUniversalAdditionalCameraData();
        cameraData.cameraStack.Add(UICamera);
    }

    private void ReplaceSettings(params Component[] sources)
    {
        foreach (var source in sources)
        {
            var topmost = source.GetComponent<CurvedUIVertexEffect>();
            if (topmost)
                topmost.mySettings = curvedUISettings;
            
            foreach (var child in source.GetComponentsInChildren<CurvedUIVertexEffect>())
                child.mySettings = curvedUISettings;
        }
    }
}
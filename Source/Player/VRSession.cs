using System;
using CWVR.Input;
using CWVR.Networking;
using CWVR.UI;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CWVR.Player;

public class VRSession : MonoBehaviour
{
    public static VRSession Instance { get; private set; }

    /// <summary>
    /// Whether or not the game has VR enabled. This field will only be populated after CWVR has loaded.
    /// </summary>
    public static bool InVR => Plugin.Flags.HasFlag(Flags.VR);
    

    private InputSystem inputSystem;
    
    public Camera MainCamera { get; private set; }
    
    public VRPlayer LocalPlayer { get; private set; }
    public Controls Controls { get; private set; }
    public HUD HUD { get; private set; }
    
    public NetworkManager NetworkManager { get; private set; }
    
    private void Awake()
    {
        Instance = this;

        NetworkManager = new GameObject("VR Network Manager").AddComponent<NetworkManager>();
        
        if (InVR)
            InitializeVRSession();
        
        // Add VR settings to pause menu
        var settingsObj = FindObjectOfType<EscapeMenuSettingsPage>(true).gameObject;
        var settingsMenu = settingsObj.AddComponent<UI.Settings.SettingsMenu>();
        var remapHandler = settingsObj.AddComponent<RemapHandler>();

        settingsMenu.remapHandler = remapHandler;
    }

    private void InitializeVRSession()
    {
        // Store Camera ref
        MainCamera = global::MainCamera.instance.cam;
        
        // Setup Input System and Controls
        inputSystem = new GameObject("VR Input System").AddComponent<InputSystem>();
        Controls = new Controls(inputSystem);
        
        // Create local VR player
        LocalPlayer = global::Player.localPlayer.gameObject.AddComponent<VRPlayer>();
        
        // Create HUD
        HUD = gameObject.AddComponent<HUD>();
        
        // Load controller bindings
        if (Plugin.Config.EnableCustomControls.Value)
            ControlScheme.LoadSchema(Plugin.Config.CustomControls.Value);
        else
            ControlScheme.LoadProfile(InputSystem.DetectControllerProfile());
        
        Controls.ReloadBindings();
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}
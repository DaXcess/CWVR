using CWVR.Assets;
using CWVR.Networking;
using CWVR.UI;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using Zorro.ControllerSupport;

namespace CWVR.Player;

public class VRSession : MonoBehaviour
{
    public static VRSession Instance { get; private set; }

    /// <summary>
    /// Whether the game has VR enabled. This field will only be populated after CWVR has loaded.
    /// </summary>
    public static bool InVR => Plugin.Flags.HasFlag(Flags.VR);

    public Camera MainCamera { get; private set; }
    
    public VRPlayer LocalPlayer { get; private set; }
    public HUD HUD { get; private set; }
    
    public NetworkManager NetworkManager { get; private set; }
    
    private void Awake()
    {
        Instance = this;

        NetworkManager = new GameObject("VR Network Manager").AddComponent<NetworkManager>();
        
        if (InVR)
            InitializeVRSession();
    }

    private void InitializeVRSession()
    {
        // Disable base UI input system
        var input = GameObject.Find("EventSystem")?.GetComponent<InputSystemUIInputModule>();
        if (input != null)
            input.enabled = false;
        
        // Store Camera ref
        MainCamera = global::MainCamera.instance.cam;
        
        // Setup VR inputs
        InputHandler.Instance.m_playerInput.actions = AssetManager.InputActions;
        
        // Create local VR player
        LocalPlayer = global::Player.localPlayer.gameObject.AddComponent<VRPlayer>();
        
        // Create HUD
        HUD = gameObject.AddComponent<HUD>();
    }

    private void OnDestroy()
    {
        Instance = null;
    }
}
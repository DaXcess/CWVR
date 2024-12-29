using System.Collections.Generic;
using System.Linq;
using CWVR.Assets;
using CWVR.UI.Settings;
using UnityEngine;
using UnityEngine.InputSystem;
using Zorro.ControllerSupport;

namespace CWVR.Input;

public class RemapManager : MonoBehaviour
{
    /// <summary>
    /// Due to many VR controllers also reporting "touched" state, we have to disable them as they interfere with
    /// "pressed" bindings (some touched bindings are allowed, since they don't have a corresponding "pressed" binding)
    /// </summary>
    private readonly string[] DISALLOWED_BINDINGS =
    [
        "<WMRSpatialController>/touchpadTouched",
        "<OculusTouchController>/primaryTouched",
        "<OculusTouchController>/secondaryTouched",
        "<OculusTouchController>/triggerTouched",
        "<OculusTouchController>/thumbstickTouched",
        "<ViveController>/trackpadTouched",
        "<ValveIndexController>/systemTouched",
        "<ValveIndexController>/primaryTouched",
        "<ValveIndexController>/secondaryTouched",
        "<ValveIndexController>/gripForce",
        "<ValveIndexController>/triggerTouched",
        "<ValveIndexController>/thumbstickTouched",
        "<ValveIndexController>/trackpadTouched",
        "<ValveIndexController>/trackpadForce",
        "<QuestProTouchController>/primaryTouched",
        "<QuestProTouchController>/secondaryTouched",
        "<QuestProTouchController>/triggerTouched",
        "<QuestProTouchController>/thumbstickTouched",
        "<QuestProTouchController>/triggerCurl",
        "<QuestProTouchController>/triggerSlide",
        "<QuestProTouchController>/triggerProximity",
        "<QuestProTouchController>/thumbProximity",
        "*/isTracked",
    ];

    public static RemapManager Instance { get; private set; }

    private PlayerInput playerInput;
    private ControlSettingHeader header;
    private readonly List<RemapBinding> cells = [];
    
    private SFX_Instance hoverSound;
    private SFX_Instance clickSound;

    private InputActionRebindingExtensions.RebindingOperation currentOperation;
    private RemapBinding currentBinding;
    private float lastRebindTime;

    private void Awake()
    {
        Instance = this;
        playerInput = InputHandler.Instance.m_playerInput;
        
        // Grab sound effects from another UI element
        var button = FindObjectOfType<EscapeMenuButton>();

        hoverSound = button.hoverSound;
        clickSound = button.clickSound;
        
        playerInput.onControlsChanged += OnControlsChanged;
        
        // Load binding overrides
        playerInput.actions.LoadBindingOverridesFromJson(Plugin.Config.ControllerBindingsOverride.Value);
        
        ReloadBindings();
    }

    private void OnDestroy()
    {
        DestroySettings();
        
        playerInput.onControlsChanged -= OnControlsChanged;
    }

    public void DisplaySettings(Transform container)
    {
        header = Instantiate(AssetManager.ControlSettingHeaderCell, container).GetComponent<ControlSettingHeader>();
        
        var headerSounds = header.GetComponentInChildren<UI_Sound>();
        headerSounds.hoverSound = hoverSound;
        headerSounds.clickSound = clickSound;

        header.button.onClick.AddListener(ResetBindings);
        
        foreach (var remappableControl in AssetManager.RemappableControls.controls)
        {
            var component = Instantiate(AssetManager.ControlSettingCell, container)
                .GetComponent<RemapBinding>();

            component.Setup(remappableControl, hoverSound, clickSound);
            
            cells.Add(component);
        }
        
        ReloadBindings();
    }

    public void DestroySettings()
    {
        if (header && header.gameObject != null)
            Destroy(header.gameObject);

        foreach (var cell in cells.Where(cell => cell != null && cell.gameObject != null))
            Destroy(cell.gameObject);
        
        cells.Clear();
    }

    /// <summary>
    /// Initiate the interactive rebinding of a control
    /// </summary>
    public bool StartRebind(RemapBinding binding)
    {
        // Prevent accidentally re-triggering the rebind process
        if (Time.realtimeSinceStartup - lastRebindTime < 0.5f)
            return false;

        // Don't allow rebinding until a controller scheme is known
        if (string.IsNullOrEmpty(playerInput.currentControlScheme))
            return false;

        if (currentOperation != null)
        {
            currentOperation.Dispose();

            if (currentBinding != null)
                currentBinding.OnFinishRebind();
        }

        playerInput.DeactivateInput();
        currentBinding = binding;
        currentOperation = binding.ActionReference.action.PerformInteractiveRebinding(binding.RebindIndex)
            .OnMatchWaitForAnother(0.1f).WithControlsHavingToMatchPath("<XRController>").WithTimeout(5)
            .OnComplete(_ => CompleteRebind(binding))
            .OnCancel(_ => CompleteRebind(binding));

        foreach (var exclude in DISALLOWED_BINDINGS)
            currentOperation.WithControlsExcluding(exclude);

        currentOperation.Start();

        return true;
    }

    private void OnControlsChanged(PlayerInput input)
    {
        ReloadBindings();
    }
    
    private void ReloadBindings()
    {
        if (!header)
            return;
        
        header.headerText.text = string.IsNullOrEmpty(playerInput.currentControlScheme)
            ? "Please connect both controllers"
            : $"VR Controllers: {playerInput.currentControlScheme}";
        
        foreach (var option in cells)
        {
            option.Reload();
        }
    }

    private void ResetBindings()
    {
        playerInput.actions.RemoveAllBindingOverrides();
        Plugin.Config.ControllerBindingsOverride.Value = "";
        
        ReloadBindings();
    }
    
    /// <summary>
    /// Finalize the rebinding operation, and permanently update the binding configuration
    /// </summary>
    private void CompleteRebind(RemapBinding binding)
    {
        currentOperation.Dispose();
        playerInput.ActivateInput();

        lastRebindTime = Time.realtimeSinceStartup;
        
        binding.OnFinishRebind();

        Plugin.Config.ControllerBindingsOverride.Value = playerInput.actions.SaveBindingOverridesAsJson();
    }
}
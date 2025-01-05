using UnityEngine.Rendering.Universal;

namespace CWVR.MultiLoader.Common;

public interface IConfig
{
    
    // General configuration
    
    public IConfigEntry<bool> DisableVR { get; }
    public IConfigEntry<bool> DisableRagdollCamera { get; }
    public IConfigEntry<bool> EnableVerboseLogging { get; }
    
    // Graphics configuration
    
    public IConfigEntry<float> RenderScale { get; }
    public IConfigEntry<UpscalingFilterSelection> UpscalingFilter { get; }
    public IConfigEntry<bool> EnableOcclusionMesh { get; }

    // Input configuration

    public IConfigEntry<TurnProviderOption> TurnProvider { get; }
    public IConfigEntry<float> SmoothTurnSpeedModifier { get; }
    public IConfigEntry<int> SnapTurnSize { get; }
    public IConfigEntry<SprintActivationMode> SprintActivation { get; }
    public IConfigEntry<bool> InteractToZoom { get; }
    
    // Internal configuration

    public IConfigEntry<bool> FirstTimeLaunch { get; }
    public IConfigEntry<string> OpenXRRuntimeFile { get; }
    public IConfigEntry<string> ControllerBindingsOverride { get; } 
    
    public enum TurnProviderOption
    {
        Snap,
        Smooth,
        Disabled
    }

    public enum SprintActivationMode
    {
        Hold,
        Press
    }
}

public interface IConfigEntry<TValue>
{
    public TValue Value { get; set; }
}
using System;
using BepInEx.Configuration;
using CWVR.MultiLoader.Common;
using UnityEngine.Rendering.Universal;

namespace CWVR.MultiLoader.BepInEx;

public class Config(ConfigFile file) : IConfig
{
    public ConfigFile File { get; } = file;
    
    // General configuration

    public IConfigEntry<bool> DisableVR { get; } = new WrappedConfigEntry<bool>(file.Bind("General", "DisableVR", false,
        "Disables the main functionality of this mod, can be used if you want to play without VR while keeping the mod installed."));

    public IConfigEntry<bool> DisableRagdollCamera { get; } = new WrappedConfigEntry<bool>(
        file.Bind("General", "DisableRagdollCamera", false, "Disable if coward"));

    public IConfigEntry<bool> EnableVerboseLogging { get; } =
        new WrappedConfigEntry<bool>(file.Bind("General", "EnableVerboseLogging", false,
            "Enables verbose debug logging during OpenXR initialization"));
    
    // Graphics configuration

    public IConfigEntry<float> RenderScale { get; } = new WrappedConfigEntry<float>(file.Bind("Graphics",
        "RenderScale", 1f,
        new ConfigDescription(
            "The resolution scale to render the game in. Lower values mean more performance, at the cost of quality.",
            new AcceptableValueRange<float>(0.1f, 2))));

    public IConfigEntry<UpscalingFilterSelection> UpscalingFilter { get; } =
        new WrappedConfigEntry<UpscalingFilterSelection>(file.Bind("Graphics", "UpscalingFilter",
            UpscalingFilterSelection.Auto,
            new ConfigDescription(
                "The filter to use to perform upscaling back to native resolution. Is only used if the render scale is lower than 1.",
                new AcceptableValueEnum<UpscalingFilterSelection>())));

    public IConfigEntry<bool> EnableOcclusionMesh { get; } =
        new WrappedConfigEntry<bool>(file.Bind("Graphics", "EnableOcclusionMesh", true,
            "The occlusion mesh will cause the game to stop rendering pixels outside of the lens' views, which increases performance."));

    // Input configuration

    public IConfigEntry<IConfig.TurnProviderOption> TurnProvider { get; } =
        new WrappedConfigEntry<IConfig.TurnProviderOption>(file.Bind("Input",
            "TurnProvider",
            IConfig.TurnProviderOption.Smooth,
            new ConfigDescription("Specify which turning provider your player uses, if any.",
                new AcceptableValueEnum<IConfig.TurnProviderOption>())));

    public IConfigEntry<float> SmoothTurnSpeedModifier { get; } = new WrappedConfigEntry<float>(file.Bind("Input",
        "SmoothTurnSpeedModifier", 1f,
        new ConfigDescription(
            "A multiplier that is added to the smooth turning speed. Requires turn provider to be set to smooth.",
            new AcceptableValueRange<float>(0.25f, 5))));

    public IConfigEntry<int> SnapTurnSize { get; } = new WrappedConfigEntry<int>(file.Bind("Input", "SnapTurnSize", 45,
        new ConfigDescription(
            "The amount of rotation that is applied when performing a snap turn. Requires turn provider to be set to snap.",
            new AcceptableValueRange<int>(10, 180))));

    public IConfigEntry<bool> ToggleSprint { get; } = new WrappedConfigEntry<bool>(file.Bind("Input", "ToggleSprint",
        false,
        "Whether the sprint button should toggle sprint instead of having to hold it down."));

    public IConfigEntry<bool> InteractToZoom { get; } = new WrappedConfigEntry<bool>(file.Bind("Input",
        "InteractToZoom", false,
        "Require holding the interact button to zoom the camera. Removes the need to hold interact to swap items."));

    // Internal configuration

    public IConfigEntry<bool> FirstTimeLaunch { get; } = new WrappedConfigEntry<bool>(file.Bind("Internal",
        "FirstTimeLaunch", true,
        "Keeps track if the game was launched in VR before. For internal use only."));

    public IConfigEntry<string> OpenXRRuntimeFile { get; } = new WrappedConfigEntry<string>(file.Bind("Internal",
        "OpenXRRuntimeFile", "",
        "Overrides the OpenXR plugin to use a specific json file. For internal use only."));

    public IConfigEntry<string> ControllerBindingsOverride { get; } = new WrappedConfigEntry<string>(
        file.Bind("Internal", "CustomControls", "", "The custom control schema to use"));
}

public class WrappedConfigEntry<T>(ConfigEntry<T> entry) : IConfigEntry<T>
{
    public T Value
    {
        get => entry.Value;
        set => entry.Value = value;
    }
}

internal class AcceptableValueEnum<T>() : AcceptableValueBase(typeof(T))
    where T : notnull, Enum
{
    private readonly string[] names = Enum.GetNames(typeof(T));

    public override object Clamp(object value) => value;
    public override bool IsValid(object value) => true;
    public override string ToDescriptionString() => $"# Acceptable values: {string.Join(", ", names)}";
}
using System;
using BepInEx.Configuration;
using CWVR.MultiLoader.Common;
using CWVR.Patches;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CWVR.MultiLoader.BepInEx;

public class Config(ConfigFile file) : IConfig
{
    public ConfigFile File { get; } = file;

    // General configuration

    public IConfigEntry<bool> DisableVR { get; } = file.BindWrapped("General", "DisableVR", false,
        "Disables the main functionality of this mod, can be used if you want to play without VR while keeping the mod installed.");

    public IConfigEntry<bool> DisableRagdollCamera { get; } =
        file.BindWrapped("General", "DisableRagdollCamera", false, "Disable if coward");

    public IConfigEntry<bool> EnableVerboseLogging { get; } = file.BindWrapped("General", "EnableVerboseLogging", false,
        "Enables verbose debug logging during OpenXR initialization");
    
    // Graphics configuration

    public IConfigEntry<float> RenderScale { get; } = file.BindWrapped("Graphics", "RenderScale", 1f,
        new ConfigDescription(
            "The resolution scale to render the game in. Lower values mean more performance, at the cost of quality.",
            new AcceptableValueRange<float>(0.1f, 2)));

    public IConfigEntry<UpscalingFilterSelection> UpscalingFilter { get; } = file.BindWrapped("Graphics",
        "UpscalingFilter", UpscalingFilterSelection.Auto,
        new ConfigDescription(
            "The filter to use to perform upscaling back to native resolution. Is only used if the render scale is lower than 1.",
            new AcceptableValueEnum<UpscalingFilterSelection>()));

    public IConfigEntry<bool> EnableOcclusionMesh { get; } = file.BindWrapped("Graphics", "EnableOcclusionMesh", true,
        "The occlusion mesh will cause the game to stop rendering pixels outside of the lens' views, which increases performance.");

    // Input configuration

    public IConfigEntry<IConfig.TurnProviderOption> TurnProvider { get; } = file.BindWrapped("Input", "TurnProvider",
        IConfig.TurnProviderOption.Smooth,
        new ConfigDescription("Specify which turning provider your player uses, if any.",
            new AcceptableValueEnum<IConfig.TurnProviderOption>()));

    public IConfigEntry<float> SmoothTurnSpeedModifier { get; } = file.BindWrapped("Input", "SmoothTurnSpeedModifier",
        1f,
        new ConfigDescription(
            "A multiplier that is added to the smooth turning speed. Requires turn provider to be set to smooth.",
            new AcceptableValueRange<float>(0.25f, 5)));

    public IConfigEntry<int> SnapTurnSize { get; } = file.BindWrapped("Input", "SnapTurnSize", 45,
        new ConfigDescription(
            "The amount of rotation that is applied when performing a snap turn. Requires turn provider to be set to snap.",
            new AcceptableValueRange<int>(10, 180)));

    public IConfigEntry<IConfig.SprintActivationMode> SprintActivation { get; } = file.BindWrapped("Input",
        nameof(SprintActivation), IConfig.SprintActivationMode.Press,
        new ConfigDescription(
            "Determines the way sprint should be used: whether you need to hold the button, or only press it once.",
            new AcceptableValueEnum<IConfig.SprintActivationMode>()));

    public IConfigEntry<bool> InteractToZoom { get; } = file.BindWrapped("Input", "InteractToZoom", true,
        "Require holding the interact button to zoom the camera. Removes the need to hold interact to swap items.");

    // Internal configuration

    public IConfigEntry<bool> FirstTimeLaunch { get; } = file.BindWrapped("Internal", "FirstTimeLaunch", true,
        "Keeps track if the game was launched in VR before. For internal use only.");

    public IConfigEntry<string> OpenXRRuntimeFile { get; } = file.BindWrapped("Internal", "OpenXRRuntimeFile", "",
        "Overrides the OpenXR plugin to use a specific json file. For internal use only.");

    public IConfigEntry<string> ControllerBindingsOverride { get; } =
        file.BindWrapped("Internal", "CustomControls", "", "The custom control schema to use");
    
    // Event Handlers

    public void ApplySettings()
    {
        XRPatches.EnableOcclusionMesh = EnableOcclusionMesh.Value;
        
        var asset = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
        if (asset == null) // It would be very weird if this is true
            return;

        asset.renderScale = RenderScale.Value;
        asset.upscalingFilter = UpscalingFilter.Value;
    }
}

public class BepInConfigEntry<T>(ConfigEntry<T> entry) : IConfigEntry<T>
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

internal static class BepInConfigEntryExtensions
{
    public static BepInConfigEntry<T> BindWrapped<T>(this ConfigFile file, string section, string key, T defaultValue,
        ConfigDescription configDescription = null)
    {
        return new BepInConfigEntry<T>(file.Bind(section, key, defaultValue, configDescription));
    }

    public static BepInConfigEntry<T> BindWrapped<T>(this ConfigFile file, string section, string key, T defaultValue, string description)
    {
        return new BepInConfigEntry<T>(file.Bind(section, key, defaultValue, description));
    }
}
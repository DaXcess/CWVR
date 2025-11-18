using System;
using BepInEx.Configuration;
using CWVR.Patches;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CWVR;

public class Config(ConfigFile file)
{
    public ConfigFile File { get; } = file;

    // General configuration

    public ConfigEntry<bool> DisableVR { get; } = file.Bind("General", "DisableVR", false,
        "Disables the main functionality of this mod, can be used if you want to play without VR while keeping the mod installed.");

    public ConfigEntry<bool> DisableRagdollCamera { get; } =
        file.Bind("General", "DisableRagdollCamera", false, "Disable if coward");

    public ConfigEntry<bool> EnableVerboseLogging { get; } = file.Bind("General", "EnableVerboseLogging", false,
        "Enables verbose debug logging during OpenXR initialization");
    
    // Graphics configuration

    public ConfigEntry<float> RenderScale { get; } = file.Bind("Graphics", "RenderScale", 1f,
        new ConfigDescription(
            "The resolution scale to render the game in. Lower values mean more performance, at the cost of quality.",
            new AcceptableValueRange<float>(0.1f, 2)));

    public ConfigEntry<UpscalingFilterSelection> UpscalingFilter { get; } = file.Bind("Graphics",
        "UpscalingFilter", UpscalingFilterSelection.Auto,
        new ConfigDescription(
            "The filter to use to perform upscaling back to native resolution. Is only used if the render scale is lower than 1.",
            new AcceptableValueEnum<UpscalingFilterSelection>()));

    public ConfigEntry<bool> EnableOcclusionMesh { get; } = file.Bind("Graphics", "EnableOcclusionMesh", true,
        "The occlusion mesh will cause the game to stop rendering pixels outside of the lens' views, which increases performance.");

    // Input configuration

    public ConfigEntry<TurnProviderOption> TurnProvider { get; } = file.Bind("Input", "TurnProvider",
        TurnProviderOption.Smooth,
        new ConfigDescription("Specify which turning provider your player uses, if any.",
            new AcceptableValueEnum<TurnProviderOption>()));

    public ConfigEntry<float> SmoothTurnSpeedModifier { get; } = file.Bind("Input", "SmoothTurnSpeedModifier",
        1f,
        new ConfigDescription(
            "A multiplier that is added to the smooth turning speed. Requires turn provider to be set to smooth.",
            new AcceptableValueRange<float>(0.25f, 5)));

    public ConfigEntry<int> SnapTurnSize { get; } = file.Bind("Input", "SnapTurnSize", 45,
        new ConfigDescription(
            "The amount of rotation that is applied when performing a snap turn. Requires turn provider to be set to snap.",
            new AcceptableValueRange<int>(10, 180)));

    public ConfigEntry<SprintActivationMode> SprintActivation { get; } = file.Bind("Input",
        nameof(SprintActivation), SprintActivationMode.Press,
        new ConfigDescription(
            "Determines the way sprint should be used: whether you need to hold the button, or only press it once.",
            new AcceptableValueEnum<SprintActivationMode>()));

    public ConfigEntry<bool> InteractToZoom { get; } = file.Bind("Input", "InteractToZoom", true,
        "Require holding the interact button to zoom the camera. Removes the need to hold interact to swap items.");

    // Internal configuration

    public ConfigEntry<bool> FirstTimeLaunch { get; } = file.Bind("Internal", "FirstTimeLaunch", true,
        "Keeps track if the game was launched in VR before. For internal use only.");

    public ConfigEntry<string> OpenXRRuntimeFile { get; } = file.Bind("Internal", "OpenXRRuntimeFile", "",
        "Overrides the OpenXR plugin to use a specific json file. For internal use only.");

    public ConfigEntry<string> ControllerBindingsOverride { get; } =
        file.Bind("Internal", "CustomControls", "", "The custom control schema to use");
    
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

internal class AcceptableValueEnum<T>() : AcceptableValueBase(typeof(T))
    where T : notnull, Enum
{
    private readonly string[] names = Enum.GetNames(typeof(T));

    public override object Clamp(object value) => value;
    public override bool IsValid(object value) => true;
    public override string ToDescriptionString() => $"# Acceptable values: {string.Join(", ", names)}";
}

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
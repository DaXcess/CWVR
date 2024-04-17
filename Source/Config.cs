using System;
using BepInEx.Configuration;

namespace CWVR;

public class Config(ConfigFile file)
{
    public ConfigFile File { get; } = file;

    // General configuration

    public ConfigEntry<bool> DisableVR { get; } = file.Bind("General", "DisableVR", false,
        "Disables the main functionality of this mod, can be used if you want to play without VR while keeping the mod installed.");

    public ConfigEntry<bool> DisableRagdollCamera { get; } =
        file.Bind("General", "DisableRagdollCamera", false, "Disable if coward");
    
    // Input configuration

    public ConfigEntry<TurnProviderOption> TurnProvider { get; } = file.Bind("Input", "TurnProvider",
        TurnProviderOption.Smooth,
        new ConfigDescription($"Specify which turning provider your player uses, if any.",
            new AcceptableValueEnum<TurnProviderOption>()));

    public ConfigEntry<float> SmoothTurnSpeedModifier { get; } = file.Bind("Input", "SmoothTurnSpeedModifier", 1f,
        new ConfigDescription(
            "A multiplier that is added to the smooth turning speed. Requires turn provider to be set to smooth.",
            new AcceptableValueRange<float>(0.25f, 5)));

    public ConfigEntry<float> SnapTurnSize { get; } = file.Bind("Input", "SnapTurnSize", 45f,
        new ConfigDescription(
            "The amount of rotation that is applied when performing a snap turn. Requires turn provider to be set to snap.",
            new AcceptableValueRange<float>(10, 180)));
    
    public ConfigEntry<bool> ToggleSprint { get; } = file.Bind("Input", "ToggleSprint", false,
        "Whether the sprint button should toggle sprint instead of having to hold it down.");

    public enum TurnProviderOption
    {
        Snap,
        Smooth,
        Disabled
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
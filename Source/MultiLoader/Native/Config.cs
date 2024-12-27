using System;
using System.Collections.Generic;
using System.Linq;
using CWVR.MultiLoader.Common;
using CWVR.Patches;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Zorro.Settings;

namespace CWVR.MultiLoader.Native;

#if NATIVE

public class Config : IConfig
{
    
    // General configuration

    public IConfigEntry<bool> DisableVR => GameHandler.Instance.SettingsHandler.GetSetting<DisableVRSetting>();

    public IConfigEntry<bool> DisableRagdollCamera =>
        GameHandler.Instance.SettingsHandler.GetSetting<DisableRagdollCameraSetting>();

    public IConfigEntry<bool> EnableVerboseLogging =>
        GameHandler.Instance.SettingsHandler.GetSetting<VerboseLoggingSetting>();
    
    // Graphics configuration

    public IConfigEntry<float> RenderScale => GameHandler.Instance.SettingsHandler.GetSetting<RenderScaleSetting>();

    public IConfigEntry<UpscalingFilterSelection> UpscalingFilter =>
        GameHandler.Instance.SettingsHandler.GetSetting<UpscalingFilterSetting>();

    public IConfigEntry<bool> EnableOcclusionMesh =>
        GameHandler.Instance.SettingsHandler.GetSetting<OcclusionMeshSetting>();

    // Input configuration

    public IConfigEntry<IConfig.TurnProviderOption> TurnProvider =>
        GameHandler.Instance.SettingsHandler.GetSetting<TurnProviderSetting>();

    public IConfigEntry<float> SmoothTurnSpeedModifier =>
        GameHandler.Instance.SettingsHandler.GetSetting<SmoothTurnSpeedSetting>();

    public IConfigEntry<int> SnapTurnSize => GameHandler.Instance.SettingsHandler.GetSetting<SnapTurnSizeSetting>();
    
    public IConfigEntry<bool> ToggleSprint => GameHandler.Instance.SettingsHandler.GetSetting<PressToSprintSetting>();

    public IConfigEntry<bool> InteractToZoom =>
        GameHandler.Instance.SettingsHandler.GetSetting<InteractToZoomSetting>();

    
    // Internal configuration
    
    public IConfigEntry<bool> FirstTimeLaunch =>
        GameHandler.Instance.SettingsHandler.GetSetting<FirstTimeLaunchSetting>();
    
    public IConfigEntry<string> OpenXRRuntimeFile =>
        GameHandler.Instance.SettingsHandler.GetSetting<OpenXRRuntimeSetting>();

    public IConfigEntry<string> ControllerBindingsOverride =>
        GameHandler.Instance.SettingsHandler.GetSetting<ControllerBindingsOverrideSetting>();
}

[ContentWarningSetting]
[UsedImplicitly]
public class DisableVRSetting : EnumSetting, IExposedSetting, IConfigEntry<bool>
{
    public new bool Value
    {
        get => base.Value == 1;
        set => base.Value = value ? 1 : 0;
    }

    public override void ApplyValue()
    {
    }

    public override int GetDefaultValue() => 0;

    public override List<string> GetChoices()
    {
        return ["Enable VR", "Disable VR"];
    }

    public SettingCategory GetSettingCategory() => SettingCategory.Mods;

    public string GetDisplayName() => "[CWVR] Enable VR Mod";
}

[ContentWarningSetting]
[UsedImplicitly]
public class DisableRagdollCameraSetting : EnumSetting, IExposedSetting, IConfigEntry<bool>
{
    public new bool Value
    {
        get => base.Value == 1;
        set => base.Value = value ? 1 : 0;
    }
    
    public override void ApplyValue()
    {
    }

    public override int GetDefaultValue() => 0;

    public override List<string> GetChoices()
    {
        return ["Enabled", "Disabled"];
    }

    public SettingCategory GetSettingCategory() => SettingCategory.Mods;

    public string GetDisplayName() => "[CWVR] Ragdoll Camera Motion";
}

[ContentWarningSetting]
[UsedImplicitly]
public class VerboseLoggingSetting : EnumSetting, IExposedSetting, IConfigEntry<bool>
{
    public new bool Value
    {
        get => base.Value == 1;
        set => base.Value = value ? 1 : 0;
    }
    
    public override void ApplyValue()
    {
    }

    public override int GetDefaultValue() => 0;

    public override List<string> GetChoices() => ["Disabled", "Enabled"];

    public SettingCategory GetSettingCategory() => SettingCategory.Mods;

    public string GetDisplayName() => "[CWVR] Enable Verbose Startup Logging";
}

[ContentWarningSetting]
[UsedImplicitly]
public class RenderScaleSetting : FloatSetting, IExposedSetting, IConfigEntry<float>
{
    public override void ApplyValue()
    {
        var asset = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
        if (asset == null) // It would be very weird if this is true
            return;

        asset.renderScale = Value;
    }

    public override float GetDefaultValue() => 1;

    public override float2 GetMinMaxValue() => new(0.1f, 2);

    public SettingCategory GetSettingCategory() => SettingCategory.Graphics;

    public string GetDisplayName() => "Render Scale";
}

[ContentWarningSetting]
[UsedImplicitly]
public class UpscalingFilterSetting : EnumSetting, IExposedSetting, IConfigEntry<UpscalingFilterSelection>
{
    public new UpscalingFilterSelection Value
    {
        get => (UpscalingFilterSelection)base.Value;
        set => base.Value = (int)value;
    }
    
    public override void ApplyValue()
    {
        var asset = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
        if (asset == null) // It would be very weird if this is true
            return;

        asset.upscalingFilter = Value;
    }

    public override List<string> GetChoices() => Enum.GetNames(typeof(UpscalingFilterSelection)).ToList();

    public override int GetDefaultValue() => (int)UpscalingFilterSelection.Auto;

    public SettingCategory GetSettingCategory() => SettingCategory.Graphics;

    public string GetDisplayName() => "Upscaling Filter";
}

[ContentWarningSetting]
[UsedImplicitly]
public class OcclusionMeshSetting : EnumSetting, IExposedSetting, IConfigEntry<bool>
{
    public new bool Value
    {
        get => base.Value == 1; 
        set => base.Value = value ? 1 : 0;
    }

    public override void ApplyValue()
    {
        XRPatches.EnableOcclusionMesh = Value;
    }

    public override int GetDefaultValue() => 1;

    public SettingCategory GetSettingCategory() => SettingCategory.Graphics;

    public string GetDisplayName() => "Enable Occlusion Mesh";

    public override List<string> GetChoices() => ["Disabled", "Enabled"];
}

[ContentWarningSetting]
[UsedImplicitly]
public class TurnProviderSetting : EnumSetting, IExposedSetting, IConfigEntry<IConfig.TurnProviderOption>
{
    
    public new IConfig.TurnProviderOption Value
    {
        get => (IConfig.TurnProviderOption)base.Value;
        set => base.Value = (int)value;
    }

    public override void ApplyValue()
    {
    }

    public override int GetDefaultValue()
    {
        return (int)IConfig.TurnProviderOption.Smooth;
    }

    public override List<string> GetChoices()
    {
        return Enum.GetNames(typeof(IConfig.TurnProviderOption)).ToList();
    }

    public SettingCategory GetSettingCategory()
    {
        return SettingCategory.Mods;
    }

    public string GetDisplayName()
    {
        return "[CWVR] Controller Turning Mode";
    }
}

[ContentWarningSetting]
[UsedImplicitly]
public class SmoothTurnSpeedSetting : FloatSetting, IExposedSetting, IConfigEntry<float>
{
    public override void ApplyValue()
    {
    }

    public override float GetDefaultValue() => 1;

    public override float2 GetMinMaxValue() => new(0.1f, 5);

    public SettingCategory GetSettingCategory() => SettingCategory.Mods;
    
    public string GetDisplayName() => "[CWVR] Smooth Turning Speed";
}

[ContentWarningSetting]
[UsedImplicitly]
public class SnapTurnSizeSetting : FloatSetting, IExposedSetting, IConfigEntry<int>
{
    public new int Value
    {
        get => (int)base.Value;
        set => base.Value = value;
    }

    public override void ApplyValue()
    {
        if (!Mathf.Approximately(base.Value, Value))
            base.Value = Value;
    }

    public override float GetDefaultValue() => 45;

    public override float2 GetMinMaxValue() => new(10, 180);

    public SettingCategory GetSettingCategory() => SettingCategory.Mods;

    public string GetDisplayName() => "[CWVR] Snap Turn Size";

    public override float Clamp(float value) => Mathf.RoundToInt(base.Clamp(value));

    public override string Expose(float result) => Mathf.RoundToInt(result).ToString();
}

[ContentWarningSetting]
[UsedImplicitly]
public class PressToSprintSetting : EnumSetting, IExposedSetting, IConfigEntry<bool>
{
    public new bool Value
    {
        get => base.Value == 1;
        set => base.Value = value ? 1 : 0;
    }

    public override void ApplyValue()
    {
    }

    public override int GetDefaultValue() => 0;

    public override List<string> GetChoices() => ["Press", "Hold"];

    public SettingCategory GetSettingCategory() => SettingCategory.Mods;

    public string GetDisplayName() => "[CWVR] Sprint Activation Mode";
}

[ContentWarningSetting]
[UsedImplicitly]
public class InteractToZoomSetting : EnumSetting, IExposedSetting, IConfigEntry<bool>
{
    public new bool Value
    {
        get => base.Value == 1;
        set => base.Value = value ? 1 : 0;
    }

    public override void ApplyValue()
    {
    }

    public override int GetDefaultValue() => 1;

    public SettingCategory GetSettingCategory() => SettingCategory.Mods;

    public override List<string> GetChoices() => ["Disabled", "Required"];

    public string GetDisplayName() => "[CWVR] Hold Interact to Zoom";
}

[ContentWarningSetting]
[UsedImplicitly]
public class FirstTimeLaunchSetting : BoolSetting, IConfigEntry<bool>
{
    public override void ApplyValue()
    {
    }

    public override bool GetDefaultValue() => true;
}

[ContentWarningSetting]
[UsedImplicitly]
public class OpenXRRuntimeSetting : StringSetting, IConfigEntry<string>
{
    public override void ApplyValue()
    {
    }

    public override string GetDefaultValue() => "";
}

[ContentWarningSetting]
[UsedImplicitly]
public class ControllerBindingsOverrideSetting : StringSetting, IConfigEntry<string>
{
    public override void ApplyValue()
    {
    }

    public override string GetDefaultValue() => "";
}

#endif
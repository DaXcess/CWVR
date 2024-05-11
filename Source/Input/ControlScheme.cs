using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine.XR.Interaction.Toolkit;
using XRController = CWVR.Input.InputSystem.XRController;

namespace CWVR.Input;

public static class ControlScheme
{
    private static readonly Dictionary<string, Dictionary<string, Binding>> BuiltInControls =
        JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Binding>>>(
            Encoding.UTF8.GetString(Properties.Resources.controls));
    
    private static Dictionary<string, Binding> currentScheme = [];

    public static IReadOnlyDictionary<string, Binding> Scheme => currentScheme;

    static ControlScheme()
    {
        // Make sure the default profile is loaded, well..., by default
        LoadProfile("default");
    }

    /// <summary>
    /// Get the bindings for a specified built-in profile
    /// </summary>
    public static Dictionary<string, Binding> GetProfile(string name)
    {
        return BuiltInControls[name];
    }
    
    /// <summary>
    /// Loads a specified built in controller profile
    /// </summary>
    public static void LoadProfile(string name)
    {
        currentScheme = new Dictionary<string, Binding>(BuiltInControls[name]);
    }

    /// <summary>
    /// Load a control schema from JSON
    /// </summary>
    public static void LoadSchema(string json)
    {
        currentScheme = JsonConvert.DeserializeObject<Dictionary<string, Binding>>(json);
    }

    /// <summary>
    /// Update a binding within the current schema
    /// </summary>
    public static void UpdateBinding(string name, Binding binding)
    {
        currentScheme[name] = binding;
    }

    /// <summary>
    /// Serialize the current control scheme as JSON
    /// </summary>
    public static string ToJson()
    {
        return JsonConvert.SerializeObject(currentScheme);
    }
}

public struct Binding
{
    public XRController controller;
    public InputHelpers.Button? button;
    public InputHelpers.Axis2D? axis;

    public float deadzone;

    public static Dictionary<string, Binding> LoadFromJson(string json)
    {
        return JsonConvert.DeserializeObject<Dictionary<string, Binding>>(json);
    }
}
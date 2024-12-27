using System;
using System.Collections.Generic;
using CWVR.Assets;
using UnityEngine.InputSystem;
using Zorro.ControllerSupport;

namespace CWVR.Input;

public class Actions
{
    public static Actions Instance { get; private set; } = new();
    
    public InputAction HeadPosition { get; private set; }
    public InputAction HeadRotation { get; private set; }
    public InputAction HeadTrackingState { get; private set; }

    public InputAction LeftHandPosition { get; private set; }
    public InputAction LeftHandRotation { get; private set; }
    public InputAction LeftHandTrackingState { get; private set; }

    public InputAction RightHandPosition { get; private set; }
    public InputAction RightHandRotation { get; private set; }
    public InputAction RightHandTrackingState { get; private set; }

    private Actions()
    {
        HeadPosition = AssetManager.DefaultXRActions.FindAction("XR Tracking - Head/Position");
        HeadRotation = AssetManager.DefaultXRActions.FindAction("XR Tracking - Head/Rotation");
        HeadTrackingState = AssetManager.DefaultXRActions.FindAction("XR Tracking - Head/Tracking State");

        LeftHandPosition = AssetManager.DefaultXRActions.FindAction("XR Tracking - Left Hand/Position");
        LeftHandRotation = AssetManager.DefaultXRActions.FindAction("XR Tracking - Left Hand/Rotation");
        LeftHandTrackingState = AssetManager.DefaultXRActions.FindAction("XR Tracking - Left Hand/Tracking State");

        RightHandPosition = AssetManager.DefaultXRActions.FindAction("XR Tracking - Right Hand/Position");
        RightHandRotation = AssetManager.DefaultXRActions.FindAction("XR Tracking - Right Hand/Rotation");
        RightHandTrackingState = AssetManager.DefaultXRActions.FindAction("XR Tracking - Right Hand/Tracking State");
    }

    public InputAction this[string name] => InputHandler.Instance.m_playerInput.actions[name];
}

public static class InputActionExtensions
{
    private static Dictionary<InputAction, bool> floatCache = [];

    public static float ReadFloatThisFrame(this InputAction action, float deadzone = 0.75f)
    {
        var value = action.ReadValue<float>();
        var should = MathF.Abs(value) >= deadzone;

        try
        {
            if (should && !floatCache.GetValueOrDefault(action, false))
                return value < 0 ? -1 : 1;
            
            return 0f;
        }
        finally
        {
            floatCache[action] = should;
        }
    }
}
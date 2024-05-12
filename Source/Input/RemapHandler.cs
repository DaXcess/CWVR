using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace CWVR.Input;

public class RemapHandler : MonoBehaviour
{
    private RemapListenHandle currentListenHandle;

    public RemapListenHandle DetectBooleanControl(Action<Binding> onBindingSet)
    {
        currentListenHandle = new RemapListenHandle(onBindingSet, BindingType.BooleanControl);
        return currentListenHandle;
    }

    public RemapListenHandle DetectAxis2DControl(Action<Binding> onBindingSet)
    {
        currentListenHandle = new RemapListenHandle(onBindingSet, BindingType.Axis2D);
        return currentListenHandle;
    }

    private void Update()
    {
        if (currentListenHandle is null)
            return;

        switch (currentListenHandle.BindingType)
        {
            case BindingType.BooleanControl:
                if (DetectButtonControl() is { } buttonBinding)
                {
                    currentListenHandle.OnBindingSet(buttonBinding);
                    currentListenHandle = null;
                }

                break;

            case BindingType.Axis2D:
                if (DetectAxis2DControl(0.75f) is { } axisBinding)
                {
                    currentListenHandle.OnBindingSet(axisBinding);
                    currentListenHandle = null;
                }

                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static Binding? DetectButtonControl()
    {
        var controllers = (InputSystem.XRController[])Enum.GetValues(typeof(InputSystem.XRController));
        var buttons = (InputHelpers.Button[])Enum.GetValues(typeof(InputHelpers.Button));

        foreach (var controller in controllers)
        foreach (var button in buttons)
        {
            if (button is InputHelpers.Button.PrimaryTouch or InputHelpers.Button.SecondaryTouch
                or InputHelpers.Button.Primary2DAxisTouch or InputHelpers.Button.Secondary2DAxisTouch)
                continue;

            var device = controller.GetDevice();
            if (!device.isValid)
                continue;

            device.IsPressed(button, out var value, 0.75f);

            if (value)
            {
                return new Binding
                {
                    controller = controller,
                    button = button
                };
            }
        }

        return null;
    }

    private static Binding? DetectAxis2DControl(float deadzone)
    {
        var controllers = (InputSystem.XRController[])Enum.GetValues(typeof(InputSystem.XRController));
        var axes = (InputHelpers.Axis2D[])Enum.GetValues(typeof(InputHelpers.Axis2D));

        foreach (var controller in controllers)
        foreach (var axis in axes)
        {
            var device = controller.GetDevice();
            if (!device.isValid)
                continue;

            device.TryReadAxis2DValue(axis, out var value);

            if (Mathf.Abs(value.x) < deadzone)
                value.x = 0;
            if (Mathf.Abs(value.y) < deadzone)
                value.y = 0;

            if (value.x != 0 || value.y != 0)
            {
                return new Binding
                {
                    controller = controller,
                    axis = axis
                };
            }
        }

        return null;
    }
}

public class RemapListenHandle(Action<Binding> onBindingSet, BindingType type)
{
    public Action<Binding> OnBindingSet { get; } = onBindingSet;
    public BindingType BindingType { get; }= type;
}

public enum BindingType
{
    BooleanControl,
    Axis2D
}
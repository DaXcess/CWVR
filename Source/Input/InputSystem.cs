using System;
using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.OpenXR.Features.Interactions;

namespace CWVR.Input;

public class InputSystem : MonoBehaviour
{
    private readonly List<InputControl> controls = [];

    private bool initialized;
    private InputDevice leftController;
    private InputDevice rightController;

    private void Awake()
    {
        StartCoroutine(DetectControllers());
    }

    public static string DetectControllerProfile()
    {
        var leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        if (leftController.isValid)
            return DetectLayout(leftController);
        
        var rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        if (rightController.isValid)
            return DetectLayout(rightController);
        
        string DetectLayout(InputDevice device)
        {
            return device.name switch
            {
                OculusTouchControllerProfile.kDeviceLocalizedName or HPReverbG2ControllerProfile.kDeviceLocalizedName
                    or ValveIndexControllerProfile.kDeviceLocalizedName
                    or MetaQuestTouchProControllerProfile.kDeviceLocalizedName
                    or KHRSimpleControllerProfile.kDeviceLocalizedName => "default",
                HTCViveControllerProfile.kDeviceLocalizedName => "htc_vive",
                MicrosoftMotionControllerProfile.kDeviceLocalizedName => "wmr",
                _ => "default"
            };
        }
        
        return "default";
    }
    
    private IEnumerator DetectControllers()
    {
        while (true)
        {
            leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);

            if (!leftController.isValid || !rightController.isValid)
            {
                yield return null;
                continue;
            }

            initialized = true;

            break;
        }
    }

    private void Update()
    {
        if (!initialized)
            return;

        controls.Do(ctrl => ctrl.Update());
    }

    private InputDevice GetController(XRController controller)
    {
        return controller switch
        {
            XRController.Left => leftController,
            XRController.Right => rightController,
            _ => throw new ArgumentOutOfRangeException(nameof(controller), controller, null)
        };
    }

    public ButtonControl RegisterButtonControl(XRController controller, InputHelpers.Button button,
        float deadzone = 0.75f)
    {
        var control = new ButtonControl(this, controller, button, deadzone);
        controls.Add(control);

        return control;
    }

    public Axis2DControl RegisterAxis2DControl(XRController controller, InputHelpers.Axis2D axis, float deadzone = 0)
    {
        var control = new Axis2DControl(this, controller, axis, deadzone);
        controls.Add(control);

        return control;
    }

    public void ClearBindings()
    {
        controls.Clear();
    }

    public class ButtonControl(
        InputSystem inputSystem,
        XRController controller,
        InputHelpers.Button button,
        float deadzone)
        : InputControl
    {
        private bool isPressed;
        private bool wasPressed;

        public bool Pressed()
        {
            return isPressed;
        }

        public bool PressedDown()
        {
            return isPressed && !wasPressed;
        }

        public bool Released()
        {
            return !isPressed && wasPressed;
        }

        public void Update()
        {
            inputSystem.GetController(controller).IsPressed(button, out var value, deadzone);
            wasPressed = isPressed;
            isPressed = value;
        }
    }

    public class Axis2DControl(
        InputSystem inputSystem,
        XRController controller,
        InputHelpers.Axis2D axis,
        float deadzone = 0f) : InputControl
    {
        private Vector2 value;

        public Vector2 GetValue()
        {
            return value;
        }

        public void Update()
        {
            inputSystem.GetController(controller).TryReadAxis2DValue(axis, out value);

            if (Mathf.Abs(value.x) < deadzone)
                value.x = 0;
            if (Mathf.Abs(value.y) < deadzone)
                value.y = 0;
        }
    }

    private interface InputControl
    {
        public void Update();
    }

    public enum XRController
    {
        Left,
        Right
    }
}
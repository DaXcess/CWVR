using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

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

    public ButtonControl RegisterButtonControl(XRController controller, InputHelpers.Button button)
    {
        var control = new ButtonControl(this, controller, button);
        controls.Add(control);

        return control;
    }

    public Axis2DControl RegisterAxis2DControl(XRController controller, InputHelpers.Axis2D axis, float deadzone = 0)
    {
        var control = new Axis2DControl(this, controller, axis, deadzone);
        controls.Add(control);

        return control;
    }

    public DirectionalAxisControl RegisterDirectionalAxisControl(XRController controller, InputHelpers.Axis2D axis, DirectionalAxisControl.AxisDirection direction, float deadzone = 0.75f)
    {
        var control = new DirectionalAxisControl(this, controller, axis, direction, deadzone);
        controls.Add(control);

        return control;
    }

    public class ButtonControl(InputSystem inputSystem, XRController controller, InputHelpers.Button button) : InputControl
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
            inputSystem.GetController(controller).TryReadSingleValue(button, out var value);
            wasPressed = isPressed;
            isPressed = value != 0f;
        }
    }
    
    public class Axis2DControl(InputSystem inputSystem, XRController controller, InputHelpers.Axis2D axis, float deadzone = 0f) : InputControl
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

    public class DirectionalAxisControl(
        InputSystem inputSystem,
        XRController controller,
        InputHelpers.Axis2D axis,
        DirectionalAxisControl.AxisDirection axisDirection,
        float deadzone = 0.75f) : InputControl
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
        
        public void Update()
        {
            inputSystem.GetController(controller).TryReadAxis2DValue(axis, out var value);
            wasPressed = isPressed;
            isPressed = axisDirection switch
            {
                AxisDirection.Up => value.y > deadzone,
                AxisDirection.Down => value.y < -deadzone,
                AxisDirection.Left => value.x < -deadzone,
                AxisDirection.Right => value.x > deadzone,
                _ => throw new ArgumentOutOfRangeException(nameof(axisDirection), axisDirection, null)
            };
        }

        public enum AxisDirection
        {
            Up,
            Down,
            Left,
            Right
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
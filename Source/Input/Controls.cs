using System.Linq;
using HarmonyLib;
using UnityEngine;
using Axis2DControl = CWVR.Input.InputSystem.Axis2DControl;
using ButtonControl = CWVR.Input.InputSystem.ButtonControl;

namespace CWVR.Input;

public class Controls(InputSystem inputSystem)
{
    public Axis2DControl Movement { get; private set; }
    public ButtonControl Jump { get; private set; }
    public ButtonControl Sprint { get; private set; }
    public ButtonControl Crouch { get; private set; }
    public ButtonControl Interact { get; private set; }
    public ButtonControl Use { get; private set; }
    public ButtonControl Drop { get; private set; }
    public ButtonControl FlipCamera { get; private set; }
    public ButtonControl Menu { get; private set; }
    public ButtonControl ResetHeight { get; private set; }
    public ButtonControl ZoomIn { get; private set; }
    public ButtonControl ZoomOut { get; private set; }

    // Turning

    public ButtonControl TurnLeft { get; private set; }
    public ButtonControl TurnRight { get; private set; }

    // Modal

    public ButtonControl ModalLeft { get; private set; }
    public ButtonControl ModalRight { get; private set; }
    public ButtonControl ModalPress { get; private set; }

    // Spectating

    public Axis2DControl Pivot { get; private set; }
    public ButtonControl SpectateNext { get; private set; }
    public ButtonControl SpectatePrevious { get; private set; }

    public static string[] ControlNames => AccessTools.GetDeclaredProperties(typeof(Controls))
        .Where(prop => prop.PropertyType == typeof(ButtonControl) || prop.PropertyType == typeof(Axis2DControl))
        .Select(prop => prop.Name).ToArray();
    
    private bool toggleSprintState;

    public void SampleInput(global::Player.PlayerInput input, global::Player.PlayerData data, global::Player player)
    {
        input.ResetInput();
        input.movementInput = Vector2.zero;

        if (Modal.Instance.m_show || EscapeMenu.Instance.Open)
            return;

        if (!player.HasLockedMovement())
            input.movementInput = Movement.GetValue();

        if (data.inputOverideAmount > 0.01f)
            input.movementInput =
                Vector2.Lerp(input.movementInput, data.overrideMovementInput, data.inputOverideAmount);

        input.escapeWasPressed = Menu.PressedDown();

        if (data.inputOverideAmount > 0.99f)
            return;

        if (!player.HasLockedMovement())
        {
            if (Plugin.Config.ToggleSprint.Value)
            {
                if (Sprint.PressedDown())
                    toggleSprintState = !toggleSprintState;

                if (player.data.staminaDepleated ||
                    (input.movementInput.y <= 0.1f && !player.refs.controller.canSprintInAnyDirection))
                    toggleSprintState = false;

                input.sprintIsPressed = toggleSprintState;
            }
            else
            {
                input.sprintIsPressed = Sprint.Pressed();
            }
        }

        if (data.cantUseItemFor <= 0f)
        {
            input.clickWasPressed = Use.PressedDown();
            input.clickIsPressed = Use.Pressed();
            input.clickWasReleased = Use.Released();
        }

        if (!player.HasLockedMovement())
        {
            input.jumpWasPressed = Jump.PressedDown();
            input.jumpIsPressed = Jump.Pressed();
            input.crouchWasPressed = Crouch.PressedDown();
            input.crouchIsPressed = Crouch.Pressed();
        }

        input.interactWasPressed = Interact.PressedDown();
        input.dropItemWasPressed = Drop.PressedDown();
        input.dropItemWasReleased = Drop.Released();
        input.dropItemIsPressed = Drop.Pressed();
        input.toggleCameraFlipWasPressed = FlipCamera.PressedDown();
    }

    public void ReloadBindings()
    {
        inputSystem.ClearBindings();

        foreach (var (name, binding) in ControlScheme.Scheme)
        {
            if (AccessTools.Property(typeof(Controls), name) is not { } property) continue;
            
            if (property.PropertyType == typeof(Axis2DControl) && binding.axis is not null)
            {
                property.SetValue(this,
                    inputSystem.RegisterAxis2DControl(binding.controller, binding.axis.Value, binding.deadzone));
            }
            else if (property.PropertyType == typeof(ButtonControl) && binding.button is not null)
            {
                property.SetValue(this,
                    inputSystem.RegisterButtonControl(binding.controller, binding.button.Value, binding.deadzone));
            }
        }
    }
}
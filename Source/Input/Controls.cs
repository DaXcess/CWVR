using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Axis2DControl = CWVR.Input.InputSystem.Axis2DControl;
using ButtonControl = CWVR.Input.InputSystem.ButtonControl;
using XRController = CWVR.Input.InputSystem.XRController;
using DirectionalAxisControl = CWVR.Input.InputSystem.DirectionalAxisControl;

namespace CWVR.Input;

// TODO: Configurable?
public class Controls(InputSystem inputSystem)
{
    public Axis2DControl Movement { get; } =
        inputSystem.RegisterAxis2DControl(XRController.Left, InputHelpers.Axis2D.PrimaryAxis2D);

    public ButtonControl Jump { get; } =
        inputSystem.RegisterButtonControl(XRController.Right, InputHelpers.Button.PrimaryButton);

    public ButtonControl Sprint { get; } =
        inputSystem.RegisterButtonControl(XRController.Left, InputHelpers.Button.Primary2DAxisClick);

    public ButtonControl Crouch { get; } =
        inputSystem.RegisterButtonControl(XRController.Right, InputHelpers.Button.Primary2DAxisClick);

    public ButtonControl Interact { get; } =
        inputSystem.RegisterButtonControl(XRController.Right, InputHelpers.Button.GripButton);

    public ButtonControl Use { get; } =
        inputSystem.RegisterButtonControl(XRController.Right, InputHelpers.Button.TriggerButton);

    public ButtonControl Drop { get; } =
        inputSystem.RegisterButtonControl(XRController.Right, InputHelpers.Button.SecondaryButton);

    public ButtonControl FlipCamera { get; } =
        inputSystem.RegisterButtonControl(XRController.Left, InputHelpers.Button.GripButton);

    public ButtonControl Menu { get; } =
        inputSystem.RegisterButtonControl(XRController.Left, InputHelpers.Button.PrimaryButton);

    public ButtonControl ResetHeight { get; } =
        inputSystem.RegisterButtonControl(XRController.Left, InputHelpers.Button.SecondaryButton);

    public DirectionalAxisControl ZoomIn { get; } = inputSystem.RegisterDirectionalAxisControl(XRController.Right,
        InputHelpers.Axis2D.PrimaryAxis2D, DirectionalAxisControl.AxisDirection.Up);

    public DirectionalAxisControl ZoomOut { get; } = inputSystem.RegisterDirectionalAxisControl(XRController.Right,
        InputHelpers.Axis2D.PrimaryAxis2D, DirectionalAxisControl.AxisDirection.Down);


    // Turning

    public DirectionalAxisControl TurnLeft { get; } =
        inputSystem.RegisterDirectionalAxisControl(XRController.Right, InputHelpers.Axis2D.PrimaryAxis2D,
            DirectionalAxisControl.AxisDirection.Left);

    public DirectionalAxisControl TurnRight { get; } =
        inputSystem.RegisterDirectionalAxisControl(XRController.Right, InputHelpers.Axis2D.PrimaryAxis2D,
            DirectionalAxisControl.AxisDirection.Right);

    // Modal

    public DirectionalAxisControl ModalLeft { get; } = inputSystem.RegisterDirectionalAxisControl(XRController.Left,
        InputHelpers.Axis2D.PrimaryAxis2D, DirectionalAxisControl.AxisDirection.Left);

    public DirectionalAxisControl ModalRight { get; } = inputSystem.RegisterDirectionalAxisControl(XRController.Left,
        InputHelpers.Axis2D.PrimaryAxis2D, DirectionalAxisControl.AxisDirection.Right);

    public ButtonControl ModalPress { get; } = inputSystem.RegisterButtonControl(XRController.Right,
        InputHelpers.Button.PrimaryButton);

    // Spectating

    public Axis2DControl Pivot { get; } =
        inputSystem.RegisterAxis2DControl(XRController.Left, InputHelpers.Axis2D.PrimaryAxis2D);

    public ButtonControl SpectateNext { get; } =
        inputSystem.RegisterButtonControl(XRController.Right, InputHelpers.Button.TriggerButton);

    public ButtonControl SpectatePrevious { get; } =
        inputSystem.RegisterButtonControl(XRController.Left, InputHelpers.Button.TriggerButton);

    private bool toggleSprintState;
    private float timeNotMoved;

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

        if (input.movementInput == Vector2.zero)
            timeNotMoved += Time.deltaTime;
        else
            timeNotMoved = 0;

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
                    (input.movementInput.y <= 0.1f && !player.refs.controller.canSprintInAnyDirection) ||
                    timeNotMoved > Plugin.Config.ToggleSprintTimer.Value)
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
}
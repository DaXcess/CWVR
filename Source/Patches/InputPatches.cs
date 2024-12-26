using CWVR.Assets;
using CWVR.Input;
using HarmonyLib;
using UnityEngine.InputSystem;
using Zorro.ControllerSupport;

namespace CWVR.Patches;

[CWVRPatch]
public static class InputPatches
{
    /// <summary>
    /// Replace the content warning inputs with VR inputs
    /// </summary>
    [HarmonyPatch(typeof(InputHandler), nameof(InputHandler.OnCreated))]
    [HarmonyPostfix]
    private static void OnCreateInputHandler(InputHandler __instance)
    {
        var playerInput = __instance.m_playerInput;

        playerInput.actions = AssetManager.InputActions;
        playerInput.defaultActionMap = "Player";
        playerInput.neverAutoSwitchControlSchemes = false;
        playerInput.notificationBehavior = PlayerNotifications.InvokeCSharpEvents;

        playerInput.enabled = false;
        playerInput.enabled = true;

        __instance.gameObject.AddComponent<RemapManager>();
    }

    /// <summary>
    /// Force the game to believe we still use M+K since VR menu navigation is made for M+K layouts
    /// </summary>
    [HarmonyPatch(typeof(InputHandler), nameof(InputHandler.GetCurrentUsedInputScheme))]
    [HarmonyPrefix]
    private static bool ForceInputScheme(ref InputScheme __result)
    {
        __result = InputScheme.KeyboardMouse;

        return false;
    }

    /// <summary>
    /// Replace player input references with VR input references
    /// </summary>
    [HarmonyPatch(typeof(global::Player.PlayerInput), nameof(global::Player.PlayerInput.Initialize))]
    [HarmonyPostfix]
    private static void OnPlayerInputInitialize(global::Player.PlayerInput __instance)
    {
        var playerInput = InputHandler.Instance.m_playerInput;

        playerInput.ActivateInput();
        
        __instance.moveAction = InputActionReference.Create(playerInput.actions["Player/Move"]);
        __instance.jumpAction = InputActionReference.Create(playerInput.actions["Player/Jump"]);
        __instance.sprintAction = InputActionReference.Create(playerInput.actions["Player/Sprint"]);
        __instance.crouchAction = InputActionReference.Create(playerInput.actions["Player/Crouch"]);
        __instance.interactAction = InputActionReference.Create(playerInput.actions["Player/Interact"]);
        __instance.dropItemAction = InputActionReference.Create(playerInput.actions["Player/DropItem"]);
        __instance.selfieModeAction = InputActionReference.Create(playerInput.actions["Player/SelfieMode"]);
        __instance.itemClickAction = InputActionReference.Create(playerInput.actions["Player/Click"]);
        __instance.itemRightClickAction = InputActionReference.Create(playerInput.actions["Player/Aim"]);
        __instance.menuBackAction = InputActionReference.Create(playerInput.actions["Player/MenuBack"]);
        __instance.emoteAction = InputActionReference.Create(playerInput.actions["Player/Emote"]);
    }

    /// <summary>
    /// Since we're forcing M+K layout, we'll have to manually modify sprint values
    /// </summary>
    [HarmonyPatch(typeof(global::Player.PlayerInput), nameof(global::Player.PlayerInput.SampeInput))]
    [HarmonyPostfix]
    private static void InjectSprintValue(global::Player.PlayerInput __instance)
    {
        if (!Plugin.Config.ToggleSprint.Value)
        {
            __instance.sprintIsPressed =
                GlobalInputHandler.CanTakeInput() && __instance.sprintAction.action.IsPressed();
        }
        else if (GlobalInputHandler.CanTakeInput())
        {
            if (!__instance.isControllerSprinting)
                __instance.isControllerSprinting = __instance.sprintAction.action.WasPressedThisFrame();

            __instance.sprintIsPressed = __instance.isControllerSprinting;
        }
        else
        {
            __instance.isControllerSprinting = false;
        }
    }
}
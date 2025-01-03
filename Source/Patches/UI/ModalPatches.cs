using System.Linq;
using CWVR.Input;
using CWVR.Player;
using HarmonyLib;
using UnityEngine.UI;

namespace CWVR.Patches.UI;

[CWVRPatch]
internal static class ModalPatches
{
    private static EscapeMenuButton[] previousButtons;
    private static EscapeMenuButton[] buttons;
    private static int buttonIndex;

    /// <summary>
    /// Store previous buttons so that we don't accidentally count them towards the total buttons
    /// </summary>
    [HarmonyPatch(typeof(Modal), nameof(Modal.ShowModal))]
    [HarmonyPrefix]
    private static void BeforeShowModal(Modal __instance)
    {
        if (!VRSession.Instance)
            return;

        previousButtons = __instance.GetComponentsInChildren<EscapeMenuButton>();
    }

    /// <summary>
    /// Make sure every component of the modal UI is rendered on top
    /// </summary>
    [HarmonyPatch(typeof(Modal), nameof(Modal.ShowModal))]
    [HarmonyPostfix]
    private static void OnShowModal(Modal __instance)
    {
        if (!VRSession.Instance)
            return;

        __instance.gameObject.SetLayerRecursive(6);

        buttons = __instance.GetComponentsInChildren<EscapeMenuButton>().Where(btn => !previousButtons.Contains(btn))
            .ToArray();
        buttonIndex = 0;

        buttons[0].OnSelected();
    }

    [HarmonyPatch(typeof(Modal), nameof(Modal.Update))]
    [HarmonyPostfix]
    private static void OnUpdate(Modal __instance)
    {
        if (!VRSession.Instance)
            return;

        if (!__instance.m_show)
            return;

        if (Actions.Instance["ModalPress"].WasPressedThisFrame())
        {
            buttons[buttonIndex].GetComponent<Button>().onClick?.Invoke();
            return;
        }

        switch (Actions.Instance["ModalNavigate"].ReadFloatThisFrame())
        {
            case < 0:
                buttons[buttonIndex].OnDeselect();
                buttonIndex = (buttonIndex - 1 + buttons.Length) % buttons.Length;
                buttons[buttonIndex].OnSelected();
                break;
            case > 0:
                buttons[buttonIndex].OnDeselect();
                buttonIndex = (buttonIndex + 1 + buttons.Length) % buttons.Length;
                buttons[buttonIndex].OnSelected();
                break;
        }
    }
}
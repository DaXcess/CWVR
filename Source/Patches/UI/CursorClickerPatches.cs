using System.Collections.Generic;
using System.Reflection.Emit;
using CWVR.Player;
using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using static HarmonyLib.AccessTools;

namespace CWVR.Patches.UI;

[CWVRPatch]
internal static class CursorClickerPatches
{
    [HarmonyPatch(typeof(CursorClicker), nameof(CursorClicker.LateUpdate))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> CursorClickerTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions)
            .Advance(1)
            .RemoveInstructions(8)
            // Make the cursor follow the primary interactor instead of the main camera (head)
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(VRSession), nameof(VRSession.Instance))),
                new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(VRSession), nameof(VRSession.LocalPlayer))),
                new CodeInstruction(OpCodes.Callvirt,
                    PropertyGetter(typeof(VRPlayer), nameof(VRPlayer.PrimaryInteractor))),
                new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(Component), nameof(Component.transform))),
                new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(Transform), nameof(Transform.position))),
                new CodeInstruction(OpCodes.Stloc_0),
            
                new CodeInstruction(OpCodes.Call, PropertyGetter(typeof(VRSession), nameof(VRSession.Instance))),
                new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(VRSession), nameof(VRSession.LocalPlayer))),
                new CodeInstruction(OpCodes.Callvirt,
                    PropertyGetter(typeof(VRPlayer), nameof(VRPlayer.PrimaryInteractor))),
                new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(Component), nameof(Component.transform))),
                new CodeInstruction(OpCodes.Callvirt, PropertyGetter(typeof(Transform), nameof(Transform.up))),
                new CodeInstruction(OpCodes.Stloc_1)
            )
            .MatchForward(false, [new CodeMatch(OpCodes.Ldc_I4, 0x143)])
            .Advance(5);

        var falseAddr = matcher.Operand;

        // Use PlayerInput inputs instead of hardcoded mouse/keyboard inputs
        matcher.Advance(-5).RemoveInstructions(6).InsertAndAdvance([
                new CodeInstruction(OpCodes.Ldsfld, Field(typeof(global::Player), nameof(global::Player.localPlayer))),
                new CodeInstruction(OpCodes.Ldfld, Field(typeof(global::Player), nameof(global::Player.input))),
                new CodeInstruction(OpCodes.Ldfld,
                    Field(typeof(global::Player.PlayerInput), nameof(global::Player.PlayerInput.interactWasPressed))),
                new CodeInstruction(OpCodes.Brfalse_S, falseAddr)
            ]).MatchForward(false,
            [
                new CodeMatch(OpCodes.Call,
                    PropertyGetter(typeof(UnityEngine.Input), nameof(UnityEngine.Input.mousePosition)))
            ]).RemoveInstructions(1)
            // Use cursorPos to detect clickable elements instead of mouse location on screen
            .InsertAndAdvance([
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, Field(typeof(CursorClicker), nameof(CursorClicker.cursorPos)))
            ]).SetOperandAndAdvance(Method(typeof(CursorClickerPatches), nameof(GetUiUnderPosVR)));

        return matcher.InstructionEnumeration();
    }

    private static bool GetUiUnderPosVR(EventSystem me, out List<RaycastResult> hits, Vector3 position)
    {
        var screenPosition = MainCamera.instance.cam.WorldToScreenPoint(position);
        var ed = new PointerEventData(me)
        {
#pragma warning disable Harmony003
            position = screenPosition
#pragma warning restore Harmony003
        };
        
        hits = [];
        
        me.RaycastAll(ed, hits);

        return hits.Count > 0;
    }
}
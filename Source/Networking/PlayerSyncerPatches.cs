using System.Collections.Generic;
using System.Reflection.Emit;
using CWVR.Patches;
using CWVR.Player;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;
using Zorro.Core.Serizalization;
using static HarmonyLib.AccessTools;

namespace CWVR.Networking;

[CWVRPatch]
internal static class PlayerSyncerPatches
{
    /// <summary>
    /// Translate VR Camera into player look values for flatscreen players during sync
    /// </summary>
    [HarmonyPatch(typeof(PlayerSyncer), nameof(PlayerSyncer.OnPhotonSerializeView))]
    [HarmonyPrefix]
    private static void BeforeSerializeView(PlayerSyncer __instance, PhotonStream stream)
    {
        // Skip if not local player or VR is not yet ready
        if (!__instance.player.IsLocal || !VRSession.Instance)
            return;

        // We only care when we're writing data
        if (!stream.IsWriting)
            return;

        if (__instance.player.data.dead || __instance.player.data.currentBed is not null)
            return;
        
        // Reverse the camera rotation to playerLookValues for syncing to the network
        // ChatGPT did this (and it works, surprisingly), cuz I ain't a mathematician
        var forward = VRSession.Instance.MainCamera.transform.forward;
        var yaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
        var pitch = Mathf.Asin(forward.y) * Mathf.Rad2Deg;
        
        __instance.player.data.playerLookValues = new Vector2(yaw, pitch);
    }

    /// <summary>
    /// Inject a call to <see cref="SerializeAdditionalData"/> right before sending to inject additional data during sync
    /// </summary>
    [HarmonyPatch(typeof(PlayerSyncer), nameof(PlayerSyncer.OnPhotonSerializeView))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> InjectAdditionalDataPatch(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
            [
                new CodeMatch(OpCodes.Callvirt,
                    PropertyGetter(typeof(BinarySerializer), nameof(BinarySerializer.buffer)))
            ])
            .Advance(-1)
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Dup),
                new CodeInstruction(OpCodes.Call, Method(typeof(PlayerSyncerPatches), nameof(SerializeAdditionalData)))
            )
            .InstructionEnumeration();
    }

    /// <summary>
    /// Additional player data to synchronize
    /// </summary>
    private static void SerializeAdditionalData(BinarySerializer serializer)
    {
        if (!VRSession.Instance)
            return;

        var hip = global::Player.localPlayer.refs.ragdoll.GetBodypart(BodypartType.Hip);
        var refs = global::Player.localPlayer.refs;
        var leftHandPos = refs.IK_Hand_L.position;
        var rightHandPos = refs.IK_Hand_R.position;

        leftHandPos = hip.animationTarget.transform.InverseTransformPoint(leftHandPos);
        leftHandPos = hip.rig.transform.TransformPoint(leftHandPos);
        
        rightHandPos = hip.animationTarget.transform.InverseTransformPoint(rightHandPos);
        rightHandPos = hip.rig.transform.TransformPoint(rightHandPos);
        
        var leftHandRot = refs.IK_Hand_L.rotation;
        var rightHandRot = refs.IK_Hand_R.rotation;
        
        // We're in VR
        serializer.WriteBool(true);
        
        // Left hand
        serializer.WriteFloat3(leftHandPos);
        serializer.WriteFloat3(leftHandRot.eulerAngles);

        // Right hand
        serializer.WriteFloat3(rightHandPos);
        serializer.WriteFloat3(rightHandRot.eulerAngles);
    }
}

[CWVRPatch(CWVRPatchTarget.Universal)]
internal static class UniversalPlayerSyncerPatches
{
    /// <summary>
    /// Inject a call to <see cref="DeserializeAdditionalData"/> right before disposing to read additional data after sync
    /// </summary>
    [HarmonyPatch(typeof(PlayerSyncer), nameof(PlayerSyncer.OnPhotonSerializeView))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> InjectReadAdditionalDataPatch(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(false,
            [
                new CodeMatch(OpCodes.Callvirt, Method(typeof(BinaryDeserializer), nameof(BinaryDeserializer.Dispose)))
            ])
            .Advance(-1)
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldloc_S, (byte)4),
                new CodeInstruction(OpCodes.Call,
                    Method(typeof(UniversalPlayerSyncerPatches), nameof(DeserializeAdditionalData)))
            )
            .InstructionEnumeration();
    }

    private static void DeserializeAdditionalData(PlayerSyncer syncer, BinaryDeserializer deserializer)
    {
        // No additional data to deserialize
        if (deserializer.position == deserializer.buffer.Length)
            return;

        // Not enough data to deserialize
        if (deserializer.buffer.Length - deserializer.position < 25)
            return;

        if (!deserializer.ReadBool())
            return;
        
        if (!VRSession.Instance.NetworkManager.TryGetNetworkPlayer(syncer.player, out var player))
            player = VRSession.Instance.NetworkManager.RegisterVRPlayer(syncer.player);
        
        player.DeserializeRig(deserializer);
    }
}
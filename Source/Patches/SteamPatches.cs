using HarmonyLib;
using Steamworks;

namespace CWVR.Patches;

[CWVRPatch]
internal static class SteamPatches
{
    [HarmonyPatch(typeof(SteamLobbyHandler), nameof(SteamLobbyHandler.InviteScreen))]
    [HarmonyPrefix]
    private static bool InviteScreenPatch(SteamLobbyHandler __instance)
    {
        if (OpenXR.GetActiveRuntimeName(out var name) && name is "SteamVR" or "SteamVR/OpenXR")
            SteamFriends.ActivateGameOverlay("friends");
        else
            Modal.Show("Invite from PC",
                "You must go to your PC and manually invite people into your game.\nInviting through VR is not supported.",
                [new ModalOption("Ok")]);
        
        return false;
    }
}
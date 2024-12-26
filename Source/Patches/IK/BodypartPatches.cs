using CWVR.Player;
using HarmonyLib;

namespace CWVR.Patches.IK;

[CWVRPatch(CWVRPatchTarget.Universal)]
internal static class BodypartPatches
{
    // TODO: I really want a better solution for this
    // Just setting very high forces is not really a desirable solution
    // I'd rather just have the hand tracking be instantaneous, while also still keeping their physics
    // **and** also not overshoot the position/rotation constraints
    // (So not being able to clip through walls and not detaching from the body)
    
    [HarmonyPatch(typeof(Bodypart), nameof(Bodypart.FollowAnimJointVel))]
    [HarmonyPrefix]
    private static void OnFollowAnimJointVel(Bodypart __instance, ref float force)
    {
        if (!VRSession.InVR && !VRSession.Instance.NetworkManager.InVR(__instance.player))
            return;
        
        if (__instance.bodypartType != BodypartType.Hand_L && __instance.bodypartType != BodypartType.Hand_R)
            return;
    
        force = 900f;
    }
    
    [HarmonyPatch(typeof(Bodypart), nameof(Bodypart.FollowAnimJointAngularVel))]
    [HarmonyPrefix]
    private static bool OnFollowAnimJointAngularVel(Bodypart __instance, ref float torque)
    {
        if (!VRSession.InVR && !VRSession.Instance.NetworkManager.InVR(__instance.player))
            return true;
        
        if (__instance.bodypartType != BodypartType.Hand_L && __instance.bodypartType != BodypartType.Hand_R)
            return true;

        __instance.rig.transform.rotation = __instance.animationTarget.transform.rotation;

        return false;
    }
}
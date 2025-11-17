using System;
using CWVR.Experiments;
using CWVR.UI;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace CWVR.MultiLoader.Native;

#if NATIVE

[ContentWarningPlugin(Plugin.PLUGIN_GUID, Plugin.PLUGIN_VERSION, true)]
[UsedImplicitly]
public class NativeMod
{
    private static readonly Harmony early = new("io.daxcess.cwvr-early");

    static NativeMod()
    {
        if (CheckBepInExDuplicate())
        {
            Debug.LogWarning("CWVR is already loaded by BepInEx");

            Modal.Show("CW // VR duplicate install",
                "CW // VR has been installed through both BepInEx *and* the Steam Workshop.\nThe settings for the VR mod will show twice and might not work properly.\nIt is recommended to only use either the BepInEx mod or the Steam Workshop.",
                [new ModalOption("OK")]);

            return;
        }

        Plugin.Logger = new Logger();
        Plugin.Config = new Config();
        Plugin.Loader = Loader.Native;

        Plugin.SetupEarlyDependencies();
        
        early.Patch(AccessTools.Method(typeof(RichPresenceHandler), nameof(RichPresenceHandler.Initialize)),
            postfix: new HarmonyMethod(((Action)OnGameInitialize).Method));
    }

    /// <summary>
    /// CWVR requires access to the game's settings handler, which isn't initialized during the static constructor
    ///
    /// We initialize CWVR during the RichPresenceHandler setup since that runs after the settings have been created
    /// </summary>
    private static void OnGameInitialize()
    {
        Plugin.InitializePlugin();
        early.UnpatchSelf();
    }

    private static bool CheckBepInExDuplicate()
    {
        try
        {
            return CheckBepInExDuplicateInner();
        }
        catch
        {
            return false;
        }
    }

    private static bool CheckBepInExDuplicateInner()
    {
        return global::BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(Plugin.PLUGIN_GUID);
    }
}

#endif
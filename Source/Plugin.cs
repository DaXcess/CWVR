﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using CWVR.Assets;
using CWVR.Patches;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;

namespace CWVR;

[ContentWarningPlugin(PLUGIN_GUID, PLUGIN_VERSION, true)]
[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private const string PLUGIN_GUID = "io.daxcess.cwvr";
    private const string PLUGIN_NAME = "CWVR";
    private const string PLUGIN_VERSION = "0.0.1";
    
    private const string BANNER = "                             ,--.,--.                         \n ,-----.,--.   ,--.         /  //  /     ,--.   ,--.,------.  \n'  .--./|  |   |  |        /  //  /       \\  `.'  / |  .--. ' \n|  |    |  |.'.|  |       /  //  /         \\     /  |  '--'.' \n'  '--'\\|   ,'.   |      /  //  /           \\   /   |  |\\  \\  \n `-----''--'   '--'     /  //  /             `-'    `--' '--' \n                       `--'`--'                               \n\n             ___________________________ \n            < Another VR mod by DaXcess >\n             --------------------------- \n                    \\   ^__^\n                     \\  (oo)\\_______\n                        (__)\\       )\\/\\\n                            ||----w |\n                            ||     ||\n";

    public new static Config Config { get; private set; }
    public static Flags Flags { get; private set; } = 0;

    private void Awake()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        CWVR.Logger.SetSource(Logger);

        Config = new Config(base.Config);
        
        foreach (var line in BANNER.Split('\n'))
            Logger.LogInfo($"   {line}");

        var disableVr = Config.DisableVR.Value || Environment.GetCommandLineArgs()
            .Contains("--disable-vr", StringComparer.InvariantCultureIgnoreCase);
        
        if (disableVr)
            Logger.LogWarning("VR has been disabled by config or the `--disable-vr` command line flag");
        
        if (!LoadEarlyRuntimeDependencies())
        {
            Logger.LogError("Disabling mod because required runtime dependencies could not be loaded!");
            return;
        }

        if (!AssetManager.LoadAssets())
        {
            Logger.LogError("Disabling mod because assets could not be loaded!");
            // return;
        }

        HarmonyPatcher.PatchUniversal();
        Logger.LogInfo("Inserted Universal patches using Harmony");

        if (disableVr || !InitializeVR())
            return;

        // Perform global VR setup here

        Flags |= Flags.VR;
    }

    private bool LoadEarlyRuntimeDependencies()
    {
        try
        {
            var current = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (current is null)
                throw new Exception("Failed to get assembly location");

            foreach (var file in Directory.GetFiles(Path.Combine(current, "RuntimeDeps"), "*.dll"))
            {
                var filename = Path.GetFileName(file);

                // Ignore known unmanaged libraries
                if (filename is "UnityOpenXR.dll" or "openxr_loader.dll")
                    continue;

                Logger.LogDebug($"Early loading {filename}");

                try
                {
                    Assembly.LoadFile(file);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning($"Failed to early load {filename}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(
                $"Unexpected error occured while loading early runtime dependencies (incorrect folder structure?): {ex.Message}");
            return false;
        }

        return true;
    }

    private bool InitializeVR()
    {
        Logger.LogInfo("Loading VR...");
        
        SetupRuntimeAssets(out var mustRestart);
        if (mustRestart)
        {
            Logger.LogError("You must restart the game to allow VR to function properly");
            Flags |= Flags.RestartRequired;
            
            return false;
        }

        EnableControllerProfiles();
        InitializeXRRuntime();

        if (!StartDisplay())
        {
            Logger.LogError("Failed to start in VR Mode! Only Non-VR features are available!");
            return false;
        }
        
        HarmonyPatcher.PatchVR();
        Logger.LogInfo("Inserted VR patches using Harmony");
        
        return true;
    }

    /// <summary>
    /// Loads controller profiles provided by Unity into OpenXR, which will enable controller support.
    /// By default, only the HMD input profile is loaded.
    /// </summary>
    private void EnableControllerProfiles()
    {
        var valveIndex = ScriptableObject.CreateInstance<ValveIndexControllerProfile>();
        var hpReverb = ScriptableObject.CreateInstance<HPReverbG2ControllerProfile>();
        var htcVive = ScriptableObject.CreateInstance<HTCViveControllerProfile>();
        var mmController = ScriptableObject.CreateInstance<MicrosoftMotionControllerProfile>();
        var khrSimple = ScriptableObject.CreateInstance<KHRSimpleControllerProfile>();
        var metaQuestTouch = ScriptableObject.CreateInstance<MetaQuestTouchProControllerProfile>();
        var oculusTouch = ScriptableObject.CreateInstance<OculusTouchControllerProfile>();

        valveIndex.enabled = true;
        hpReverb.enabled = true;
        htcVive.enabled = true;
        mmController.enabled = true;
        khrSimple.enabled = true;
        metaQuestTouch.enabled = true;
        oculusTouch.enabled = true;
        
        // This feature list is empty by default if the game isn't a VR game
        OpenXRSettings.Instance.features =
        [
            valveIndex,
            hpReverb,
            htcVive,
            mmController,
            khrSimple,
            metaQuestTouch,
            oculusTouch
        ];
        
        Logger.LogDebug("Enabled XR Controller Profiles");
    }

    /// <summary>
    /// Attempt to start the OpenXR runtime.
    /// </summary>
    private void InitializeXRRuntime()
    {
        // Set up the OpenXR loader
        var generalSettings = ScriptableObject.CreateInstance<XRGeneralSettings>();
        var managerSettings = ScriptableObject.CreateInstance<XRManagerSettings>();
        var xrLoader = ScriptableObject.CreateInstance<OpenXRLoader>();

        generalSettings.Manager = managerSettings;
        
        // Casting this, because I couldn't stand the `this field is obsolete` warning
        ((List<XRLoader>)managerSettings.activeLoaders).Clear();
        ((List<XRLoader>)managerSettings.activeLoaders).Add(xrLoader);

        OpenXRSettings.Instance.renderMode = OpenXRSettings.RenderMode.MultiPass;
        OpenXRSettings.Instance.depthSubmissionMode = OpenXRSettings.DepthSubmissionMode.None;
        
        // Initialize XR
        generalSettings.InitXRSDK();
        generalSettings.Start();
        
        Logger.LogInfo("Initialized OpenXR Runtime");
    }

    /// <summary>
    /// Start the XR display subsystem
    /// </summary>
    /// <returns><see langword="false"/> if no displays were found, <see langword="true"/> otherwise.</returns>
    private bool StartDisplay()
    {
        var displays = new List<XRDisplaySubsystem>();
        
        SubsystemManager.GetInstances(displays);

        if (displays.Count < 1)
            return false;
        
        displays[0].Start();
        
        Logger.LogInfo("Started XR Display subsystem, welcome to VR!");

        return true;
    }

    /// <summary>
    /// Helper function for <see cref="SetupRuntimeAssets"/> to copy resource files and return false if the source does not exist
    /// </summary>
    private bool CopyResourceFile(string sourceFile, string destinationFile)
    {
        if (!File.Exists(sourceFile))
            return false;

        if (File.Exists(destinationFile))
        {
            var sourceHash = Utils.ComputeHash(File.ReadAllBytes(sourceFile));
            var destHash = Utils.ComputeHash(File.ReadAllBytes(destinationFile));

            if (sourceHash.SequenceEqual(destHash))
                return true;
        }
        
        File.Copy(sourceFile, destinationFile, true);

        return true;
    }

    /// <summary>
    /// Place required runtime libraries and configuration in the game files to allow VR to be started
    /// </summary>
    private void SetupRuntimeAssets(out bool mustRestart)
    {
        mustRestart = false;

        var root = Path.Combine(Paths.GameRootPath, "Content Warning_Data");
        var subsystems = Path.Combine(root, "UnitySubsystems");
        if (!Directory.Exists(subsystems))
            Directory.CreateDirectory(subsystems);

        var openXr = Path.Combine(subsystems, "UnityOpenXR");
        if (!Directory.Exists(openXr))
            Directory.CreateDirectory(openXr);

        var manifest = Path.Combine(openXr, "UnitySubsystemsManifest.json");
        if (!File.Exists(manifest))
        {
            File.WriteAllText(manifest, """
                                        {
                                                "name": "OpenXR XR Plugin",
                                                "version": "1.8.2",
                                                "libraryName": "UnityOpenXR",
                                                "displays": [
                                                        {
                                                                "id": "OpenXR Display"
                                                        }
                                                ],
                                                "inputs": [
                                                        {
                                                                "id": "OpenXR Input"
                                                        }
                                                ]
                                        }
                                        """);
            mustRestart = true;
        }

        var plugins = Path.Combine(root, "Plugins");
        var uoxrTarget = Path.Combine(plugins, "UnityOpenXR.dll");
        var oxrLoaderTarget = Path.Combine(plugins, "openxr_loader.dll");

        var current = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var uoxr = Path.Combine(current, "RuntimeDeps/UnityOpenXR.dll");
        var oxrLoader = Path.Combine(current, "RuntimeDeps/openxr_loader.dll");
        
        if (!CopyResourceFile(uoxr, uoxrTarget))
            Logger.LogWarning("Could not find UnityOpenXR.dll to copy to the game, VR might not work!");
        
        if (!CopyResourceFile(oxrLoader, oxrLoaderTarget))
            Logger.LogWarning("Could not find openxr_loader.dll to copy to the game, VR might not work!");
    }
}

[Flags]
public enum Flags
{
    VR = 1 << 0,
    RestartRequired = 1 << 1
}
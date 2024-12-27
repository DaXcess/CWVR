using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using CWVR.Assets;
using CWVR.MultiLoader.Common;
using CWVR.Patches;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using ILogger = CWVR.MultiLoader.Common.ILogger;

namespace CWVR;

public static class Plugin
{
    public const string PLUGIN_GUID = "io.daxcess.cwvr";
    public const string PLUGIN_NAME = "CWVR";
    public const string PLUGIN_VERSION = "1.1.1";

    private const string BANNER =
        "                             ,--.,--.                         \n ,-----.,--.   ,--.         /  //  /     ,--.   ,--.,------.  \n'  .--./|  |   |  |        /  //  /       \\  `.'  / |  .--. ' \n|  |    |  |.'.|  |       /  //  /         \\     /  |  '--'.' \n'  '--'\\|   ,'.   |      /  //  /           \\   /   |  |\\  \\  \n `-----''--'   '--'     /  //  /             `-'    `--' '--' \n                       `--'`--'                               \n\n             ___________________________ \n            < Another VR mod by DaXcess >\n             --------------------------- \n                    \\   ^__^\n                     \\  (oo)\\_______\n                        (__)\\       )\\/\\\n                            ||----w |\n                            ||     ||\n";

    public static ILogger Logger { get; set; }
    public static IConfig Config { get; set; }
    public static Flags Flags { get; set; } = 0;
    public static Loader Loader { get; set; }

    /// <summary>
    /// Prepare CWVR for use within Content Warning using their modding framework
    /// </summary>
    public static void SetupEarlyDependencies()
    {
        var modRoot = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var runtimeDeps = Path.Combine(modRoot, "RuntimeDeps");
        
        // Force load assemblies required by CWVR
        
        foreach (var file in Directory.GetFiles(runtimeDeps, "*.dll"))
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
    
    public static void InitializePlugin()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        InputSystem.PerformDefaultPluginInitialization();
        
        foreach (var line in BANNER.Split('\n'))
            Logger.LogInfo($"   {line}");

        var disableVr = Config.DisableVR.Value || Environment.GetCommandLineArgs()
            .Contains("--disable-vr", StringComparer.InvariantCultureIgnoreCase);
        
        if (disableVr)
            Logger.LogWarning("VR has been disabled by config or the `--disable-vr` command line flag");
        
        if (!AssetManager.LoadAssets())
        {
            Logger.LogError("Disabling mod because assets could not be loaded!");
            return;
        }

        HarmonyPatcher.PatchUniversal();
        Logger.LogInfo("Inserted Universal patches using Harmony");
        
        if (disableVr || !InitializeVR())
            return;

        // Perform global VR setup here

        Flags |= Flags.VR;
    }

    private static bool InitializeVR()
    {
        Logger.LogInfo("Loading VR...");

        if (!OpenXR.Loader.InitializeXR())
        {
            Logger.LogError("Failed to start in VR Mode! Only Non-VR features are available!");
            Logger.LogWarning("If your intention is to play without VR, you can ignore the previous error.");

            Utils.EnqueueModal("VR startup failed",
                "Something went wrong and we weren't able to launch the game in VR.\nIf you want to play without VR, it is recommended to disable VR completely by pressing the button below.\nIf you need help troubleshooting, grab a copy of the logs, which you can open using the button below.",
                [
                    new ModalOption("Disable VR", () => Config.DisableVR.Value = true), new ModalOption("Open Logs",
                        () => Process.Start("notepad.exe", Application.consoleLogPath)),
                    new ModalOption("Continue")
                ]);

            return false;
        }

        if (OpenXR.GetActiveRuntimeName(out var name) &&
            OpenXR.GetActiveRuntimeVersion(out var major, out var minor, out var patch))
            Logger.LogInfo($"OpenXR Runtime being used: {name} ({major}.{minor}.{patch})");
        else
            Logger.LogWarning("Could not get OpenXR Runtime info?");

        HarmonyPatcher.PatchVR();
        
        Logger.LogDebug("Inserted VR patches using Harmony");

        var pass = new XROcclusionMeshPass(RenderPassEvent.BeforeRendering);

        RenderPipelineManager.beginCameraRendering += (context, camera) =>
        {
            camera.GetUniversalAdditionalCameraData().scriptableRenderer.EnqueuePass(pass);
        };
        
        return true;
    }
}

[Flags]
public enum Flags
{
    VR = 1 << 0,
}

public enum Loader
{
    BepInEx,
    Native
}
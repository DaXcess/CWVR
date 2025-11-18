using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using CWVR.Assets;
using CWVR.Patches;
using Unity.Burst.LowLevel;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CWVR;

[ContentWarningPlugin(PLUGIN_GUID, PLUGIN_VERSION, true)]
[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public const string PLUGIN_GUID = "io.daxcess.cwvr";
    public const string PLUGIN_NAME = "CWVR";
    public const string PLUGIN_VERSION = "1.2.0";

    private const string BANNER =
        "                             ,--.,--.                         \n ,-----.,--.   ,--.         /  //  /     ,--.   ,--.,------.  \n'  .--./|  |   |  |        /  //  /       \\  `.'  / |  .--. ' \n|  |    |  |.'.|  |       /  //  /         \\     /  |  '--'.' \n'  '--'\\|   ,'.   |      /  //  /           \\   /   |  |\\  \\  \n `-----''--'   '--'     /  //  /             `-'    `--' '--' \n                       `--'`--'                               \n\n             ___________________________ \n            < Another VR mod by DaXcess >\n             --------------------------- \n                    \\   ^__^\n                     \\  (oo)\\_______\n                        (__)\\       )\\/\\\n                            ||----w |\n                            ||     ||\n";

    public new static Config Config { get; set; }
    public static Flags Flags { get; set; } = 0;

    private void Awake()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        InputSystem.PerformDefaultPluginInitialization();

        CWVR.Logger.source = Logger;

        Config = new Config(base.Config);
        base.Config.SettingChanged += (_, _) => Config.ApplySettings();

        foreach (var line in BANNER.Split('\n'))
            Logger.LogInfo($"   {line}");

        // Allow disabling VR via config and command line
        var disableVr = Config.DisableVR.Value || Environment.GetCommandLineArgs()
            .Contains("--disable-vr", StringComparer.InvariantCultureIgnoreCase);

        if (disableVr)
            Logger.LogWarning("VR has been disabled by config or the `--disable-vr` command line flag");

        if (!PreloadRuntimeDependencies())
        {
            Logger.LogError("Disabling mod because required runtime dependencies could not be loaded!");
            return;
        }

        if (!LoadBurstLibrary())
        {
            Logger.LogError("Disabling mod because required Burst optimizations could not be loaded!");
            return;
        }

        if (!AssetManager.LoadAssets())
        {
            Logger.LogError("Disabling mod because assets could not be loaded!");
            return;
        }

        // The built-in mirror view shader is broken in this version for some reason (maybe optimized to no-op)
        Utils.SetupOrReplaceXRMirrorShader();

        HarmonyPatcher.PatchUniversal();
        Logger.LogInfo("Inserted Universal patches using Harmony");

        if (!disableVr && InitializeVR())
            Flags |= Flags.VR;

        Utils.Enqueue(() => Config.ApplySettings());
    }

    private bool PreloadRuntimeDependencies()
    {
        try
        {
            var deps = Path.Combine(Path.GetDirectoryName(Info.Location)!, "RuntimeDeps");

            foreach (var file in Directory.GetFiles(deps, "*.dll"))
            {
                var filename = Path.GetFileName(file);

                // Ignore known unmanaged libraries
                if (filename is "lib_burst_generated.dll")
                    continue;

                try
                {
                    Assembly.LoadFile(file);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning($"Failed to preload '{filename}': {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(
                $"Unexpected error occured while preloading runtime dependencies (incorrect folder structure?): {ex.Message}");

            return false;
        }

        return true;
    }

    public static void ToggleVR()
    {
        if (Flags.HasFlag(Flags.VR))
        {
            OpenXR.Loader.DeinitializeXR();
            HarmonyPatcher.UnpatchVR();

            Flags &= ~Flags.VR;
        }
        else
        {
            if (!InitializeVR())
                return;

            Flags |= Flags.VR;
        }
    }

    private static bool LoadBurstLibrary()
    {
        var modRoot = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var runtimeDeps = Path.Combine(modRoot, "RuntimeDeps");
        var burstLibrary = Path.Combine(runtimeDeps, "lib_burst_generated.dll");

        return File.Exists(burstLibrary) && BurstCompilerService.LoadBurstLibrary(burstLibrary);
    }

    private static bool InitializeVR()
    {
        CWVR.Logger.LogInfo("Loading VR...");

        if (!OpenXR.Loader.InitializeXR())
        {
            CWVR.Logger.LogError("Failed to start in VR Mode! Only Non-VR features are available!");
            CWVR.Logger.LogWarning("If your intention is to play without VR, you can ignore the previous error.");

            Utils.EnqueueModal("VR startup failed",
                "Something went wrong and we weren't able to launch the game in VR.\nIf you want to play without VR, it is recommended to disable VR completely by pressing the button below.\nIf you need help troubleshooting, grab a copy of the logs, which you can open using the button below.",
                [
                    new ModalOption("Disable VR", () => Config.DisableVR.Value = true),
                    new ModalOption("Open Logs", () => Process.Start("notepad.exe", Application.consoleLogPath)),
                    new ModalOption("Continue")
                ]);

            return false;
        }

        if (OpenXR.GetActiveRuntimeName(out var name) &&
            OpenXR.GetActiveRuntimeVersion(out var major, out var minor, out var patch))
            CWVR.Logger.LogInfo($"OpenXR Runtime being used: {name} ({major}.{minor}.{patch})");
        else
            CWVR.Logger.LogWarning("Could not get OpenXR Runtime info?");

        HarmonyPatcher.PatchVR();

        CWVR.Logger.LogDebug("Inserted VR patches using Harmony");

        return true;
    }
}

[Flags]
public enum Flags
{
    VR = 1 << 0,
}
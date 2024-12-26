using System.Diagnostics;
using System.Reflection;
using Mono.Cecil;
using MonoMod.RuntimeDetour;

namespace CWVR.Preload;

public static class Preload
{
    public static IEnumerable<string> TargetDLLs { get; } = [];

    private const string VR_MANIFEST = """
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
                                       """;

    private static ILogger Logger { get; set; } = null!;

    /// <summary>
    /// Content Warning Preload entrypoint
    /// </summary>
    private static void PreloadInit()
    {
        Logger = new NopLogger();
        
        InitializeMultiLoader();
        PatchAssemblyTypeEnumerator();
    }
    
    /// <summary>
    /// BepInEx Preload entrypoint
    /// </summary>
    private static void Initialize()
    {
        Logger = new BepInExLogger();
        
        InitializeMultiLoader();
    }

    private static void InitializeMultiLoader()
    {
        Logger.Log("Setting up VR runtime assets");

        SetupRuntimeAssets();

        Logger.Log("We're done here. Goodbye!");
    }

    private static void SetupRuntimeAssets()
    {
        SetupSubsystemAssets();
        SetupPluginAssets();
    }

    private static void SetupSubsystemAssets()
    {
        var gameRoot = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule!.FileName)!;
        var root = Path.Combine(gameRoot, "Content Warning_Data");
        var subsystems = Path.Combine(root, "UnitySubsystems");
        if (!Directory.Exists(subsystems))
            Directory.CreateDirectory(subsystems);

        var openXr = Path.Combine(subsystems, "UnityOpenXR");
        if (!Directory.Exists(openXr))
            Directory.CreateDirectory(openXr);

        var manifest = Path.Combine(openXr, "UnitySubsystemsManifest.json");
        if (!File.Exists(manifest))
            File.WriteAllText(manifest, VR_MANIFEST);
    }

    private static void SetupPluginAssets()
    {
        var gameRoot = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule!.FileName)!;
        var root = Path.Combine(gameRoot, "Content Warning_Data");
        var plugins = Path.Combine(root, "Plugins");
        var oxrPluginTarget = Path.Combine(plugins, "UnityOpenXR.dll");
        var oxrLoaderTarget = Path.Combine(plugins, "openxr_loader.dll");

        var current = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        var oxrPlugin = Path.Combine(current, "RuntimeDeps/UnityOpenXR.dll");
        var oxrLoader = Path.Combine(current, "RuntimeDeps/openxr_loader.dll");
        
        File.Copy(oxrPlugin, oxrPluginTarget, true);
        File.Copy(oxrLoader, oxrLoaderTarget, true);
    }

#pragma warning disable CS8618
    // Keep in scope just to be sure the hook stays attached
    private static Hook _getTypesHook;
#pragma warning restore CS8618
    
    /// <summary>
    /// Hook Assembly.GetTypes() so it won't crash if it encounters references to missing assemblies
    /// </summary>
    private static void PatchAssemblyTypeEnumerator()
    {
        _getTypesHook = new Hook(
            typeof(Assembly).GetMethod("GetTypes", BindingFlags.Instance | BindingFlags.Public),
            typeof(Preload).GetMethod(nameof(GetTypesHooked))
        );
    }
    
    public static Type[] GetTypesHooked(Func<Assembly, Type[]> orig, Assembly self)
    {
        try
        {
            return orig(self);
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.Where(t => t != null).ToArray();
        }
    }
    
    public static void Patch(AssemblyDefinition assembly)
    {
        // No-op
    }
}
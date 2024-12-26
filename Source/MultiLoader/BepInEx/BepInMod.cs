using BepInEx;

namespace CWVR.MultiLoader.BepInEx;

[BepInPlugin(Plugin.PLUGIN_GUID, Plugin.PLUGIN_NAME, Plugin.PLUGIN_VERSION)]
public class BepInMod : BaseUnityPlugin
{
    private void Awake()
    {
        Plugin.Logger = new Logger(Logger);
        Plugin.Config = new Config(Config);
        Plugin.Loader = Loader.BepInEx;
        
        Plugin.InitializePlugin();
    }
}

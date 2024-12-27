using BepInEx;

namespace CWVR.MultiLoader.BepInEx;

#if BEPINEX

[ContentWarningPlugin(Plugin.PLUGIN_GUID, Plugin.PLUGIN_VERSION, true)]
[BepInPlugin(Plugin.PLUGIN_GUID, Plugin.PLUGIN_NAME, Plugin.PLUGIN_VERSION)]
public class BepInMod : BaseUnityPlugin
{
    private void Awake()
    {
        var config = new Config(Config);

        Config.SettingChanged += (_, _) => config.ApplySettings();

        Plugin.Config = config;
        Plugin.Logger = new Logger(Logger);
        Plugin.Loader = Loader.BepInEx;
   
        Plugin.InitializePlugin();
        Utils.Enqueue(() => config.ApplySettings());
    }
}

#endif
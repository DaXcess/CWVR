using System.Collections.Generic;
using System.Linq;
using BepInEx;

namespace CWVR;

public class Compat
{
    private readonly CompatibleMod[] ModCompatibilityList =
    [
        new CompatibleMod("ContentSettings", "CommanderCat101.ContentSettings")
    ];
    
    private readonly List<string> DetectedMods = [];

    public Compat(IEnumerable<PluginInfo> plugins)
    {
        foreach (var plugin in plugins)
        {
            var mod = ModCompatibilityList.FirstOrDefault(mod => mod.Guid == plugin.Metadata.GUID);

            if (mod == null)
                continue;

            Logger.LogInfo($"Found compatible mod {mod.Name}");

            DetectedMods.Add(mod.Name);
        }
    }

    public bool IsLoaded(string modName)
    {
        return DetectedMods.Contains(modName);
    }

    private class CompatibleMod(string name, string guid)
    {
        public string Name { get; } = name;
        public string Guid { get; } = guid;
    }
}
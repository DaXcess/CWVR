using ContentSettings.API.Settings.UI;
using HarmonyLib;

namespace CWVR.Compatibility.ContentSettings;

internal static class ContentSettingsExtensions
{
    public static SettingsMenu GetSettingsMenu(this SettingsTab tab)
    {
        return (SettingsMenu)AccessTools.Field(typeof(SettingsTab), "settingsMenu").GetValue(tab);
    }
}
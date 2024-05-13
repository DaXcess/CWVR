using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BepInEx.Configuration;
using CWVR.Assets;
using CWVR.Input;
using CWVR.Patches;
using CWVR.Player;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CWVR.UI.Settings;

public class SettingsMenu : MonoBehaviour
{
    private SFX_Instance hoverSound;
    private SFX_Instance clickSound;

    private bool isInitializing;
    private bool isDirty;

    private TMP_Dropdown runtimesDropdown;

    internal RemapHandler remapHandler;
    
    private void Awake()
    {
        // Grab sound effects from another UI element
        var button = FindObjectOfType<EscapeMenuButton>();

        hoverSound = button.hoverSound;
        clickSound = button.clickSound;
        
        if (Plugin.Compatibility.IsLoaded("ContentSettings"))
            return;

        // Create settings tab
        var tabs = GetComponentInChildren<CW_TABS>();
        var settingsTabObj = Instantiate(AssetManager.VRSettingsTab, tabs.transform);
        var settingsTab = settingsTabObj.GetComponent<CW_TAB>();
        var settingsCatTabVr = settingsTabObj.GetComponent<SettingCategoryTab>();
        
        settingsTab.hoverSound = hoverSound;
        settingsTab.clickSound = clickSound;
        settingsCatTabVr.settingsMenu = global::SettingsMenu.Instance;
    }
    
    private GameObject[] objects = [];

    internal void DisplayVRSettings(Transform container)
    {
        isInitializing = true;
        
        var categories = new Dictionary<string, List<KeyValuePair<ConfigDefinition, ConfigEntryBase>>>();

        foreach (var entry in Plugin.Config.File)
        {
            // Skip internal
            if (entry.Key.Section.ToLower() == "internal")
                continue;

            if (!categories.TryGetValue(entry.Key.Section, out var list))
            {
                list = [];
                categories.Add(entry.Key.Section, list);
            }
            
            list.Add(entry);
        }

        var uiList = new List<GameObject>();

        // Add OpenXR Runtime setting
        var runtimes = OpenXR.DetectOpenXRRuntimes(out _);

        if (runtimes != null)
        {
            var enumUI = Instantiate(AssetManager.EnumSettingCell, container);
            var text = enumUI.GetComponentInChildren<TextMeshProUGUI>();
            var entry = enumUI.GetComponentInChildren<ConfigEntry>();
            runtimesDropdown = enumUI.GetComponentInChildren<TMP_Dropdown>();

            text.text = "OpenXR Runtime";
            entry.m_Category = "Internal";
            entry.m_Name = "OpenXRRuntimeFile";

            var selectedIndex = 0;

            if (!string.IsNullOrEmpty(Plugin.Config.OpenXRRuntimeFile.Value))
                for (var i = 0; i < runtimes.Count; i++)
                    if (runtimes.ElementAt(i).Value == Plugin.Config.OpenXRRuntimeFile.Value)
                    {
                        selectedIndex = i + 1;
                        break;
                    }

            runtimesDropdown.AddOptions(["System Default", .. runtimes.Keys]);
            runtimesDropdown.value = selectedIndex;

            uiList.Add(enumUI);
        }

        // Add settings derived from Config.cs
        foreach (var (category, settings) in categories)
        {
            foreach (var (key, config) in settings)
            {
                var name = key.Key;

                if (config.SettingType.IsEnum)
                {
                    var enumUI = Instantiate(AssetManager.EnumSettingCell, container);
                    var text = enumUI.GetComponentInChildren<TextMeshProUGUI>();
                    var dropdown = enumUI.GetComponentInChildren<TMP_Dropdown>();
                    var entry = enumUI.GetComponentInChildren<ConfigEntry>();

                    text.text = Utils.PascalToLongString(name);
                    entry.m_Category = category;
                    entry.m_Name = name;

                    var names = Enum.GetNames(config.SettingType);
                    var idx = Array.FindIndex(names, name => name == config.BoxedValue.ToString());

                    dropdown.ClearOptions();
                    dropdown.AddOptions([.. names]);
                    dropdown.SetValueWithoutNotify(idx);
                    
                    uiList.Add(enumUI);
                }
                else if (config.SettingType == typeof(float) &&
                         config.Description.AcceptableValues is AcceptableValueRange<float> floatValues)
                {
                    var sliderUI = Instantiate(AssetManager.SliderSettingCell, container);
                    var text = sliderUI.GetComponentInChildren<TextMeshProUGUI>();
                    var slider = sliderUI.GetComponentInChildren<Slider>();
                    var input = sliderUI.GetComponentInChildren<TMP_InputField>();
                    var entry = sliderUI.GetComponentInChildren<ConfigEntry>();

                    text.text = Utils.PascalToLongString(name);
                    entry.m_Category = category;
                    entry.m_Name = name;

                    slider.maxValue = floatValues.MaxValue;
                    slider.minValue = floatValues.MinValue;
                    var value = Mathf.Round((float)config.BoxedValue * 100f) / 100f;
                    slider.SetValueWithoutNotify(value);
                    input.SetTextWithoutNotify(value.ToString(CultureInfo.InvariantCulture));
                    
                    uiList.Add(sliderUI);
                }
                else if (config.SettingType == typeof(int) &&
                         config.Description.AcceptableValues is AcceptableValueRange<int> intValues)
                {
                    var sliderUI = Instantiate(AssetManager.SliderSettingCell, container);
                    var text = sliderUI.GetComponentInChildren<TextMeshProUGUI>();
                    var slider = sliderUI.GetComponentInChildren<Slider>();
                    var input = sliderUI.GetComponentInChildren<TMP_InputField>();
                    var entry = sliderUI.GetComponentInChildren<ConfigEntry>();

                    text.text = Utils.PascalToLongString(name);
                    entry.m_Category = category;
                    entry.m_Name = name;

                    slider.maxValue = intValues.MaxValue;
                    slider.minValue = intValues.MinValue;
                    slider.wholeNumbers = true;
                    slider.SetValueWithoutNotify((int)config.BoxedValue);
                    input.SetTextWithoutNotify(config.BoxedValue.ToString());
                    
                    uiList.Add(sliderUI);
                }
                else if (config.SettingType == typeof(bool))
                {
                    var boolUI = Instantiate(AssetManager.BooleanSettingCell, container);
                    var text = boolUI.GetComponentInChildren<TextMeshProUGUI>();
                    var dropdown = boolUI.GetComponentInChildren<TMP_Dropdown>();
                    var entry = boolUI.GetComponentInChildren<ConfigEntry>();
                    
                    text.text = Utils.PascalToLongString(name);
                    entry.m_Category = category;
                    entry.m_Name = name;
                    
                    dropdown.SetValueWithoutNotify((bool)config.BoxedValue ? 1 : 0);    
                    
                    uiList.Add(boolUI);
                }
            }
        }

        objects = uiList.ToArray();

        foreach (var obj in objects)
        foreach (var uiSound in obj.GetComponentsInChildren<UI_Sound>(true))
        {
            uiSound.hoverSound = hoverSound;
            uiSound.clickSound = clickSound;
        }

        isInitializing = false;
    }

    internal void DisplayControlsSettings(Transform container)
    {
        isInitializing = true;
        
        var properties = AccessTools.GetPropertyNames(typeof(Controls));
        var bindings = Plugin.Config.EnableCustomControls.Value
            ? Binding.LoadFromJson(Plugin.Config.CustomControls.Value)
            : ControlScheme.GetProfile(InputSystem.DetectControllerProfile());
        
        var uiList = new List<GameObject>();

        {
            var boolUI = Instantiate(AssetManager.BooleanSettingCell, container);
            var text = boolUI.GetComponentInChildren<TextMeshProUGUI>();
            var dropdown = boolUI.GetComponentInChildren<TMP_Dropdown>();
            var entry = boolUI.GetComponentInChildren<ConfigEntry>();

            text.text = "Enable Custom Controls";
            entry.m_Category = "Internal";
            entry.m_Name = "EnableCustomControls";

            dropdown.SetValueWithoutNotify(Plugin.Config.EnableCustomControls.Value ? 1 : 0);

            uiList.Add(boolUI);
        }

        foreach (var name in properties)
        {
            if (AccessTools.Property(typeof(Controls), name) is not { } property) continue;

            if (property.PropertyType != typeof(InputSystem.Axis2DControl) &&
                property.PropertyType != typeof(InputSystem.ButtonControl))
                continue;
            
            var controlUI = Instantiate(AssetManager.ControlSettingCell, container);
            var entry = controlUI.GetComponent<RemapBinding>();

            entry.settingTitle.text = Utils.PascalToLongString(name);
            entry.settingName = name;
            entry.remapHandler = remapHandler;
            
            if (property.PropertyType == typeof(InputSystem.Axis2DControl))
            {
                entry.isAxisOnly = true;
            } else if (property.PropertyType == typeof(InputSystem.ButtonControl))
            {
                entry.isAxisOnly = false;
            }

            if (!Plugin.Config.EnableCustomControls.Value)
            {
                controlUI.GetComponentInChildren<Button>().interactable = false;
                controlUI.GetComponentInChildren<Slider>().interactable = false;
                controlUI.GetComponentInChildren<TMP_InputField>().interactable = false;
            }

            entry.SetBinding(bindings[name]);
            
            uiList.Add(controlUI);
        }
        
        objects = uiList.ToArray();

        foreach (var obj in objects)
        foreach (var uiSound in obj.GetComponentsInChildren<UI_Sound>(true))
        {
            uiSound.hoverSound = hoverSound;
            uiSound.clickSound = clickSound;
        }

        isInitializing = false;
    }

    private void ReloadControlsSettings(bool useCustom)
    {
        var bindings = useCustom
            ? Binding.LoadFromJson(Plugin.Config.CustomControls.Value)
            : ControlScheme.GetProfile(InputSystem.DetectControllerProfile());
        
        foreach (var obj in objects[1..])
        {
            var entry = obj.GetComponent<RemapBinding>();
            entry.SetBinding(bindings[entry.settingName]);
            
            obj.GetComponentInChildren<Button>().interactable = useCustom;
            obj.GetComponentInChildren<Slider>().interactable = useCustom;
            obj.GetComponentInChildren<TMP_InputField>().interactable = useCustom;
        }
        
        // Reload bindings if we're in game
        if (VRSession.Instance is {} instance)
            instance.Controls.ReloadBindings();
    }
    
    internal void DestroySettings()
    {
        foreach (var obj in objects)
            Destroy(obj);

        objects = [];
    }

    private void SetOpenXRRuntime(int index)
    {
        if (!runtimesDropdown)
            return;
        
        if (index == 0)
        {
            Plugin.Config.OpenXRRuntimeFile.Value = "";
            return;
        }

        var name = runtimesDropdown.options[index].text;
        var runtimes = OpenXR.DetectOpenXRRuntimes(out _);
        
        if (!runtimes.TryGetValue(name, out var runtimeFile))
        {
            Modal.ShowError("Error", "Failed to update OpenXR Runtime");
            return;
        }

        Plugin.Config.OpenXRRuntimeFile.Value = runtimeFile;
    }
    
    internal void UpdateValue(string category, string name, object value)
    {
        // Ignore updates when populating initial values
        if (isInitializing)
            return;

        switch (category)
        {
            case "Internal" when name == "OpenXRRuntimeFile":
                SetOpenXRRuntime((int)value);
                return;
            case "Internal" when name == "EnableCustomControls":
                ReloadControlsSettings(Convert.ToBoolean(value));
                break;
        }

        var entry = Plugin.Config.File[category, name];
        if (entry is null)
            return;

        if (value.GetType() == entry.SettingType)
            entry.BoxedValue = value;
        else if (entry.SettingType == typeof(float))
            entry.BoxedValue = Convert.ToSingle(value);
        else if (entry.SettingType == typeof(string))
            entry.BoxedValue = Convert.ToString(value);
        else if (entry.SettingType == typeof(int))
            entry.BoxedValue = Convert.ToInt32(value);
        else if (entry.SettingType == typeof(bool))
            entry.BoxedValue = Convert.ToBoolean(value);
        else if (entry.SettingType.IsEnum)
        {
            if (value is string s)
                entry.BoxedValue = Enum.Parse(entry.SettingType, s);
            else
                entry.BoxedValue = Enum.ToObject(entry.SettingType, value);
        }
        else
            throw new InvalidCastException();

        isDirty = true;
    }
}

[CWVRPatch(CWVRPatchTarget.Universal)]
[HarmonyPatch]
internal static class SettingsMenuPatches
{
    [HarmonyPatch(typeof(global::SettingsMenu), nameof(global::SettingsMenu.Show))]
    [HarmonyPrefix]
    private static void BeforeShow(SettingCategory category)
    {
        Object.FindObjectOfType<SettingsMenu>().DestroySettings();
    }

    [HarmonyPatch(typeof(global::SettingsMenu), nameof(global::SettingsMenu.Show))]
    [HarmonyPostfix]
    private static void AfterShow(global::SettingsMenu __instance, SettingCategory category)
    {
        if ((int)category == 3)
            Object.FindObjectOfType<SettingsMenu>().DisplayVRSettings(__instance.m_settingsContainer);

        if (category is SettingCategory.Controls && VRSession.InVR)
        {
            foreach (var cell in __instance.m_cells)
                Object.Destroy(cell.gameObject);
            
            __instance.m_cells.Clear();
            
            Object.FindObjectOfType<SettingsMenu>().DisplayControlsSettings(__instance.m_settingsContainer);
        }
    }
}
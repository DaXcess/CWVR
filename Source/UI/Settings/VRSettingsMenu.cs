using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BepInEx.Configuration;
using CWVR.Assets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CWVR.UI.Settings;

public class VRSettingsMenu : MonoBehaviour
{
    public static VRSettingsMenu Instance { get; private set; }
    
    private SFX_Instance hoverSound;
    private SFX_Instance clickSound;

    private TMP_Dropdown runtimesDropdown;

    private void Awake()
    {
        Instance = this;
        
        // Grab sound effects from another UI element
        var button = FindObjectOfType<EscapeMenuButton>();

        hoverSound = button.hoverSound;
        clickSound = button.clickSound;
    }
    
    private GameObject[] cells = [];

    internal void DisplayVRSettings(Transform container)
    {
        var uiList = new List<GameObject>();

        // Add OpenXR Runtime setting
        if (OpenXR.GetRuntimes() is var runtimes && runtimes.Count > 0)
        {
            var enumUI = Instantiate(AssetManager.EnumSettingCell, container);
            var text = enumUI.GetComponentInChildren<TextMeshProUGUI>();
            var entry = enumUI.GetComponentInChildren<ConfigEntry>();
            runtimesDropdown = enumUI.GetComponentInChildren<TMP_Dropdown>();

            text.text = "[CWVR] Preferred OpenXR Runtime";
            entry.m_Category = "Internal";
            entry.m_Name = "OpenXRRuntimeFile";

            var selectedIndex = 0;

            if (!string.IsNullOrEmpty(Plugin.Config.OpenXRRuntimeFile.Value))
                for (var i = 0; i < runtimes.Count; i++)
                    if (runtimes.ElementAt(i).Path == Plugin.Config.OpenXRRuntimeFile.Value)
                    {
                        selectedIndex = i + 1;
                        break;
                    }

            runtimesDropdown.AddOptions(["System Default", .. runtimes.Select(rt => rt.Name)]);
            runtimesDropdown.SetValueWithoutNotify(selectedIndex);

            uiList.Add(enumUI);
        }

        // BepInEx only configuration cells

#if BEPINEX

        // We need to have both a compiler *and* a runtime check since during development both BEPINEX and NATIVE are defined
        if (Plugin.Loader == Loader.BepInEx)
        {
            var categories = new Dictionary<string, List<KeyValuePair<ConfigDefinition, ConfigEntryBase>>>();

            foreach (var entry in ((CWVR.MultiLoader.BepInEx.Config)Plugin.Config).File)
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

            // Add settings derived from BepInEx/Config.cs
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

                        text.text = $"[CWVR] {Utils.PascalToLongString(name)}";
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

                        text.text = $"[CWVR] {Utils.PascalToLongString(name)}";
                        entry.m_Category = category;
                        entry.m_Name = name;

                        slider.m_MaxValue = floatValues.MaxValue;
                        slider.m_MinValue = floatValues.MinValue;

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

                        text.text = $"[CWVR] {Utils.PascalToLongString(name)}";
                        entry.m_Category = category;
                        entry.m_Name = name;

                        slider.m_MaxValue = intValues.MaxValue;
                        slider.m_MinValue = intValues.MinValue;
                        slider.m_WholeNumbers = true;
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

                        text.text = $"[CWVR] {Utils.PascalToLongString(name)}";
                        entry.m_Category = category;
                        entry.m_Name = name;

                        dropdown.SetValueWithoutNotify((bool)config.BoxedValue ? 1 : 0);

                        uiList.Add(boolUI);
                    }
                }
            }
        }

#endif

        cells = uiList.ToArray();
        
        foreach (var cell in cells)
        foreach (var uiSound in cell.GetComponentsInChildren<UI_Sound>(true))
        {
            uiSound.hoverSound = hoverSound;
            uiSound.clickSound = clickSound;
        }
    }

    internal void DestroySettings()
    {
        foreach (var cell in cells.Where(cell => cell != null))
            Destroy(cell);

        cells = [];
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

        if (OpenXR.GetRuntimes() is var runtimes && runtimes.Count == 0)
        {
            Modal.ShowError("Error", "Failed to query system installed OpenXR runtimes");
            return;
        }

        var runtimeName = runtimesDropdown.options[index].text;
        if (!runtimes.TryGetRuntime(runtimeName, out var runtime))
        {
            Modal.ShowError("Error", "Failed to update OpenXR runtime");
            return;
        }

        Plugin.Config.OpenXRRuntimeFile.Value = runtime.Path;
    }

    internal void UpdateValue(string category, string name, object value)
    {
        if (category == "Internal" && name == "OpenXRRuntimeFile")
        {
            SetOpenXRRuntime((int)value);
            return;
        }
            
        var entry = ((CWVR.MultiLoader.BepInEx.Config)Plugin.Config).File[category, name];
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
    }
}
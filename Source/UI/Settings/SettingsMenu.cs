using System;
using System.Collections.Generic;
using System.Globalization;
using BepInEx.Configuration;
using CWVR.Assets;
using CWVR.Patches;
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
    
    private void Awake()
    {
        // Grab references to original working tabs
        var tabs = GetComponentInChildren<CW_TABS>();
        var tab = tabs.GetComponentInChildren<CW_TAB>();

        hoverSound = tab.hoverSound;
        clickSound = tab.clickSound;
        
        // Create settings tab
        var settingsTabObj = Instantiate(AssetManager.VRSettingsTab, tabs.transform);
        var settingsTab = settingsTabObj.GetComponent<CW_TAB>();
        var settingsCatTabVr = settingsTabObj.GetComponent<SettingCategoryTab>();
        
        settingsTab.hoverSound = hoverSound;
        settingsTab.clickSound = clickSound;
        settingsCatTabVr.settingsMenu = global::SettingsMenu.Instance;
    }

    private GameObject[] objects = [];

    internal void DisplaySettings(Transform container)
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
        
        foreach (var (category, settings) in categories)
        {
            foreach (var setting in settings)
            {
                var name = setting.Key.Key;
                var config = setting.Value;

                if (config.SettingType.IsEnum)
                {
                    var enumPrefab = AssetManager.Load<GameObject>("EnumSettingCell");
                    var enumUI = Instantiate(enumPrefab, container);
                    var text = enumUI.GetComponentInChildren<TextMeshProUGUI>();
                    var dropdown = enumUI.GetComponentInChildren<TMP_Dropdown>();
                    var entry = enumUI.GetComponentInChildren<ConfigEntry>();

                    text.text = Utils.PascalToLongString(name);
                    entry.m_Category = category;
                    entry.m_Name = name;

                    var names = Enum.GetNames(config.SettingType);
                    var idx = Array.FindIndex(names, (name) => name == config.BoxedValue.ToString());

                    dropdown.ClearOptions();
                    dropdown.AddOptions([.. names]);
                    dropdown.SetValueWithoutNotify(idx);
                    
                    uiList.Add(enumUI);
                }
                else if (config.SettingType == typeof(float) &&
                         config.Description.AcceptableValues is AcceptableValueRange<float> floatValues)
                {
                    var sliderPrefab = AssetManager.Load<GameObject>("SliderSettingCell");
                    var sliderUI = Instantiate(sliderPrefab, container);
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
                    var sliderPrefab = AssetManager.Load<GameObject>("SliderSettingCell");
                    var sliderUI = Instantiate(sliderPrefab, container);
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
                    var boolPrefab = AssetManager.Load<GameObject>("BooleanSettingCell");
                    var boolUI = Instantiate(boolPrefab, container);
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

    internal void DestroySettings()
    {
        foreach (var obj in objects)
            Destroy(obj);
    }

    internal void UpdateValue(string category, string name, object value)
    {
        // Ignore updates when populating initial values
        if (isInitializing)
            return;

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

    internal void ApplySettings()
    {
        if (!isDirty)
            return;
        
        // TODO: Apply settings here when needed (e.g. XR eye resolution scale)
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
        if ((int)category != 3)
            return;
        
        Object.FindObjectOfType<SettingsMenu>().DisplaySettings(__instance.m_settingsContainer);
    }

    [HarmonyPatch(typeof(MainMenuSettingsPage), nameof(MainMenuSettingsPage.OnBackButtonClicked))]
    [HarmonyPrefix]
    private static void OnCloseSettingsPage()
    {
        Object.FindObjectOfType<SettingsMenu>().ApplySettings();
    }
}
using UnityEngine;

namespace CWVR.UI.Settings;

public class ConfigEntry : MonoBehaviour
{
    private SettingsMenu settingsMenu;
    
    public string m_Category;
    public string m_Name;

    private void Awake()
    {
        settingsMenu = GetComponentInParent<SettingsMenu>();
    }

    public void UpdateValue(int value)
    { 
        settingsMenu.UpdateValue(m_Category, m_Name, value);
    }

    public void UpdateValue(float value)
    {
        settingsMenu.UpdateValue(m_Category, m_Name, value);
    }

    public void UpdateValue(string value)
    {
        settingsMenu.UpdateValue(m_Category, m_Name, value);
    }

    public void UpdateValue(bool value)
    {
        settingsMenu.UpdateValue(m_Category, m_Name, value);
    }
}
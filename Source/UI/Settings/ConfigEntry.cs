using UnityEngine;

namespace CWVR.UI.Settings;

public class ConfigEntry : MonoBehaviour
{
    public string m_Category;
    public string m_Name;

    public void UpdateValue(int value)
    { 
        VRSettingsMenu.Instance.UpdateValue(m_Category, m_Name, value);
    }

    public void UpdateValue(float value)
    {
        VRSettingsMenu.Instance.UpdateValue(m_Category, m_Name, value);
    }

    public void UpdateValue(string value)
    {
        VRSettingsMenu.Instance.UpdateValue(m_Category, m_Name, value);
    }

    public void UpdateValue(bool value)
    {
        VRSettingsMenu.Instance.UpdateValue(m_Category, m_Name, value);
    }
}
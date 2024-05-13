using System.Globalization;
using CWVR.Input;
using CWVR.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CWVR.UI.Settings;

public class RemapBinding : MonoBehaviour
{
    [SerializeField] internal TextMeshProUGUI settingTitle = null;
    [SerializeField] internal string settingName;
 
    private Button button;
    private TextMeshProUGUI buttonText;
    private TMP_InputField deadzoneInput;
    private Slider deadzoneSlider;

    private Binding binding;
    
    public bool isAxisOnly;

    internal RemapHandler remapHandler;
    
    private void Awake()
    {
        button = GetComponentInChildren<Button>();
        buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        deadzoneInput = GetComponentInChildren<TMP_InputField>();
        deadzoneSlider = GetComponentInChildren<Slider>();
        
        button.onClick.AddListener(() =>
        {
            buttonText.text = "[...]";
            
            if (isAxisOnly)
            {
                remapHandler.DetectAxis2DControl(binding =>
                {
                    binding.deadzone = this.binding.deadzone;
                    
                    SetBinding(binding);
                    SaveBinding();
                });
            }
            else
            {
                remapHandler.DetectBooleanControl(binding =>
                {
                    binding.deadzone = this.binding.deadzone;

                    SetBinding(binding);
                    SaveBinding();
                });
            }
        });
    }

    public void SetBinding(Binding binding)
    {
        this.binding = binding;
        
        buttonText.text =
            $"{binding.controller} {(binding.button.HasValue ? Utils.PascalToLongString(binding.button.ToString()) : Utils.PascalToLongString(binding.axis.ToString()))}";
        
        deadzoneInput.text = binding.deadzone.ToString(CultureInfo.InvariantCulture);
        deadzoneSlider.value = binding.deadzone;
    }

    public void UpdateDeadzone(float deadzone)
    {
        var binding = this.binding;
        binding.deadzone = Mathf.Clamp(deadzone, 0, 1);

        SetBinding(binding);
        SaveBinding();
    }

    private void SaveBinding()
    {
        ControlScheme.UpdateBinding(settingName, binding);

        Plugin.Config.CustomControls.Value = ControlScheme.ToJson();
        
        // Reload controls if we're in game
        if (VRSession.Instance is { } instance)
            instance.Controls.ReloadBindings();
    }
}
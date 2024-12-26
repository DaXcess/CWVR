using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CWVR.UI.Settings;

public class InputSlider : MonoBehaviour
{
    private TMP_InputField inputField;
    private Slider slider;

    private void Start()
    {
        inputField = GetComponentInChildren<TMP_InputField>();
        slider = GetComponentInChildren<Slider>();

        inputField.onDeselect.AddListener(OnInputFieldChanged);
        inputField.onSubmit.AddListener(OnInputFieldChanged);
        slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnDestroy()
    {
        inputField.onDeselect.RemoveListener(OnInputFieldChanged);
        inputField.onSubmit.RemoveListener(OnInputFieldChanged);
        slider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }

    private void OnInputFieldChanged(string value)
    {
        if (!float.TryParse(value, out var floatValue))
            return;

        if (slider.wholeNumbers)
        {
            if (!int.TryParse(value, out var intValue))
                return;

            floatValue = intValue;
        }

        var newValue = Mathf.Clamp(floatValue, slider.minValue, slider.maxValue);

        if (Mathf.Approximately(newValue, floatValue))
            inputField.text = newValue.ToString(CultureInfo.InvariantCulture);

        slider.value = newValue;
    }

    private void OnSliderValueChanged(float value)
    {
        var textValue = slider.wholeNumbers switch
        {
            true => Mathf.Round(value).ToString(CultureInfo.InvariantCulture),
            false => (value >= 1000 ? Mathf.Round(value) : Mathf.Round(value * 100) / 100).ToString(CultureInfo
                .InvariantCulture)
        };

        if (inputField.text != textValue)
            inputField.text = textValue;
    }
}
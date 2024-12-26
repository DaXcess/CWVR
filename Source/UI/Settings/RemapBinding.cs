using CWVR.Assets;
using CWVR.Input;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zorro.ControllerSupport;

#pragma warning disable CS0649

namespace CWVR.UI.Settings;

public class RemapBinding : MonoBehaviour
{
    [SerializeField] internal TextMeshProUGUI settingTitle;
    [SerializeField] internal Button settingButton;
    [SerializeField] internal TextMeshProUGUI settingButtonText;
    [SerializeField] internal Image settingButtonImage;

    private RemappableControl control;
    private PlayerInput playerInput;
    private int bindingIndex;

    internal int RebindIndex => bindingIndex;
    internal InputActionReference ActionReference => control.currentInput;
    
    private void Awake()
    {
        playerInput = InputHandler.Instance.m_playerInput;
    }

    internal void Setup(RemappableControl remappableControl, SFX_Instance hoverSound, SFX_Instance clickSound)
    {
        control = remappableControl;

        var sfx = GetComponentInChildren<UI_Sound>();
        sfx.hoverSound = hoverSound;
        sfx.clickSound = clickSound;

        Reload();
        
        settingButton.onClick.AddListener(OnClickRebind);
    }
    
    public void Reload()
    {
        bindingIndex = Mathf.Max(control.bindingIndex, 0) +
                           Mathf.Max(control.currentInput.action.GetBindingIndex(playerInput.currentControlScheme), 0);

        settingTitle.text = control.controlName;
        settingButtonImage.sprite = string.IsNullOrEmpty(playerInput.currentControlScheme)
            ? null
            : AssetManager.RemappableControls.icons[control.currentInput.action.bindings[bindingIndex].effectivePath];

        // Only show image if it actually has a sprite (otherwise it'll be a white rectangle)
        settingButtonImage.enabled = settingButtonImage.sprite != null;
    }

    public void OnFinishRebind()
    {
        settingButtonText.enabled = false;

        Reload();
    }
    
    private void OnClickRebind()
    {
        if (!RemapManager.Instance.StartRebind(this))
            return;

        settingButtonImage.enabled = false;
        settingButtonText.enabled = true;
    }
}
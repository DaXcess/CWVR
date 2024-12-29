using CWVR.Patches;
using CWVR.Player;
using CWVR.UI;
using UnityEngine;
using Zorro.ControllerSupport;

namespace CWVR.Experiments;

public class HotSwitcher : MonoBehaviour
{
    private void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.F8))
            ToggleVR();
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private void ToggleVR()
    {
        var wasVR = VRSession.InVR;
        
        Plugin.ToggleVR();

        if (wasVR && !VRSession.InVR)
            LeftVR();
        else if (!wasVR && VRSession.InVR)
            EnteredVR();
    }

    private void EnteredVR()
    {
        InputPatches.OnCreateInputHandler(InputHandler.Instance);

        FindObjectOfType<GameAPI>().gameObject.AddComponent<MainMenu>();
    }

    private void LeftVR()
    {
        InputPatches.OnLeaveVR();
        
        Destroy(FindObjectOfType<MainMenu>());
    }
}
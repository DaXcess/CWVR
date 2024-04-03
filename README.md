# Content Warning VR Mod

## THIS IS A PROOF OF CONCEPT, I AM NOT SURE IF I WILL TURN THIS INTO AN ACTUAL MOD

### Unity Library Dependencies

**Provided by mod:**
- Unity.XR.Interaction.Toolkit 2.5.3
- Unity.XR.Management 4.4.1
- Unity.XR.OpenXR 1.8.2
- Unity.XR.CoreUtils 2.2.3
- UnityEngine.SpatialTracking 2.1.10
- Unity.InputSystem 1.7.0

**Provided by CW:**
- No libraries needed as of yet

### Input

`Player.PlayerInput`, `GlobalInputHandler`

Will most likely prefix patch `Player.PlayerInput.SampeInput` (SampleInput? Spelling mistake?!?!?)

**Camera movement**:

Will still have to find out how to patch camera movement to use VR, since the `lookInput` set by `PlayerInput.SampeInput` would probably always be set to zero when using VR.

Maybe `PlayerController.SetRotations`? Or we patch `PlayerController.Look` and set the `data.playerLookValues`, but these are Vector2's so most likely that won't be it.

```csharp
// An idea of how the patched SetRotations could look like
private void SetRotations()
{
    this.player.data.lookDirection = VRSession.Instance.MainCamera.transform.forward;
    this.player.data.lookDirectionRight = VRSession.Instance.MainCamera.transform.right;
    this.player.data.lookDirectionUp = VRSession.Instance.MainCamera.transform.up;        
}
```
# 1.2.0

- Updated for the latest version of Content Warning
- Removed Workshop support in favor of Thunderstore/BepInEx

# 1.1.4

- Added additional scanning for OpenXR runtimes
- Changed the invite friend button to open the SteamVR overlay (if possible), otherwise it shows a modal
- Changed the BepInEx version of "InteractToZoom" to be enabled by default, just like the native version
- Fixed scrolling backwards in the inventory (By @gingerphoenix10 in https://github.com/DaXcess/CWVR/pull/27)
- Fixed sprint activation option being the wrong way around
- Fixed 'EnableOcclusionMesh' option being ignored in the BepInEx version

# 1.1.3

- Added unseeded* bindings for WMR
- Added unseeded* bindings for Valve Index
- Added unseeded* bindings for HTC Vive
- Added unseeded* bindings for HP Reverb

> \*No specialized bindings have been created for these controllers, manually editing the bindings is required 

# 1.1.2

- Added hot VR loading, by pressing `F8` while in the main menu to toggle VR on or off
- Added a button to reset VR keybinds in the controls menu
- Fixed an issue where unknown controller profiles couldn't rebind or use certain controls

# 1.1.1

- Added a popup in game when VR fails to load, allowing easier access to logs
- Fixed an issue where Content Loader would load this mod from the workshop incorrectly
- Fixed an issue where some settings wouldn't be applied when using BepInEx

# 1.1.0

- Added support for v1.19.e
- Added Steam Workshop support
- Added Occlusion Mesh as an optional optimization
- Added a Resolution Scale setting (also works on flatscreen)
- Added an Upscaling Filter setting (also works on flatscreen)
- Reworked input system
- Reworked settings system
- Reworked rebinding UI
- Removed the restart requirements when first using the VR mod

# 1.0.3

- Added a temporary patch to prevent Content Warning from breaking soft dependencies
- QoL improvements to settings menu

# 1.0.2

- Hotfix for custom controls
- Fixed UI interactions not working for the sponsors monitors
- Added compatibility with ContentSettings

# 1.0.1

**Time for more controllers!**

The most important part of this update is control rebinding. Controllers with specialized layouts (like the HTC vive or WMR) we're not able to play CWVR. Now you can go into the settings and change the bindings to your needs.

- Replaced the `Controls` menu in the settings with VR controls, that can be rebound at any time, whether you are in-game or not
- Fixed the defibrillator not using VR controls as input
- Fixed the quota and views UI not being visible

# 1.0.0

**Time to get SpöökFamous!!**

No changelogs are necessary for this version, as it is the first version. Subsequent versions will contain a list of changes and new contributors.

### Verifying mod signature

CW // VR comes pre-packaged with a digital signature. You can use tools like GPG to verify the `CWVR.dll.sig` signature with the `CWVR.dll` plugin file.

The public key which can be used to verify the file is [9422426F6125277B82CC477DCF78CC72F0FD5EAD (OpenPGP Key Server)](https://keys.openpgp.org/vks/v1/by-fingerprint/9422426F6125277B82CC477DCF78CC72F0FD5EAD).
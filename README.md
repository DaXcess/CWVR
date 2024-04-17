# Content Warning VR Mod

> [!WARNING]
> This mod is not finished. While playable, expect stuff to straight up not work or break.

CW // VR is a [BepInEx](https://docs.bepinex.dev/) mod that adds fully fledged, 6DOF VR support into Content Warning, including fully motion controlled hands.

This mod, just like my Lethal Company VR mod, is powered by Unity's OpenXR plugin, and is thereby compatible with a wide range of headsets and runtimes, like Oculus, Virtual Desktop, SteamVR and many more!

# Install from source

To install the mod from the source code, you will first have to compile the mod. Instructions for this are available in [COMPILING.md](COMPILING.md).

Next up you'll need to grab a copy of some **Runtime Dependencies**. You can either grab these from [one of the releases](https://github.com/DaXcess/LCVR/releases), or if you truly want the no hand holding experience, you can retrieve them from a Unity project.

## Retrieving Runtime Dependencies from a Unity Project

First of all start by installing Unity 2022.3.10f1, which is the Unity version that Content Warning uses. Once you have installed the editor, create a new Unity project. If you are planning on adding prefabs to the mod, use the URP template and add the OpenXR plugin via the Unity package manager. Otherwise you can just use the VR template.

Make sure you set the scripting backend to Mono, and not to Il2Cpp (Unity will warn you when you try to compile a VR game with Il2Cpp enabled). You can now build your dummy game. Once the game is built you can navigate to it's `<Project Name>_Data/Managed` directory. There you will need to extract the following files:

- UnityEngine.SpatialTracking.dll
- Unity.XR.CoreUtils.dll
- Unity.XR.Interaction.Toolkit.dll
- Unity.XR.Management.dll
- Unity.XR.OpenXR.dll
- Unity.InputSystem.dll

And from the `<Project Name>_Data/Plugins/x86_64` directory:

- openxr_loader.dll
- UnityOpenXR.dll

## Install BepInEx

BepInEx is the modloader that CW // VR uses to mod the game. You can download BepInEx from their [GitHub Releases](https://github.com/BepInEx/BepInEx/releases) (LCVR currently targets BepInEx 5.4.22).

To install BepInEx, you can follow their [Installation Gude](https://docs.bepinex.dev/articles/user_guide/installation/index.html#installing-bepinex-1).

## Installing the mod

Once BepInEx has been installed and run at least once, you can start installing the mod.

First of all, in the `BepInEx/plugins` folder, create a new folder called `CWVR` (doesn't have to be named that specifically, but makes identification easier). Inside this folder, place the `CWVR.dll` file that was generated during the [COMPILING.md](COMPILING.md) steps.

After this has been completed, create a new directory called `RuntimeDeps` (has to be named exactly that) inside of the `CWVR` folder. Inside this folder you will need to put the DLLs that you have retrieved during the [Retrieving Runtime Depenencies](#retrieving-runtime-dependencies-from-a-unity-project) step. You can now run the game with LCVR installed.

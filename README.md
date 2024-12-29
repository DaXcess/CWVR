# Content Warning VR Mod

[![Thunderstore Version](https://img.shields.io/thunderstore/v/DaXcess/CWVR?style=for-the-badge&logo=thunderstore&logoColor=white)](https://thunderstore.io/c/content-warning/p/DaXcess/CWVR)
[![GitHub Version](https://img.shields.io/github/v/release/DaXcess/CWVR?style=for-the-badge&logo=github)](https://github.com/DaXcess/CWVR/releases/latest)
[![Thunderstore Downloads](https://img.shields.io/thunderstore/dt/DaXcess/CWVR?style=for-the-badge&logo=thunderstore&logoColor=white)](https://thunderstore.io/c/content-warning/p/DaXcess/CWVR)
[![GitHub Downloads](https://img.shields.io/github/downloads/DaXcess/CWVR/total?style=for-the-badge&logo=github)](https://github.com/DaXcess/CWVR/releases/latest)
[![Steam Downloads](https://img.shields.io/steam/downloads/3392710007?style=for-the-badge&logo=steam)](https://steamcommunity.com/sharedfiles/filedetails/?id=3392710007)
[![Steam Subscriptions](https://img.shields.io/steam/subscriptions/3392710007?style=for-the-badge&logo=steam)](https://steamcommunity.com/sharedfiles/filedetails/?id=3392710007)
<br/>
[![Release Build](https://img.shields.io/github/actions/workflow/status/DaXcess/CWVR/build-release.yaml?branch=main&style=for-the-badge&label=RELEASE)](https://github.com/DaXcess/CWVR/actions/workflows/build-release.yaml)
[![Debug Build](https://img.shields.io/github/actions/workflow/status/DaXcess/CWVR/build-debug.yaml?branch=dev&style=for-the-badge&label=DEBUG)](https://github.com/DaXcess/CWVR/actions/workflows/build-debug.yaml)

CW // VR is a MultiLoader mod that adds fully fledged, 6DOF VR support into Content Warning, including fully motion controlled hands.

This mod, just like my Lethal Company VR mod, is powered by Unity's OpenXR plugin, and is thereby compatible with a wide range of headsets and runtimes, like Oculus, Virtual Desktop, SteamVR and many more!

CW // VR supports both [BepInEx](https://docs.bepinex.dev/) and the [Content Warning Workshop](https://steamcommunity.com/app/2881650/workshop/), however compatibility will only be handled within the loader that CW // VR is loaded with.

### Discord Server

We have a dedicated section within the [Flat2VR Discord Server](https://discord.com/invite/ZFSCSDe)!

Look for the [`#cw-join`](https://discord.com/channels/747967102895390741/1236717861653712896) channel to join the Content Warning VR section.

# Usage

All usage information is stored on the [Thunderstore Page](https://thunderstore.io/c/content-warning/p/DaXcess/CWVR/), the rest of this README will only outline how to *manually* install the VR mod. Use either Thunderstore or the Steam Workshop if you're not planning to set aside a couple of hours to manually compile and install the mod.

# Install from source

To install the mod from the source code, you will first have to compile the mod. Instructions for this are available in [COMPILING.md](COMPILING.md).

Next up you'll need to grab a copy of some **Runtime Dependencies**. You can either grab these from [one of the releases](https://github.com/DaXcess/CWVR/releases), or if you truly want the no hand holding experience, you can retrieve them from a Unity project.

## Retrieving Runtime Dependencies from a Unity Project

First of all start by installing Unity 2022.3.10f1, which is the Unity version that Content Warning uses. Once you have installed the editor, create a new Unity project. If you are planning on adding prefabs to the mod, use the URP template and add the OpenXR plugin via the Unity package manager, otherwise you can just use the VR template.

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

## Installing without BepInEx

CW // VR supports Content Warning's built in mod loader.

To get started, head over to your game's directory. You can do that quickly by going to Steam -> Content Warning -> right click -> Manage -> Browse local files. Inside this folder, create a new folder called `Plugins` (unless it already exists).

Inside this `Plugins` folder, create a new folder called `CWVR`. Inside this folder, please the `CWVR.dll` and the `CWVR.Preload.dll` files that were generated during the [COMPILING.md](COMPILING.md) steps. Then make sure to rename `CWVR.Preload.dll` to `CWVR.preload.dll` (notice the lowercase P, very important!). Windows might prevent you from renaming a file where only the case has changed, so you may first rename it to any filename, and then you can rename it to `CWVR.preload.dll`.

After this has been completed, create a new directory called `RuntimeDeps` (has to be named exactly that) inside the `CWVR` folder. Here you will need to put the DLL files that you have retrieved during the [Retrieving Runtime Dependencies](#retrieving-runtime-dependencies-from-a-unity-project) step.

Finally, grab the [**Asset Bundle**](https://github.com/DaXcess/CWVR/raw/refs/heads/thunderstore/contentwarningvr) from one of the releases, and place them into the same folder as the `CWVR.dll` and `CWVR.preload.dll` files. This asset bundle file needs to be called `contentwarningvr`.

You can now run the game with CWVR installed.

## Installing with BepInEx

CW // VR supports BepInEx as a mod loader. You can download BepInEx from their [GitHub Releases](https://github.com/BepInEx/BepInEx/releases) (CWVR currently targets BepInEx 5.4.22).

To install BepInEx, you can follow their [Installation Guide](https://docs.bepinex.dev/articles/user_guide/installation/index.html#installing-bepinex-1).

Once BepInEx has been installed and run at least once, you can start installing the mod.

First of all, in the `BepInEx/plugins` folder, create a new folder called `CWVR` (doesn't have to be named that specifically, but makes identification easier). Inside this folder, place the `CWVR.dll` file that was generated during the [COMPILING.md](COMPILING.md) steps.

After this has been completed, create a new directory called `RuntimeDeps` (has to be named exactly that) inside the `CWVR` folder. Inside this folder you will need to put the following DLL files:

- UnityEngine.SpatialTracking.dll
- UnityEngine.XR.CoreUtils.dll
- Unity.XR.Interaction.Toolkit.dll
- Unity.XR.Management.dll
- Unity.XR.OpenXR.dll

These files should have been retrieved during the [Retrieving Runtime Dependencies](#retrieving-runtime-dependencies-from-a-unity-project) step, or from grabbing them from the latest release.

Next up, grab the [**Asset Bundle**](https://github.com/DaXcess/CWVR/raw/refs/heads/thunderstore/contentwarningvr) from one of the releases, and place them into the same folder as the `CWVR.dll` file. This asset bundle file needs to be called `contentwarningvr`.

Finally, in the `BepInEx/patchers` folder, also create a new folder called `CWVR` (again, doesn't have to be exact). Inside this folder, place the `CWVR.Preload.dll` file that was also generated during the [COMPILING.md](COMPILING.md) steps.

In this folder, also create a new directory called `RuntimeDeps` (again, has to be exactly named that), and place the following DLL files inside:

- openxr_loader.dll
- UnityOpenXR.dll

You can now run the game with CWVR installed.
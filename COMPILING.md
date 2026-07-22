## Prerequisites

* [.NET 10](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
    * Technically, any .NET version that supports [.NET Standard 2.0](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-0) will work.
* A legal copy of Hacknet on [Steam](https://store.steampowered.com/app/365450/Hacknet/) or [GOG](https://www.gog.com/en/game/hacknet_complete_edition)

---
## Linux Notes
If you are trying to compile on Linux, one of the things you need is the `Hacknet.exe` file. Thankfully, this is easy to get.

If you are on Steam:
1. Navigate to Hacknet in your library
2. Right-click, then click on Properties...
3. Go to the Compatibility tab
4. Check the checkbox you see there
5. Wait for Steam to download the Windows build of Hacknet
6. Install [Hacknet Pathfinder](https://github.com/Arkhist/Hacknet-Pathfinder) to your Windows build
7. Follow the steps below
8. Afterwards, you can uncheck the checkbox from before to switch back to the native Linux build (recommended)

If you are on GOG:
1. Simply download the Windows build of Hacknet for the steps below
2. Install [Hacknet Pathfinder](https://github.com/Arkhist/Hacknet-Pathfinder) to your Windows build
3. You can also download the native Linux build to use after (recommended)
---
## Files You Need
**NOTE**: Everything below will be placed in the `/lib` folder of the cloned `HacknetAP` repository.

* `Newtonsoft.Json.dll` from the [MultiClient.NET Library Repository](https://github.com/ArchipelagoMW/Archipelago.MultiClient.Net/tree/main/DLLs/net45)
* Hacknet Installation Directory:
    * `Hacknet.exe` (previously `HacknetPathfinder.exe`) (If you did not rename `HacknetPathfinder.exe`, rename it after placing it in the `/lib` folder.)
        * **Make sure it is the Pathfinder EXE**. The vanilla executable hides some things that Pathfinder exposes.
    * `FNA.dll`
* `<HacknetInstall>/BepInEx/core`:
    * `0Harmony.dll`
    * `BepInEx.Core.dll`
    * `BepInEx.Hacknet.dll`
    * `Mono.Cecil.dll`
    * `MonoMod.Utils.dll`
* `<HacknetInstall>/BepInEx/plugins`:
    * `PathfinderAPI.dll`
---
## Compiling
Simply run `dotnet build`. The mod DLL will be found in `HacknetAP-main/bin/Debug/netstandard2.0/HacknetAP.dll`.

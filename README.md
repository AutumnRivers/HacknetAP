# Hacknet: Archipelago
An [Archipelago](https://archipelago.gg/) client mod for Hacknet ([Steam](https://store.steampowered.com/app/365450/Hacknet/), [GOG](https://www.gog.com/en/game/hacknet))
---
Hi, my name is Bit, and if you're reading this, I'm already dead.

The circumstances of my death were... unique, to say the least. In order to prevent just anyone uncovering this rat's nest, I've scattered the programs you need across multiple different universes. Some very similar to your own, but most not.

Work smart, work hard, and work in unison to find me and avenge my death. Doing this now is our last chance, I think.

---
## Installation
* Download and install [Hacknet: Pathfinder](https://github.com/Arkhist/Hacknet-Pathfinder)
* Download the latest `HacknetAPClient.dll` release from the [Releases page](https://github.com/AutumnRivers/HacknetAP/releases/latest/)
* Place it in your `Hacknet/BepInEx/plugins` folder.
* <details>
  <summary>On Windows: Unblock the DLL (expand for screenshots)</summary>
    <img width="1114" height="630" alt="Windows File Explorer with HacknetAPClient.dll selected and the Properties button marked in red" src="https://github.com/user-attachments/assets/de2ac152-e0a3-4cc2-a792-43a51da0d995" />
    <img width="1114" height="630" alt="HacknetAPClient.dll Properties menu with the Unblock checkbox checked and marked in red" src="https://github.com/user-attachments/assets/325029b3-fe0c-4658-9f49-3097d44475c8" />
  </details>
* Follow [How To Use](#how-to-use)
---
## Additional Details
* Hacknet Pathfinder **DOES NOT WORK ON MAC/OSX**. As such, HacknetAP does not support it, either.
* HacknetAP is only tested with Archipelago 0.7.0 and will *not* work on versions earlier than 0.6.1! May not work on later versions.
---
## Things to Note
**These are important.** If you don't read them, you might think some things are broken.
* Any mission that requires you to download/upload an executable file is replaced with a mission to gain admin access on the target node, instead.
  * For example, in the mission, "Getting some tools together", you are usually required to download SSHCrack.exe.
  * Instead, you simply have to gain admin access to the node it's on, and then you can complete the mission.
* Striker's PC (DLC) usually has a fast administrator, but it's been replaced with a basic administrator.
  * This means, once you disconnect from it after gaining admin access, you have 15 seconds to complete the mission.
* When connecting to CSEC for the first time, you will be automatically disconnected. Simply reconnect to the node.
  * This is just something funky that happens due to the way Hacknet loads daemons.
  * You only need to do this once per savefile.
* If you have RAM limits enabled, do not panic when the game starts with full RAM on a new savefile.
  * Without full RAM, the tutorial exe cannot launch, and the player becomes softlocked.
  * Once the tutorial is finished, the RAM limit will kick in.
* If you want music rando, you need to place an `APMusic` folder alongside the mod DLL in the `/plugins` folder.
  * Inside this folder, you need to place at least 23 different music tracks that are of the `OGG Vorbis` file format.
  * Make sure it is OGG Vorbis. OGG Opus and OGG Theora **will not work** and will crash the game.
* Stuck in a mission? Reply with "abandon" or "quit" to pause the mission while you pick up another.
 
---
## Troubleshooting
* First, check [Pathfinder's common issues](https://github.com/Arkhist/Hacknet-Pathfinder#troubleshooting).

### "Hacknet crashes with a complaint about XNAWebRenderer!"
Run Hacknet with the `-disableweb` flag. This is an issue with Pathfinder, but it's most common on Linux.

### "I found a bug! How do I report it?"
Ideally, [make an issue](https://github.com/AutumnRivers/HacknetAP/issues).  
However, if you're in the [Archipelago Discord](https://discord.gg/archipelago), you can also bring it up in the 
`Hacknet` thread in the `future-game-design` forum channel. ([Direct Link](https://discord.com/channels/731205301247803413/1130229745452331120))

---
## Commands
Please see [COMMANDS.md](./COMMANDS.md).

---
## How To Use
* [Download/Install Archipelago](https://archipelago.gg/tutorial/Archipelago/setup/en#installing-the-archipelago-software)
* Download the [latest APWorld/YAML](https://github.com/AutumnRivers/Archipelago-Hacknet/releases)
* Edit the YAML as needed
* Place the APWorld in the `/custom_worlds` folder of your Archipelago installation
* [Generate and host a game with Archipelago](https://archipelago.gg/tutorial/Archipelago/setup/en#on-your-local-installation)
* Install the client mod, if you haven't already [[ ^ ]](#installation)
* Launch Hacknet, and enter the details as following:
    * `URI`: Full URI (`host:port`) where your game is hosted.
        * (e.g., `archipelago.gg:54321`, `localhost:38281`)
    * `Slot Name`: The name of *your* slot. (e.g., `Player1`, `Autumnet`)
    * `Room Pass`: If you've set up a password for your room, put it here.
* After entering the details, click `Connect To Archipelago`.
* That's it - now you can start a new save file.
    * If you're resuming a previous game of Archipelago, you should instead load the respective savefile.
* Whenever you wish to disconnect, you can simply close the game, or disconnect from the main menu. Either way will cleanly disconnect you from the server.

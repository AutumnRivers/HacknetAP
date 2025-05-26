# Hacknet: Archipelago
An [Archipelago](https://archipelago.gg/) client mod for Hacknet
---
Hi, my name is Bit, and if you're reading this, I'm already dead.

The circumstances of my death were... unique, to say the least. In order to prevent just anyone uncovering this rat's nest, I've scattered the programs you need across multiple different universes. Some very similar to your own, but most not.

Work smart, work hard, and work in unison to find me and avenge my death. Doing this now is our last chance, I think.

---
## Installation
* Download and install [Hacknet: Pathfinder](https://github.com/Arkhist/Hacknet-Pathfinder)
* Download the latest `HacknetAPClient.zip` release from the Releases page
* Extract the ZIP file to your base Hacknet folder (no, not to `BepInEx/plugins`!)
* Follow [How To Use](#how-to-use)
---
## Additional Details
* Hacknet Pathfinder **DOES NOT WORK ON MAC/OSX**. As such, HacknetAP does not support it, either.
* HacknetAP is only tested with Archipelago 0.6.2 and will *not* work on versions earlier than 0.6.1! May not work on later versions.
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
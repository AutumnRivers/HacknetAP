# Hacknet: Archipelago
An [Archipelago](https://archipelago.gg/) client mod for Hacknet
---
Hi, my name is Bit, and if you're reading this, I'm already dead.

The circumstances of my death were... unique, to say the least. In order to prevent just anyone uncovering this rat's nest, I've scattered the programs you need across multiple different universes. Some very similar to your own, but most not.

Work smart, work hard, and work in unison to find me and avenge my death. Doing this now is our last chance, I think.
---
## Installation
* Download the latest `HacknetAPClient.zip` release from the Releases page
* Extract the ZIP file to your base Hacknet folder (no, not to `BepInEx/plugins`!)
* ???
* Profit
---
## Additional Details
* This README is unfinished and will be better later
* HacknetAP is only tested with Archipelago 0.6.1 and will *not* work on earlier versions. May not work on later versions.
---
## Debug Commands
These can only be ran if Hacknet has debug commands enabled.
* `testdeathlink` - Tests a DeathLink crash.
* `pslotdata` - Prints the raw Slot Data in JSON.
* `debugsay` - Sends "Hello, World!" to the Archipelago text chat
* `debughint` - Sends "!hint SSHCrack" to the Archipelago text chat
* `debugpeek` - Checks to see what item is in Maiden Flight
* `setfactionaccess` - Sets faction access value:
    * `-1` - Disabled (don't use)
    * `0` - No Access
    * `1` - Entropy
    * `2` - Labyrinths (if labs is shuffled) / CSEC (otherwise)
    * `3` - CSEC
---
## User Commands
* `printitems` - Prints current local inventory to the terminal
* `printprog` - Prints progressive items to the terminal
* `archistatus` - Get your current connection status to Archipelago
* `archirestock` - Clears your bin folder and restocks it with your local inventory
* `rearchi` - Reconnects you to Archipelago, if you've disconnected
* `uncachechecks` - Forcibly sends out cached locations, run this after `rearchi` if you did any checks while disconnected
## User Commands
* `printitems` - Prints current local inventory to the terminal
* `printprog` - Prints progressive items to the terminal
* `archistatus` - Get your current connection status to Archipelago
* `archirestock` - Clears your bin folder and restocks it with your local inventory
* `rearchi` - Reconnects you to Archipelago, if you've disconnected
* `uncachechecks` - Forcibly sends out cached locations, run this after `rearchi` if you did any checks while disconnected
* `archisay <message/str>` - Sends `message` as a text message to the Archipelago lobby.
* `archifh` - Uses a ForceHack, if you have any available.
* `skipmission` - Skips the current mission, if you have any Mission Skips available.
---
## Debug Commands
These can only be ran if Hacknet has debug commands enabled.
* `testdeathlink` - Tests a DeathLink crash.
* `pslotdata` - Prints the raw Slot Data in JSON.
* `debugsay` - Sends "Hello, World!" to the Archipelago text chat
* `debughint` - Sends "!hint SSHCrack" to the Archipelago text chat
* `debugpeek` - Checks to see what item is in Maiden Flight
* `setfactionaccess <int>` - Sets faction access value:
    * `-1` - Disabled (don't use)
    * `0` - No Access
    * `1` - Entropy
    * `2` - Labyrinths (if labs is shuffled) / CSEC (otherwise)
    * `3` - CSEC
* `printserverdata` - Prints the current `userdata` value from Server Storage.
* `addtoptcrate <int>` - Adds `int` to the PointClicker rate.
* `addtoptcpassive <int>` - Adds `int` to the PointClicker passive rate multiplier.
* `addarchidebugentries` - Adds debug entries to the Archipelago IRC node, testing text chat and item retrievals.
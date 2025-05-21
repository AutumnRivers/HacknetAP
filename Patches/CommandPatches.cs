using System.Collections.Generic;
using Pathfinder.Event.Gameplay;

namespace HacknetArchipelago.Patches
{
    internal class CommandPatches
    {
        private static readonly List<string> _destructiveCommands = ["rm", "scp", "replace", "upload", "del", "mv"];

        public static void PreventModifyingPTCSaveData(CommandExecuteEvent cmdEvent)
        {
            var targetComp = cmdEvent.Os.connectedComp;
            string playerSave = cmdEvent.Os.defaultUser.name + ".pcsav";
            bool isDestructive = _destructiveCommands.Contains(cmdEvent.Args[0]);

            if (!isDestructive || targetComp.idName != "pointclicker" ||
                cmdEvent.Args[1] != playerSave) return;

            cmdEvent.Os.terminal.writeLine("Oops! You're not allowed to do that.");
            cmdEvent.Cancelled = true;
        }
    }
}

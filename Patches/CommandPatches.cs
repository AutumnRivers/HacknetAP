using System.Collections.Generic;
using HacknetArchipelago.Managers;
using Pathfinder.Event.Gameplay;

using Hacknet;

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

        private static readonly List<string> excludedExes = [
            "KaguyaTrials.exe", "SecurityTracer.exe", "Sequencer.exe"
            ];

        public static void PreventDownloadingUncollectedExecutables(CommandExecuteEvent cmdEvent)
        {
            if (cmdEvent.Args[0] != "scp") return;
            if (!cmdEvent.Args[1].EndsWith(".exe")) return;

            string executableName = cmdEvent.Args[1].Split('.')[0];
            bool hasExe = ArchipelagoItems.PlayerHasExecutable(executableName);
            OS os = cmdEvent.Os;

            if(!hasExe && !excludedExes.Contains(cmdEvent.Args[2]))
            {
                cmdEvent.Cancelled = true;
                os.write("You can't download that -- you haven't unlocked it yet!");
            }
        }
    }
}

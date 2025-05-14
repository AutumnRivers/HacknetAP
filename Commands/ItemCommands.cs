using Hacknet;
using System.Collections.Generic;

using HacknetArchipelago.Managers;

namespace HacknetArchipelago.Commands
{
    public class ItemCommands
    {
        private static readonly List<string> excludedComps = new()
        {
            "EnTechOfflineBackup", "dGibson"
        };

        public static void UseForceHack(OS os, string[] args)
        {
            if(InventoryManager._remainingForceHacks <= 0)
            {
                HacknetAPCore.SpeakAsSystem("You don't have any remaining ForceHacks!");
                return;
            }

            var target = os.connectedComp;

            if(target.PlayerHasAdminPermissions())
            {
                os.terminal.writeLine("ERROR : You already have admin permissions in this node!");
                return;
            } else if(excludedComps.Contains(target.idName))
            {
                os.terminal.writeLine("ERROR : Unable to use ForceHack on this node!");
                return;
            }

            target.giveAdmin(os.thisComputer.ip);
            target.log($"FORCEHACK_ACTIVATED_FROM_{os.thisComputer.ip}");
            target.admin = new Administrator();
            target.traceTime = -1;
            target.securityLevel = 0;
            target.firewall = null;
            target.hasProxy = false;
            InventoryManager._remainingForceHacks--;
            os.terminal.writeLine($"SUCCESS : You have {InventoryManager._remainingForceHacks} remaining ForceHacks.");
        }

        private static readonly List<string> excludedMissions = new()
        {
            "Bit -- Termination"
        };

        public static void UseMissionSkip(OS os, string[] args)
        {
            if(InventoryManager._remainingMissionSkips <= 0)
            {
                HacknetAPCore.SpeakAsSystem("You don't have any remaining Mission Skips!");
                return;
            }

            var mission = os.currentMission;

            if(mission == null)
            {
                os.terminal.writeLine("ERROR : You don't have an assigned mission!");
                return;
            } else if(excludedMissions.Contains(mission.email.subject))
            {
                os.terminal.writeLine("ERROR : You cannot use a Mission Skip on this mission!");
                return;
            }

            mission.goals.Clear();
            InventoryManager._remainingMissionSkips--;
            os.terminal.writeLine($"SUCCESS : You have {InventoryManager._remainingMissionSkips} remaining Mission Skips.");
            os.terminal.writeLine("(You can now reply to the mission email / posting, and it will complete.)");
        }

        public static void CheckRemainingInventory(OS os, string[] args)
        {
            string remSkips = string.Format("{0} remaining Mission Skips.", InventoryManager._remainingMissionSkips);
            string remFhs = string.Format("{0} remaining ForceHacks.", InventoryManager._remainingForceHacks);

            os.terminal.writeLine("You have:");
            os.terminal.writeLine(remSkips);
            os.terminal.writeLine(remFhs);
        }
    }
}

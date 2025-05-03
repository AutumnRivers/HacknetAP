using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hacknet;

namespace HacknetArchipelago.Commands
{
    public class ArchipelagoDebugCommands
    {
        public static void PrintSlotData(OS os, string[] args)
        {
            if (!checkIfDebugIsEnabled()) return;

            os.terminal.writeLine(HacknetAPCore.SlotData.GetRawSlotData());
        }

        public static void TestSayCommand(OS os, string[] args)
        {
            if (!checkIfDebugIsEnabled()) return;

            HacknetAPCore.ArchipelagoSession.Say("[AUTOMATED] Hello, World! From Hacknet!");
        }

        public static void TestHintCommand(OS os, string[] args)
        {
            if (!checkIfDebugIsEnabled()) return;

            HacknetAPCore.ArchipelagoSession.Say("!hint SSHCrack");
        }

        public static async void TestPeekLocation(OS os, string[] args)
        {
            if (!checkIfDebugIsEnabled()) return;

            var testItems = await HacknetAPCore.ArchipelagoSession.Locations.ScoutLocationsAsync(1);
            var testItem = testItems[1];

            StringBuilder resultBuilder = new("At Maiden Flight:\n");
            resultBuilder.Append("You can find ");
            resultBuilder.Append(testItem.ItemDisplayName);
            resultBuilder.Append(" for ");
            resultBuilder.Append(testItem.Player.Name);
            os.terminal.writeLine(resultBuilder.ToString());
        }

        public static void DebugSetFactionAccess(OS os, string[] args)
        {
            if (!checkIfDebugIsEnabled()) return;

            if (int.TryParse(args[0], out int access))
            {
                FactionAccess factionAccess = (FactionAccess)access;
                HacknetAPCore._factionAccess = factionAccess;
                os.terminal.writeLine($"Set faction access to {Enum.GetName(typeof(FactionAccess), factionAccess)} ({access})");
            } else
            {
                os.terminal.writeLine("Invalid Argument - argument must be a number");
                os.validCommand = false;
                os.commandInvalid = true;
            }
        }

        private static bool checkIfDebugIsEnabled()
        {
            var isEnabled = OS.DEBUG_COMMANDS;

            if(!isEnabled)
            {
                HacknetAPCore.SpeakAsSystem("You cannot run this command while debug commands are disabled!");
            }

            return isEnabled;
        }
    }
}

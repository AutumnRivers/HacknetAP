using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net.Enums;
using Hacknet;
using HacknetArchipelago.Daemons;
using HacknetArchipelago.Managers;

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

        public static void DebugPrintStorage(OS os, string[] args)
        {
            if (!checkIfDebugIsEnabled()) return;

            var serverStorage = HacknetAPCore.ArchipelagoSession.DataStorage[Scope.Slot, "userdata"].To<HacknetArchipelagoUserData>();

            os.terminal.writeLine("USER DATA:");
            os.terminal.writeLine($"Faction Access: {serverStorage.StoredFactionAccess}");
            os.terminal.writeLine($"Shell Limit: {serverStorage.StoredShellLimit}");
            os.terminal.writeLine($"RAM Limit: {serverStorage.StoredRAMLimit}");
            os.terminal.writeLine($"Mission Skips: {serverStorage.RemainingMissionSkips}");
            os.terminal.writeLine($"ForceHacks: {serverStorage.RemainingForceHacks}");
            os.terminal.writeLine("END");
        }

        public static void AddToConstantRate(OS os, string[] args)
        {
            if (!checkIfDebugIsEnabled()) return;

            if (int.TryParse(args[1], out int rate))
            {
                if (rate < 0)
                {
                    os.terminal.writeLine("Invalid Argument - Value must be more than -1");
                    os.commandInvalid = true;
                    return;
                }

                PointClickerManager.ChangeRateMultiplier(rate);
            }
            else
            {
                os.terminal.writeLine("Invalid Argument - argument must be a number");
                os.commandInvalid = true;
            }
        }

        public static void AddToPassiveRate(OS os, string[] args)
        {
            if (!checkIfDebugIsEnabled()) return;

            if (int.TryParse(args[1], out int rate))
            {
                if (rate < 0)
                {
                    os.terminal.writeLine("Invalid Argument - Value must be more than -1");
                    os.commandInvalid = true;
                    return;
                }

                PointClickerManager.ChangePointClickerPassiveRate(rate);
                os.terminal.writeLine($"New passive rate: {PointClickerManager.PassivePoints}");
            }
            else
            {
                os.terminal.writeLine("Invalid Argument - argument must be a number");
                os.commandInvalid = true;
            }
        }

        public static void CheckIfPlayerHasExecutable(OS os, string[] args)
        {
            if (!checkIfDebugIsEnabled()) return;

            string execName = args[1];

            os.write(ArchipelagoItems.PlayerHasExecutable(execName).ToString());
        }

        public static void AddToLocalInventory(OS os, string[] args)
        {
            if (!checkIfDebugIsEnabled()) return;

            string itemName = string.Join(" ", args.Skip(1));

            os.write("THIS IS MOSTLY COSMETIC, DON'T USE IF YOU DON'T KNOW WHAT YOU'RE DOING");
            InventoryManager.AddToInventory(itemName, "System1");
        }

        public static void CheckApWorldCompat(OS os, string[] args)
        {
            os.write("Client Mod Version: " + HacknetAPCore.ModVer);
            os.write("Target APW Version: " + HacknetAPCore.TARGET_APWORLD);
            os.write("User APW Version: " + HacknetAPCore.SlotData.APWorldVersionUsed);

            if (HacknetAPCore.TARGET_APWORLD == HacknetAPCore.SlotData.APWorldVersionUsed)
            {
                os.write("Yep, you're good to go :thumbs_up:");
            }
            else
            {
                os.write("You are not using the target APWorld version for this version of the client mod.\n" +
                         "You might run into issues.");
            }
        }

        public static void GetIdOfCurrentNode(OS os, string[] args)
        {
            if(!checkIfDebugIsEnabled()) return;

            var node = os.connectedComp;
            if (node == null)
            {
                os.write("Not connected a node!");
                return;
            }
            os.write(node.idName);
        }

        private static readonly List<ArchipelagoIRCEntry> TestEntries = new()
        {
            new("Test User", "Lorem ipsum dolor sit amet."),
            new(ArchipelagoManager.PlayerName, "Consectetur adipiscing elit."),
            new ArchipelagoItemIRCEntry("Test User", "Other User", "Progression Item", "Quick Brown Fox", ItemFlags.Advancement),
            new ArchipelagoItemIRCEntry(ArchipelagoManager.PlayerName, ArchipelagoManager.PlayerName,
                "Trap", "Three Blind Mice", ItemFlags.Trap),
            new ArchipelagoItemIRCEntry("Test User", ArchipelagoManager.PlayerName, "Useful Item", "The TARDIS",
                ItemFlags.NeverExclude),
            new ArchipelagoItemIRCEntry(ArchipelagoManager.PlayerName, "Other User", "Filler Item",
                "Cool Beans", ItemFlags.None)
        };

        public static void AddTestEntriesToIRC(OS os, string[] args)
        {
            // No debug check

            if(args.Length >= 2)
            {
                if (args[1] == "--clear")
                {
                    ArchipelagoIRCDaemon.ArchipelagoEntries.Clear();
                    os.terminal.writeLine("Cleared current entries!");
                }
            }
            foreach(var entry in TestEntries)
            {
                ArchipelagoIRCDaemon.GlobalInstance.AddIRCEntry(entry);
            }
            os.terminal.writeLine("Debug entries added!");
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

using System.Linq;
using System.Text;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Hacknet;

namespace HacknetArchipelago.Commands
{
    public class ArchipelagoUserCommands
    {
        private static bool isDebug = OS.DEBUG_COMMANDS;

        public static void ViewPlayerInventory(OS os, string[] args)
        {
            if(!HacknetAPCore._localInventory.Any())
            {
                WriteToTerminal("Your Archipelago inventory is empty!");
                return;
            }

            StringBuilder resultBuilder = new("--- LOCAL INVENTORY:\n");
            resultBuilder.Append("Local Inventory only shows executable files.\n");
            foreach(var item in HacknetAPCore._localInventory)
            {
                resultBuilder.Append($"* {item}\n");
            }
            resultBuilder.Append("--- END LOCAL INVENTORY");
            WriteToTerminal(resultBuilder.ToString());
        }

        public static void ViewProgressiveItems(OS os, string[] args)
        {
            StringBuilder resultBuilder = new("PROGRESSIVE ITEMS:\n");
            resultBuilder.Append("Faction Access: ");
            if (!HacknetAPCore.SlotData.EnableFactionAccess) { resultBuilder.Append("N/A\n"); }
            else
            {
                switch(HacknetAPCore._factionAccess)
                {
                    case FactionAccess.NoAccess:
                        resultBuilder.Append("None");
                        break;
                    case FactionAccess.Entropy:
                        resultBuilder.Append("Entropy");
                        break;
                    case FactionAccess.LabyrinthsOrCSEC:
                        resultBuilder.Append(HacknetAPCore.SlotData.ShuffleLabyrinths ?
                            "Labyrinths" : "CSEC");
                        break;
                    case FactionAccess.CSEC:
                        resultBuilder.Append("CSEC");
                        break;
                    default:
                        resultBuilder.Append("Unknown");
                        break;
                }
                if (isDebug) resultBuilder.Append($" {(int)HacknetAPCore._factionAccess}");
                resultBuilder.Append("\n");
            }
            if(HacknetAPCore.SlotData.LimitsShuffle == HacknetAPSlotData.LimitsMode.Disabled)
            {
                resultBuilder.Append("Shells: N/A\nRAM: N/A");   
            } else
            {
                if(HacknetAPCore.SlotData.LimitsShuffle == HacknetAPSlotData.LimitsMode.EnableAllLimits
                    || HacknetAPCore.SlotData.LimitsShuffle == HacknetAPSlotData.LimitsMode.OnlyShells
                    || HacknetAPCore.SlotData.LimitsShuffle == HacknetAPSlotData.LimitsMode.OnlyShellsZero)
                {
                    resultBuilder.Append($"Shells: {HacknetAPCore._shellLimit}");
                } else
                {
                    resultBuilder.Append("Shells: N/A");
                }
                resultBuilder.Append("\n");

                if(HacknetAPCore.SlotData.LimitsShuffle == HacknetAPSlotData.LimitsMode.EnableAllLimits
                    || HacknetAPCore.SlotData.LimitsShuffle == HacknetAPSlotData.LimitsMode.OnlyRAM)
                {
                    resultBuilder.Append("RAM: " + HacknetAPCore._ramLimit);
                } else
                {
                    resultBuilder.Append("RAM: N/A");
                }
            }
            WriteToTerminal(resultBuilder.ToString());
        }

        public static void GetArchipelagoStatus(OS os, string[] args)
        {
            StringBuilder resultBuilder = new("--- ARCHIPELAGO STATUS ---");
            bool connected = HacknetAPCore.ArchipelagoSession.Socket.Connected;
            if(!connected)
            {
                resultBuilder.Append("Not Connected...\n" +
                    "Reconnect with 'archirec'");
                return;
            }
            resultBuilder.Append("Connected\n");
            resultBuilder.Append($"Slot: {HacknetAPCore.ArchipelagoSession.Players.ActivePlayer.Name}\n");
            resultBuilder.Append($"URI: {HacknetAPCore.ArchipelagoSession.Socket.Uri}");
            if (isDebug) resultBuilder.Append("\nDebug Text Mode: Disabled");
            WriteToTerminal(resultBuilder.ToString());
        }

        public static void TestCrashDeathLink(OS os, string[] args)
        {
            if(!OS.DEBUG_COMMANDS)
            {
                os.commandInvalid = true;
                os.validCommand = false;
                return;
            }
            DeathLink testDL = new("TestPlayer", "You activated a test DeathLink crash!");
            HacknetAPCore.HandleDeathLink(testDL);
        }

        public static void ForceRestockExecutables(OS os, string[] args)
        {
            HacknetAPCore.ForceRestockItems();
        }

        public static void ReconnectToArchipelago(OS os, string[] args)
        {
            if (HacknetAPCore.CachedConnectionDetails.Item1 == null)
            {
                HacknetAPCore.Logger.LogError("Tried to reconnect to Archipelago, but no connection details were saved!");
                HacknetAPCore.SpeakAsSystem("Unable to connect to Archipelago -- check detached console.");
                return;
            }

            var conDetails = HacknetAPCore.CachedConnectionDetails;
            var conResult = HacknetAPCore.ConnectToArchipelago(conDetails.Item1, conDetails.Item2, conDetails.Item3);
            if(!conResult.Successful)
            {
                HacknetAPCore.SpeakAsSystem("Unable to connect to Archipelago -- check detached console.");
            } else
            {
                HacknetAPCore.SpeakAsSystem("Successfully reconnected to Archipelago! Cached locations will be sent, if they exist.");
            }
        }

        public static void ForceSendCachedLocations(OS os, string[] args)
        {
            if(HacknetAPCore._cachedChecks.Count == 0)
            {
                HacknetAPCore.SpeakAsSystem("There were no cached locations to send!");
                return;
            }

            HacknetAPCore.SendCachedLocations();
            HacknetAPCore.SpeakAsSystem("Sent out cached locations");
        }

        public static void SayCommand(OS os, string[] args)
        {
            if(args.Length <= 1)
            {
                WriteToTerminal("ERROR : Not enough arguments");
                return;
            }

            var content = args.Skip(1).ToArray();
            var fullContent = string.Join(" ", content);

            HacknetAPCore.ArchipelagoSession.Say(fullContent);
        }

        private static void WriteToTerminal(string message)
        {
            OS.currentInstance.terminal.writeLine(message);
        }
    }
}

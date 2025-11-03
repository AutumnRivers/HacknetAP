using Hacknet;
using HacknetArchipelago.Managers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HacknetArchipelago
{
    internal static class ArchipelagoItems
    {
        public static readonly Dictionary<int, Dictionary<string, string>> ArchipelagoItemToData = new()
        {
            { 21, new() { { "FTPBounce", PortExploits.crackExeData[21] } } },
            { 22, new() { { "SSHCrack", PortExploits.crackExeData[22] } } },
            { 25, new() { { "SMTPOverflow", PortExploits.crackExeData[25] } } },
            { 80, new() { { "WebServerWorm", PortExploits.crackExeData[80] } } },
            { 1433, new() { { "SQL_MemCorrupt", PortExploits.crackExeData[1433] } } },
            { 104, new() { { "KBTPortTest", PortExploits.crackExeData[104] } } },
            { 3659, new() { { "eosDeviceScan", PortExploits.crackExeData[13] } } },
            { 111, new() // DEC Suite
            {
                { "Decypher", PortExploits.crackExeData[9] },
                { "DECHead", PortExploits.crackExeData[10] }
            } },
            { 113, new() { { "OpShell", PortExploits.crackExeData[41] } } },
            { 114, new() { { "Tracekill", PortExploits.crackExeData[12] } } },
            { 115, new() { { "ThemeChanger", PortExploits.crackExeData[14] } } },
            { 116, new() { { "Clock", PortExploits.crackExeData[11] } } },
            { 117, new() { { "HexClock", PortExploits.crackExeData[16] } } },
            { 1337, new() { { "Hacknet", PortExploits.crackExeData[15] } } },
            // Labyrinths
            { 6881, new() { { "TorrentStreamInjector", PortExploits.crackExeData[6881] } } },
            { 443, new() { { "SSLTrojan", PortExploits.crackExeData[443] } } },
            { 221, new() { { "FTPSprint", PortExploits.crackExeData[211] } } },
            { 120, new() // Mem Suite
            {
                { "MemForensics", PortExploits.crackExeData[33] },
                { "MemDumpGenerator", PortExploits.crackExeData[34] }
            } },
            { 192, new() { { "PacificPortcrusher", PortExploits.crackExeData[192] } } },
            { 122, new() { { "ComShell", PortExploits.crackExeData[36] } } },
            { 123, new() { { "NetmapOrganizer", PortExploits.crackExeData[35] } } },
            { 124, new() { { "DNotes", PortExploits.crackExeData[37] } } },
            { 125, new() { { "Tuneswap", PortExploits.crackExeData[39] } } },
            { 126, new() { { "ClockV2", PortExploits.crackExeData[38] } } },
            { 193, new() { { "SignalScramble", PortExploits.crackExeData[32] } } },
            // Executable Packs
            // Regional
            { 1000, new() // Intro
            {
                { "FTPBounce", PortExploits.crackExeData[21] },
                { "SSHCrack", PortExploits.crackExeData[22] }
            } },
            { 1001, new() // Entropy
            {
                { "SMTPOverflow", PortExploits.crackExeData[25] },
                { "WebServerWorm", PortExploits.crackExeData[80] },
                { "SQL_MemCorrupt", PortExploits.crackExeData[1433] },
                { "eosDeviceScan", PortExploits.crackExeData[13] },
                { "Clock", PortExploits.crackExeData[11] }
            } },
            { 1002, new() // CSEC
            {
                { "Decypher", PortExploits.crackExeData[9] },
                { "DECHead", PortExploits.crackExeData[10] },
                { "KBTPortTest", PortExploits.crackExeData[104] },
                { "ThemeChanger", PortExploits.crackExeData[14] }
            } },
            { 1003, new() // Labyrinths
            {
                { "TorrentStreamInjector", PortExploits.crackExeData[6881] },
                { "SSLTrojan", PortExploits.crackExeData[443] },
                { "FTPSprint", PortExploits.crackExeData[211] },
                { "MemForensics", PortExploits.crackExeData[33] },
                { "MemDumpGenerator", PortExploits.crackExeData[34] },
                { "PacificPortcrusher", PortExploits.crackExeData[192] },
                { "ComShell", PortExploits.crackExeData[36] },
                { "NetmapOrganizer", PortExploits.crackExeData[35] },
                { "SignalScramble", PortExploits.crackExeData[32] }
            } },
            { 1004, new() // Finale
            {
                { "Tracekill", PortExploits.crackExeData[12] },
                { "OpShell", PortExploits.crackExeData[41] }
            } },
            // Practicality
            { 1005, new() // Portcrushers
            {
                { "FTPBounce", PortExploits.crackExeData[21] },
                { "SSHCrack", PortExploits.crackExeData[22] },
                { "SMTPOverflow", PortExploits.crackExeData[25] },
                { "WebServerWorm", PortExploits.crackExeData[80] },
                { "SQL_MemCorrupt", PortExploits.crackExeData[1433] },
                { "eosDeviceScan", PortExploits.crackExeData[13] }, // "that's not a portcrusher!" idc
                { "KBTPortTest", PortExploits.crackExeData[104] },
                { "Tracekill", PortExploits.crackExeData[12] },
                { "ThemeChanger", PortExploits.crackExeData[14] },
                { "Decypher", PortExploits.crackExeData[9] },
                { "DECHead", PortExploits.crackExeData[10] }
            } },
            { 1006, new() // Labyrinths Portcrushers
            {
                { "TorrentStreamInjector", PortExploits.crackExeData[6881] },
                { "SSLTrojan", PortExploits.crackExeData[443] },
                { "FTPSprint", PortExploits.crackExeData[211] },
                { "PacificPortcrusher", PortExploits.crackExeData[192] },
                { "SignalScramble", PortExploits.crackExeData[32] },
                { "MemForensics", PortExploits.crackExeData[33] },
                { "MemDumpGenerator", PortExploits.crackExeData[34] }
            } },
            { 1007, new() // Clock Pack
            {
                { "Clock", PortExploits.crackExeData[11] },
                { "HexClock", PortExploits.crackExeData[16] },
                { "ClockV2", PortExploits.crackExeData[38] }
            } },
            { 1008, new() // Misc.
            {
                { "OpShell", PortExploits.crackExeData[41] },
                { "ComShell", PortExploits.crackExeData[36] },
                { "NetmapOrganizer", PortExploits.crackExeData[35] },
                { "DNotes", PortExploits.crackExeData[37] },
                { "Tuneswap", PortExploits.crackExeData[39] }
            } }
        };

        public static readonly List<string> ExecutableNames = new()
        {
            "FTPBounce", "SSHCrack", "SMTPOverflow", "WebServerWorm",
            "SQL_MemCorrupt", "KBTPortTest", "eosDeviceScan", "DEC Suite",
            "OpShell", "Tracekill", "ThemeChanger", "Clock", "HexClock",
            "HacknetEXE", "TorrentStreamInjector", "SSLTrojan", "FTPSprint",
            "Mem Suite", "PacificPortcrusher", "ComShell", "NetmapOrganizer",
            "DNotes", "Tuneswap", "ClockV2", "SignalScramble"
        };

        public static readonly List<string> LabyrinthsExecutableNames = new()
        {
            "TorrentStreamInjector", "SSLTrojan", "Mem Suite", "PacificPortcrusher", "ComShell",
            "NetmapOrganizer", "DNotes", "Tuneswap", "ClockV2", "SignalScramble"
        };

        public static long ArchipelagoDataToItem(string data)
        {
            var entry = ArchipelagoItemToData.FirstOrDefault(en => en.Value.Values.Contains(data));
            if (entry.Equals(new KeyValuePair<int, List<string>>())) return -1;
            return entry.Key;
        }

        public static string ArchipelagoDataToItemName(string data)
        {
            var entry = ArchipelagoItemToData.FirstOrDefault(en => en.Value.Values.Contains(data));
            if (entry.Key < 21) return null;
            var itemName = entry.Value.First(subEntry => subEntry.Value == data).Key;
            return itemName;
        }

        public static bool PlayerHasExecutable(string execName)
        {
            string alternateItemName = execName;
            if (execName == "FTPBounce") alternateItemName = "FTPSprint";
            if(InventoryManager.PlayerCollectedItem(execName) ||
                InventoryManager.PlayerCollectedItem(alternateItemName))
            {
                return true;
            } else if(ArchipelagoManager.SlotData.ExecutableGrouping != HacknetAPSlotData.ExecutableGroupingMode.Individually)
            {
                var allItems = InventoryManager.AllCollectedItemsNoPlayers;
                for(int idx = 0; idx < allItems.Count; idx++)
                {
                    var item = allItems[idx];
                    if (!item.EndsWith("Pack")) continue;
                    if (IsExecutableInPack(execName, item)) return true;
                }
            }
            return false;
        }

        public static readonly Dictionary<string, int> PackToID = new()
        {
            { "Intro Executable Pack", 1000 },
            { "Entropy Executable Pack", 1001 },
            { "CSEC Executable Pack", 1002 },
            { "Labyrinths Executable Pack", 1003 },
            { "Finale Executable Pack", 1004 },
            { "Portcrusher Pack", 1005 },
            { "Labyrinths Portcrusher Pack", 1006 },
            { "Clock Pack", 1007 },
            { "Misc. Executables Pack", 1008 }
        };

        public static bool IsExecutableInPack(string execName, string packName)
        {
            Console.WriteLine($"Checking if {execName} is in {packName}");
            if (!PackToID.ContainsKey(packName)) return false;
            var packID = PackToID[packName];
            var pack = ArchipelagoItemToData[packID];
            return pack.Any(pair => pair.Key == execName);
        }
    }
}

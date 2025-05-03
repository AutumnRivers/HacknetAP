using Hacknet;

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
            { 118, new() { { "Hacknet", PortExploits.crackExeData[15] } } },
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
            { 193, new() { { "SignalScramble", PortExploits.crackExeData[32] } } }
            // TODO: implement executable packs / regional groups
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
    }
}

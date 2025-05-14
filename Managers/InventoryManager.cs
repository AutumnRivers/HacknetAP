using Archipelago.MultiClient.Net;
using BepInEx;
using Hacknet;
using Pathfinder.Event.Loading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HacknetArchipelago.Managers
{
    public static class InventoryManager
    {
        internal static FactionAccess _factionAccess = FactionAccess.Disabled;
        internal static int _shellLimit = -1; // -1 = disabled
        internal static int _ramLimit = 0; // 0 = disabled
        internal static int _remainingMissionSkips = 0;
        internal static int _remainingForceHacks = 0;

        internal static Dictionary<string, int> _localInventory = [];

        internal static bool PlayerHasItem(string itemName)
        {
            if (itemName.IsNullOrWhiteSpace())
            {
                return false;
            }
            return _localInventory.ContainsKey(itemName);
        }

        internal static void AddToInventory(string itemName, int amount = 1)
        {
            if (!_localInventory.ContainsKey(itemName))
            {
                _localInventory.Add(itemName, amount);
            }
            else
            {
                _localInventory[itemName] += amount;
            }
        }

        internal static void ForceRestockItems()
        {
            var items = ArchipelagoManager.Session.Items.AllItemsReceived;
            PlayerManager.ClearPlayerBinaries();
            foreach (var item in items)
            {
                ArchipelagoManager.CollectArchipelagoItem(item, false, true);
            }
        }

        internal static void CheckItemsCacheOnLoad(OSLoadedEvent osLoadedEvent)
        {
            if (osLoadedEvent.Thrown || osLoadedEvent.Cancelled) return;
            HacknetAPCore.Logger.LogDebug("Successful OS load detected. Checking items cache...");
            HacknetAPCore._originalBsodText = osLoadedEvent.Os.crashModule.bsodText;
            ArchipelagoManager.ForceCheckItemsCache();

            if (ArchipelagoManager.SlotData.EnableFactionAccess && _factionAccess == FactionAccess.Disabled)
            {
                _factionAccess = FactionAccess.NoAccess;
            }

            if (ArchipelagoManager.SlotData.LimitsShuffle == HacknetAPSlotData.LimitsMode.OnlyShellsZero && _shellLimit == -1)
            {
                _shellLimit = 0;
            }
            else if ((ArchipelagoManager.SlotData.LimitsShuffle == HacknetAPSlotData.LimitsMode.EnableAllLimits ||
                ArchipelagoManager.SlotData.LimitsShuffle == HacknetAPSlotData.LimitsMode.OnlyShells) && _shellLimit <= 0)
            {
                _shellLimit = 1;
            }

            GetLocalInventoryFromServerInventory();
        }

        private static void GetLocalInventoryFromServerInventory()
        {
            bool itemsExist = ArchipelagoManager.Session.Items.AllItemsReceived.Count > 0;
            if (!itemsExist) return;
            var executableNames = ArchipelagoItems.ExecutableNames;
            var collectedExecutables = ArchipelagoManager.Session.Items.AllItemsReceived
                .Where(i => executableNames.Contains(i.ItemDisplayName));

            _localInventory.Clear();
            foreach (var exe in collectedExecutables)
            {
                if (_localInventory.ContainsKey(exe.ItemDisplayName)) continue;
                _localInventory.Add(exe.ItemDisplayName, 1);
            }

            if (OS.currentInstance == null) return;

            var playerBin = OS.currentInstance.thisComputer.getFolderFromPath("bin");
            var exeFiles = playerBin.files.Where(f => f.name.EndsWith(".exe"));

            Dictionary<string, string> exeToPack = new()
            {
                { "Decypher", "DEC Suite" },
                { "MemDumpGenerator", "Mem Suite" }
            };

            foreach (var file in exeFiles)
            {
                var cleanName = file.name.Split('.')[0];
                string exeName = cleanName;
                if (exeToPack.ContainsKey(exeName)) exeName = exeToPack[exeName];

                if (_localInventory.ContainsKey(exeName) || !ArchipelagoItems.ExecutableNames.Contains(exeName)) continue;
                _localInventory.Add(exeName, 1);
            }
        }
    }
}

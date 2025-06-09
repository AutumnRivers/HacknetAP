using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Models;
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
        public const int DEFAULT_RAM_LIMIT = 350;

        internal static FactionAccess _factionAccess = FactionAccess.Disabled;
        internal static int _shellLimit = -1; // -1 = disabled
        internal static int _ramLimit = 0; // 0 = disabled
        internal static int _remainingMissionSkips = 0;
        internal static int _remainingForceHacks = 0;

        internal static Dictionary<string, string> _localInventory = [];
        internal static Dictionary<string, List<string>> allCollectedItems = [];

        private static bool _initialized = false;

        internal static void AddNewItem(ItemInfo itemInfo)
        {
            var name = itemInfo.ItemDisplayName;
            var player = itemInfo.Player.Name + itemInfo.LocationId;

            if(!allCollectedItems.ContainsKey(name))
            {
                List<string> players = [player];
                allCollectedItems.Add(name, players);
            } else if (!allCollectedItems[name].Contains(player))
            {
                allCollectedItems[name].Add(player);
            }
        }

        internal static void AddNewItem(string itemName, List<string> playerValues)
        {
            if(!allCollectedItems.ContainsKey(itemName))
            {
                if(OS.DEBUG_COMMANDS)
                {
                    HacknetAPCore.Logger.LogDebug("Adding new item to inventory: " + itemName);
                    HacknetAPCore.Logger.LogDebug("Collected by: ");
                    foreach(var player in playerValues)
                    {
                        HacknetAPCore.Logger.LogDebug("* " + player);
                    }
                }
                allCollectedItems.Add(itemName, playerValues);
            } else
            {
                allCollectedItems[itemName] = playerValues;
            }
        }

        internal static bool PlayerAlreadyCollectedItem(ItemInfo itemInfo)
        {
            if (_initialized && itemInfo.Player.Name == "Server") return false; // Always collect server items after loading savedata
            bool keyExists = allCollectedItems.ContainsKey(itemInfo.ItemDisplayName);
            if (OS.DEBUG_COMMANDS)
            {
                HacknetAPCore.Logger.LogDebug("Checking if player collected item " + itemInfo.ItemDisplayName);
                HacknetAPCore.Logger.LogDebug("Sent by " + itemInfo.Player.Name + itemInfo.LocationId);
                HacknetAPCore.Logger.LogDebug("Name exists in collected items: " + keyExists);
                if(keyExists)
                {
                    foreach(var player in allCollectedItems[itemInfo.ItemDisplayName])
                    {
                        HacknetAPCore.Logger.LogDebug($"Already collected from {itemInfo.Player.Name}{itemInfo.LocationId}");
                    }
                }
            }
            if (!keyExists) return false;
            return allCollectedItems[itemInfo.ItemDisplayName].Contains(
                itemInfo.Player.Name + itemInfo.LocationId);
        }

        internal static bool PlayerHasItem(string itemName)
        {
            if (itemName.IsNullOrWhiteSpace())
            {
                return false;
            }
            return _localInventory.ContainsKey(itemName);
        }

        internal static void AddToInventory(string itemName, string player)
        {
            if (!_localInventory.ContainsKey(itemName))
            {
                _localInventory.Add(itemName, player);
            }
            else if (player != "Server")
            {
                _localInventory[itemName] = player;
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
            PointClickerManager.RefreshPointClickerDaemon();
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

            if((ArchipelagoManager.SlotData.LimitsShuffle == HacknetAPSlotData.LimitsMode.OnlyRAM ||
                ArchipelagoManager.SlotData.LimitsShuffle == HacknetAPSlotData.LimitsMode.EnableAllLimits) && _ramLimit == 0)
            {
                _ramLimit = DEFAULT_RAM_LIMIT;
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
                _localInventory.Add(exe.ItemDisplayName, exe.Player.Name);
            }

            _initialized = true;
        }
    }
}

using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using BepInEx.Logging;
using Hacknet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HacknetArchipelago.Managers
{
    public static class LocationManager
    {
        public const string GameString = HacknetAPCore.GameString;

        public static ArchipelagoSession Session => ArchipelagoManager.Session;
        private static ManualLogSource Logger => HacknetAPCore.Logger;

        internal static List<string> _collectedFlags = [];
        internal static List<string> _cachedChecks = [];

        internal static void SendCachedLocations()
        {
            if (!_cachedChecks.Any()) return;

            List<long> cachedLocationIDs = new();
            foreach (var location in _cachedChecks)
            {
                var locationID = Session.Locations.GetLocationIdFromName(GameString, location);
                if (locationID == -1) continue;
                cachedLocationIDs.Add(locationID);
            }

            SendArchipelagoLocations([.. cachedLocationIDs]);
        }

        internal static ReadOnlyCollection<long> _allCheckedLocations;

        internal static void SendArchipelagoLocations(long locationID, bool notify = true)
        {
            if(_allCheckedLocations.Contains(locationID)) return;
            
            Session.Locations.CompleteLocationChecks([locationID]);
            ArchipelagoManager.UpdateServerData();
            
            if(notify) NotifyItemFoundAtLocation(locationID);
        }

        internal static void SendArchipelagoLocations(long[] locationIDs, bool notify = true)
        {
            foreach (var locationId in locationIDs)
            {
                SendArchipelagoLocations(locationId, notify);
            }
        }

        private static async Task NotifyItemFoundAtLocation(long locationID)
        {
            if (OS.DEBUG_COMMANDS) { Logger.LogDebug($"Notifying about item found at location ID {locationID}"); }
            var locationItems = await Session.Locations.ScoutLocationsAsync([locationID]);
            if (!locationItems.Any())
            {
                if (OS.DEBUG_COMMANDS)
                {
                    Logger.LogWarning($"No items found for location ID {locationID}");
                }
                return;
            }
            var item = locationItems[locationID];
            var isPlayersItem = item.Player.Slot == ArchipelagoManager.PlayerSlot;
            var punctuation = ".";

            if (item.Flags.HasFlag(ItemFlags.Advancement))
            {
                punctuation = "!";
            }
            else if (item.Flags.HasFlag(ItemFlags.Trap))
            {
                punctuation = "...";
            }

            StringBuilder notifBuilder = new("You found ");
            if (isPlayersItem)
            {
                notifBuilder.Append("your ");
                notifBuilder.Append(item.ItemDisplayName);
            }
            else
            {
                notifBuilder.Append(item.ItemDisplayName);
                notifBuilder.Append(" for ");
                notifBuilder.Append(item.Player.Name);
            }
            notifBuilder.Append(punctuation);
            notifBuilder.Append(" (");
            notifBuilder.Append(item.LocationDisplayName);
            notifBuilder.Append(")");

            if (OS.DEBUG_COMMANDS) Logger.LogDebug(notifBuilder.ToString());

            HacknetAPCore.SpeakAsSystem(notifBuilder.ToString());
        }
    }
}

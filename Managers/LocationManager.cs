using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using BepInEx.Logging;
using Hacknet;
using System;
using System.Collections.Generic;
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

        internal static void SendArchipelagoLocations(long locationID)
        {
            Session.Locations.CompleteLocationChecks([locationID]);
            NotifyItemFoundAtLocation(locationID);
        }

        internal static void SendArchipelagoLocations(long[] locationIDs)
        {
            var checkedLocations = Session.Locations.AllLocationsChecked;

            List<long> nonCheckedLocations = locationIDs.ToList();
            List<long> readOnlyCheckedLocations = nonCheckedLocations.ToList();
            foreach (var id in readOnlyCheckedLocations)
            {
                if (checkedLocations.Contains(id))
                {
                    if (OS.DEBUG_COMMANDS)
                    {
                        Logger.LogInfo($"Not sending check for location ID {id} as it has already been checked");
                    }
                    nonCheckedLocations.Remove(id);
                }
            }

            Session.Locations.CompleteLocationChecks([.. nonCheckedLocations]);

            ArchipelagoManager.UpdateServerData();
        }

        internal static async void NotifyItemFoundAtLocation(long locationID)
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
            bool isPlayersItem = item.Player.Slot == ArchipelagoManager.PlayerSlot;
            string punctuation = ".";

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

using Hacknet;

using Pathfinder.Event.Gameplay;
using System.Collections.Generic;

namespace HacknetArchipelago
{
    public class CheckForFlagsPatch
    {
        public static void CheckFlagsForArchiLocations(OSUpdateEvent oSUpdateEvent)
        {
            if (oSUpdateEvent.Cancelled || oSUpdateEvent.Thrown) return;

            OS os = oSUpdateEvent.OS;

            var playerFlags = os.Flags.Flags;
            List<long> flagIDsToSend = new();

            foreach(var flag in playerFlags)
            {
                if (HacknetAPCore._collectedFlags.Contains(flag)) continue;
                HacknetAPCore._collectedFlags.Add(flag);

                bool isArchiLocation = ArchipelagoLocations.FlagToLocation.ContainsKey(flag);
                if (!isArchiLocation) continue;

                string locationName = ArchipelagoLocations.FlagToLocation[flag];
                long locationID = HacknetAPCore.ArchipelagoSession.Locations.GetLocationIdFromName(HacknetAPCore.GameString,
                    locationName);
                if(locationID == -1)
                {
                    if (locationName.StartsWith("Achievement") && !HacknetAPCore.SlotData.ShuffleAchievements) continue;
                    HacknetAPCore.Logger.LogError($"Flag Location \"{locationName}\" ({flag}) " +
                        "returned -1 when querying the server. Skipping...");
                    continue;
                }

                flagIDsToSend.Add(locationID);
            }

            if(flagIDsToSend.Count > 0)
            {
                HacknetAPCore.SendArchipelagoLocations([.. flagIDsToSend]);
            }
        }
    }
}

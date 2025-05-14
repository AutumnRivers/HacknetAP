using HarmonyLib;
using Hacknet;
using HacknetArchipelago.Managers;

namespace HacknetArchipelago.Patches
{
    [HarmonyPatch]
    public class MissionFunctionChecker
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MissionFunctions),nameof(MissionFunctions.runCommand))]
        public static void CheckMissionFunctionForArchiLocation(int value, string name)
        {
            if(ArchipelagoLocations.CommandToLocation.ContainsKey(name))
            {
                string locationName = ArchipelagoLocations.CommandToLocation[name];
                long locationID = HacknetAPCore.ArchipelagoSession.Locations.GetLocationIdFromName(HacknetAPCore.GameString,
                    locationName);
                if(locationID == -1)
                {
                    HacknetAPCore.Logger.LogError($"Mission Function Location \"{locationName}\" ({name}) returned -1");
                    return;
                }
                LocationManager.SendArchipelagoLocations(locationID);
            }
        }
    }
}

using HarmonyLib;
using Hacknet;
using HacknetArchipelago;
using HacknetArchipelago.Managers;

namespace HacknetAPClient.Patches.Computers;

[HarmonyPatch]
public class ComputerAdminPatch
{
    // PortHackExe.Completed is more reliable, but I cannot rely on it because
    // it is possible to get admin access through other means (like logging into the admin account)
    [HarmonyPostfix]
    [HarmonyPatch(typeof(Computer), "giveAdmin")]
    public static void CheckForAdminAccessCheck(Computer __instance, string ipFrom)
    {
        if(!HacknetAPCore.SlotData.ShuffleAdminAccess || ipFrom != OS.currentInstance.thisComputer.ip) return;

        var compId = __instance.idName;
        if (!ArchipelagoLocations.NodeToLocation.TryGetValue(compId, out var location))
        {
            if (OS.DEBUG_COMMANDS)
            {
                HacknetAPCore.Logger.LogWarning($"Couldn't send check for node ID {compId}: " +
                                                "Not a valid node location");
            }
            return;
        }

        var locationId = HacknetAPCore.ArchipelagoSession.Locations.GetLocationIdFromName(HacknetAPCore.GameString,
            location);
        if (locationId == -1)
        {
            if (OS.DEBUG_COMMANDS)
            {
                HacknetAPCore.Logger.LogWarning($"Couldn't send check for node ID {compId} ({location}): " +
                                                "Location doesn't exist on server");
            }
            return;
        }
        
        LocationManager.SendArchipelagoLocations(locationId);
    }
}
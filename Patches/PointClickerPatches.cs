using HarmonyLib;
using Hacknet;
using System;
using System.Collections.Generic;

namespace HacknetArchipelago.Patches
{
    [HarmonyPatch]
    public class PointClickerPatches
    {
        private static List<int> _collectedIndices = new();

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PointClickerDaemon), "PurchaseUpgrade")]
        public static void SendPointClickerUpgrades(PointClickerDaemon __instance, int index)
        {
            if (_collectedIndices.Contains(index)) return;
            _collectedIndices.Add(index);
            if (ArchipelagoLocations.UpgradeIndexToLocation.Count >= index) return;
            string locationName = ArchipelagoLocations.UpgradeIndexToLocation[index];

            long locationID = HacknetAPCore.ArchipelagoSession.Locations.GetLocationIdFromName(HacknetAPCore.GameString,
                locationName);
            if(locationID == -1)
            {
                if(OS.DEBUG_COMMANDS)
                {
                    HacknetAPCore.Logger.LogWarning($"PointClicker Upgrade Index {index} not found in " +
                        "multiworld. If you're not shuffling PointClicker, you can ignore this.");
                }
                return;
            }

            HacknetAPCore.SendArchipelagoLocations(locationID);
        }
    }
}

using System.Linq;
using HarmonyLib;
using Hacknet;
using HacknetArchipelago.Daemons;
using HacknetArchipelago.Extensions;

namespace HacknetArchipelago.Patches.Missions
{
    [HarmonyPatch]
    public class ReplaceMissionDaemons
    {
        public const string ENTROPY_COMP_ID = "entropy00";
        public const string CSEC_COMP_ID = "mainHub";

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Computer), nameof(Computer.initDaemons))]
        public static void ReplaceMissionListingDaemons(Computer __instance)
        {
            if(__instance.idName != CSEC_COMP_ID && __instance.idName != ENTROPY_COMP_ID) return;
            
            ArchipelagoMissionListingDaemon newDaemon = new(__instance, "AP Mission Listing",
                __instance.os);
            if(__instance.idName == ENTROPY_COMP_ID)
            {
                newDaemon.MissionSourceFolderPath = "Content/Missions/Entropy/StartingSet/";
            }
            __instance.daemons.Add(newDaemon);

            // removes duplicates
            __instance.daemons = __instance.daemons.DistinctBy(d => d.name).ToList();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MissionListingServer), "navigatedTo")]
        public static bool PreventViewingOldEntropyDaemon(MissionListingServer __instance)
        {
            if (__instance.comp.idName != ENTROPY_COMP_ID) return true;

            var sysFolder = __instance.comp.getFolderFromPath("sys");
            var bootModuleList = sysFolder.searchForFile("DefaultBootModule.txt");
            bootModuleList.data = "Archipelago Mission Listing";
            
            Programs.disconnect([], OS.currentInstance);
            OS.currentInstance.terminal.writeLine("Do not connect to the old daemon!\n" +
                                                  "(If you loaded a save, reconnect to the node.)");
            
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MissionListingServer), nameof(MissionListingServer.addMisison))]
        public static bool PreventAddingOldEntropyMissions(MissionListingServer __instance,
            ActiveMission m,
            bool injectToTop)
        {
            if (__instance.comp.idName != ENTROPY_COMP_ID) return true;

            var entropyComp = __instance.comp;
            var missionListing = entropyComp.getDaemon(typeof(ArchipelagoMissionListingDaemon));

            if (missionListing == null) return true;

            var archiDaemon = (ArchipelagoMissionListingDaemon)missionListing;
            
            archiDaemon.AddMissionToListing(m, injectToTop ? 0 : -1);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MissionHubServer), "navigatedTo")]
        public static bool PreventViewingOldCsecDaemon(MissionHubServer __instance)
        {
            var sysFolder = __instance.comp.getFolderFromPath("sys");
            var bootModuleList = sysFolder.searchForFile("DefaultBootModule.txt");
            bootModuleList.data = "Archipelago Mission Listing";
            
            Programs.disconnect([], OS.currentInstance);
            OS.currentInstance.terminal.writeLine("Do not connect to the old daemon!\n" +
                                                  "(If you loaded a save, reconnect to the node.)");
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MissionHubServer), "addMission")]
        public static bool ReplaceCsecHubAddMission(MissionHubServer __instance,
            ActiveMission mission,
            bool insertAtTop,
            int desiredInsertionIndex)
        {
            var csecComp = __instance.comp;
            var newDaemon = csecComp.getDaemon(typeof(ArchipelagoMissionListingDaemon));

            if (newDaemon == null) return true;

            var archiDaemon = (ArchipelagoMissionListingDaemon)newDaemon;
            
            archiDaemon.AddMissionToListing(mission, desiredInsertionIndex);
            return false;
        }
    }
}

using Hacknet;
using HacknetArchipelago.Daemons;
using HarmonyLib;
using HacknetArchipelago.Managers;

using Pathfinder.Util;

namespace HacknetArchipelago.Patches.Goals
{
    [HarmonyPatch]
    public class VIPPatch
    {
        public const string ENTROPY_ID = "entropy00";
        public const string CSEC_ID = "mainHub";

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ActiveMission), "finish")]
        public static void CheckEntropyCSECForGoal()
        {
            OS os = OS.currentInstance;

            var entropyComp = ComputerLookup.FindById(ENTROPY_ID);
            var entropy =
                (ArchipelagoMissionListingDaemon)entropyComp.getDaemon(typeof(ArchipelagoMissionListingDaemon));

            int remainingEntropyMissions = entropy.Missions.Count;
            if(remainingEntropyMissions <= 0)
            {
                ArchipelagoManager.EventManager.IsEntropyVIP = true;
                ArchipelagoManager.UpdateServerToggle("is_entropy_vip", true);
            }

            var csecComp = ComputerLookup.FindById(CSEC_ID);
            var csec = (ArchipelagoMissionListingDaemon)csecComp.getDaemon(typeof(ArchipelagoMissionListingDaemon));

            int remainingCsecMissions = csec.Missions.Count;
            if(remainingCsecMissions <= 0)
            {
                ArchipelagoManager.EventManager.IsCSECVIP = true;
                ArchipelagoManager.UpdateServerToggle("is_csec_vip", true);
            }

            if(remainingEntropyMissions <= 0 && remainingCsecMissions <= 0)
            {
                ArchipelagoManager.AttemptSendVictory();
            }
        }
    }
}

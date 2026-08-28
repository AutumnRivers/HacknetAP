using Hacknet;
using HarmonyLib;
using Pathfinder.Util;

namespace HacknetArchipelago.Patches.Missions
{
    [HarmonyPatch]
    public class ForcePlayerOnNaixPath
    {
        public const string NAIX_MISSION_PATH = "Content/Missions/lelzSec/IntroTestMission.xml";
        public const string NAIX_END_FUNCTION = "triggerThemeHackRevenge";
        public const string NAIX_PROXY_ID = "themeHackComp";
        public const string MACROSOFT_STORAGE_ID = "miscMacrosoftStorage"; // This node is part of "Hopefully that will do", the usual follow up to Aggression must be Punished.

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MissionFunctions),"runCommand")]
        public static bool CheckForNaixMissionPatch(int value, string name)
        {
            if (name != NAIX_END_FUNCTION) return true;

            if(HacknetAPCore.SlotData.ShuffleAdminAccess)
            {
                Computer naixProxyNode = ComputerLookup.FindById(NAIX_PROXY_ID);
                OS.currentInstance.netMap.discoverNode(naixProxyNode);
                Computer macrosoftStorageNode = ComputerLookup.FindById(MACROSOFT_STORAGE_ID);
                OS.currentInstance.netMap.discoverNode(macrosoftStorageNode);
                HacknetAPCore.Logger.LogDebug("Added Naix's proxy and Macrosoft Storage node to netmap because Shuffle Admin Access " +
                    "was enabled.");
            }

            ComputerLoader.loadMission(NAIX_MISSION_PATH); // quick, dirty, works

            return false;
        }
    }
}

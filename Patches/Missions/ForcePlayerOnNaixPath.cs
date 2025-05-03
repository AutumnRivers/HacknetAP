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

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MissionFunctions),"runCommand")]
        public static bool CheckForNaixMissionPatch(int value, string name)
        {
            if (name != NAIX_END_FUNCTION) return true;

            if(HacknetAPCore.SlotData.ShuffleAdminAccess)
            {
                Computer naixProxyNode = ComputerLookup.FindById(NAIX_PROXY_ID);
                OS.currentInstance.netMap.discoverNode(naixProxyNode);
                HacknetAPCore.Logger.LogDebug("Added Naix's proxy node to netmap because Shuffle Admin Access " +
                    "was enabled.");
            }

            ComputerLoader.loadMission(NAIX_MISSION_PATH); // quick, dirty, works

            return false;
        }
    }
}

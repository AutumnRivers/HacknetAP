using Hacknet;
using HarmonyLib;
using Pathfinder.Util;

namespace HacknetArchipelago.Patches.Missions
{
    [HarmonyPatch]
    public class StrikerBattlestationAddToNetmap
    {
        public const string STRIKER_REVENGE_FUNCTION = "scanAndStartDLCVenganceHack";
        // These two nodes can be entirely missed if you don't leave logs on Striker Cache during Striker's Archives/Stash mission.
        // Obtaining them would require restarting the entire save up until this point, this patch bypasses that.
        public const string STRIKER_BATTLESTATION_ID = "dAttackHome";
        public const string STRIKER_PROXY_ID = "dAttackSource";

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MissionFunctions),"runCommand")]
        public static bool AddStrikerBattlestationToNetmap(int value, string name)
        {
            if(name == STRIKER_REVENGE_FUNCTION && HacknetAPCore.SlotData.ShuffleAdminAccess)
            {
                Computer strikerBattlestationNode = ComputerLookup.FindById(STRIKER_BATTLESTATION_ID);
                OS.currentInstance.netMap.discoverNode(strikerBattlestationNode);
                Computer strikerProxyNode = ComputerLookup.FindById(STRIKER_PROXY_ID);
                OS.currentInstance.netMap.discoverNode(strikerProxyNode);
                HacknetAPCore.Logger.LogDebug("Added Striker_Battlestation and Striker_Proxy nodes to netmap because Shuffle Admin Access was enabled.");
            }
            return true;
        }
    }
}

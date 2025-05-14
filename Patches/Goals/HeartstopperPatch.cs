using Hacknet;
using HacknetArchipelago.Managers;
using HarmonyLib;

namespace HacknetArchipelago.Patches.Goals
{
    [HarmonyPatch]
    public class HeartstopperPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PorthackHeartDaemon), "BreakHeart")]
        public static void CheckPortHackHeartForGoal()
        {
            ArchipelagoManager.EventManager.BrokeHeart = true;
            ArchipelagoManager.UpdateServerToggle("achieved_heartstopper", true);

            ArchipelagoManager.AttemptSendVictory();
        }
    }
}

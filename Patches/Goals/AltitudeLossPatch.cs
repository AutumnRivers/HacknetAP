using Hacknet;
using HarmonyLib;
using HacknetArchipelago.Managers;

namespace HacknetArchipelago.Patches.Goals
{
    [HarmonyPatch]
    public class AltitudeLossPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(DLCCreditsDaemon),"EndDLC")]
        public static void CheckDLCCreditsForGoal()
        {
            ArchipelagoManager.EventManager.LostAltitude = true;
            ArchipelagoManager.UpdateServerToggle("achieved_altitudeloss", true);

            ArchipelagoManager.AttemptSendVictory();
        }
    }
}

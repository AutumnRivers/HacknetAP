using Hacknet;
using HarmonyLib;
using HacknetArchipelago.Managers;

namespace HacknetArchipelago.Patches.Goals
{
    [HarmonyPatch]
    public class VeteranPatch
    {
        public const string GOAL_PCID = "dGibson";

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PortHackExe),"Completed")]
        public static void CheckHackedCompForGoal(PortHackExe __instance)
        {
            if (__instance.target.idName != GOAL_PCID) return;

            ArchipelagoManager.EventManager.IsVeteran = true;
            ArchipelagoManager.UpdateServerToggle("achieved_veteran", true);

            ArchipelagoManager.AttemptSendVictory();
        }
    }
}

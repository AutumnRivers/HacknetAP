using System.Text;

using Hacknet;
using HarmonyLib;

namespace HacknetArchipelago.Patches
{
    [HarmonyPatch]
    public class LogLoadedMissions
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MissionSerializer), "restoreMissionFromFile",
            [typeof(string), typeof(int), typeof(string)], [ArgumentType.Normal, ArgumentType.Out, ArgumentType.Out])]
        public static void LogSerializedMissions(object __result)
        {
            if (!OS.DEBUG_COMMANDS) return;

            ActiveMission mission = (ActiveMission)__result;

            StringBuilder details = new("LOADED MISSION DETAILS:\n");
            details.AppendLine("Posting Title: " + mission.postingTitle);
            details.AppendLine("Posting Body: " + mission.postingBody);
            details.AppendLine("Subject: " + mission.email.subject);
            details.AppendLine();

            HacknetAPCore.Logger.LogDebug(details.ToString());
        }
    }
}

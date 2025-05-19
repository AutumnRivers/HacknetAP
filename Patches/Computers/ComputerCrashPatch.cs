using System.Text;

using HarmonyLib;

using Hacknet;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using System;
using HacknetArchipelago.Managers;

namespace HacknetArchipelago.Patches.Computers
{
    [HarmonyPatch]
    public class ComputerCrashPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Computer),"crash")]
        public static void ReplaceCrashTextPrefix(Computer __instance)
        {
            if (__instance.idName != OS.currentInstance.thisComputer.idName) return;

            if(!DeathLinkManager._crashCausedByDeathLink)
            {
                OS.currentInstance.crashModule.bsodText = HacknetAPCore._originalBsodText;
            }

            if (HacknetAPCore.DeathLinkService == null && !OS.DEBUG_COMMANDS) return;

            if(!DeathLinkManager._crashCausedByDeathLink && HacknetAPCore.DeathLinkService != null)
            {
                string playerName = HacknetAPCore.ArchipelagoSession.Players.ActivePlayer.Name;
                DeathLink deathLink = new(playerName,
                    $"{playerName}'s VM hard crashed...");
                HacknetAPCore.DeathLinkService.SendDeathLink(deathLink);
                return;
            }

            Console.WriteLine("Replacing BSOD text...");
            StringBuilder newBsodText = new();
            newBsodText.Append("/-----------------------------------------------------\\\n");
            newBsodText.Append("> DEATHLINK SERVICE : ACTIVE\n");
            newBsodText.Append("> Remote Crash caused by DeathLink Service\n");
            newBsodText.Append("> For more information, visit https://archipelago.gg/\n");
            newBsodText.Append("\\-----------------------------------------------------/\n\n\n");
            newBsodText.Append($"REASON FOR REMOTE DETONATION :\n{DeathLinkManager._lastDeathLinkCause}\n");
            newBsodText.Append("ERROR CODE : 1337\n\nThe system will now restart. Please wait...");
            OS.currentInstance.crashModule.bsodText = newBsodText.ToString();
            Console.WriteLine("New BSOD Text:\n" + newBsodText.ToString());
            DeathLinkManager._crashCausedByDeathLink = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(OS),"timerExpired")]
        public static void SendDeathlinkOnTraceBack()
        {
            if (DeathLinkManager.DLService == null) return;
            string playerName = ArchipelagoManager.PlayerName;

            DeathLink traceDL = new(playerName, playerName + " got traced back!");
            DeathLinkManager.SendDeathLink(traceDL);
        }
    }
}

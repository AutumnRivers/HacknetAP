using System;

using HarmonyLib;

using Hacknet;

namespace HacknetArchipelago.Patches
{
    [HarmonyPatch]
    public class ReplaceIntroText
    {
        static Random random = new();

        private static string finisher = HacknetAPCore.IntroTextFinishers[random.Next(HacknetAPCore.IntroTextFinishers.Count)];

        [HarmonyPrefix]
        [HarmonyPatch(typeof(IntroTextModule), nameof(IntroTextModule.Update))]
        static bool Prefix(IntroTextModule __instance)
        {
            if(HacknetAPCore.SkipBootIntroText)
            {
                __instance.text = [
                    "Hacknet: Archipelago " + HacknetAPCore.ModVer,
                    "Work smart, work hard, and work in unison."
                    ];
            } else
            {
                 __instance.text = [
                    "Hacknet: Archipelago " + HacknetAPCore.ModVer,
                    "~~~~~~~~~~~~~~~~~~~~~~~~~~~",
                    "To check your Archipelago connection status, run 'archistatus' at any time.",
                    " ",
                    "If you need support, open an issue on the GitHub repository, or ask in the Archipelago Discord in #future-game-planning.",
                    " ",
                    "As long as the Archipelago mod is installed, you are free to save and load as you like.",
                    " ",
                    "Work smart, work hard, and work in unison.",
                    "~~~~~~~~~~~~~~~~~~~~~~~~~~~",
                    "If you're reading this...",
                    finisher
                    ];
            }

            return true;
        }
    }
}
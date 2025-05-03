using System;

using HarmonyLib;

using Hacknet;
using static Hacknet.DLCHubServer;
using Hacknet.Mission;
using Hacknet.Factions;
using Pathfinder.Replacements;
using Pathfinder.Util;
using BepInEx;
using System.Linq;

namespace HacknetArchipelago.Patches
{
    public class MissionPatches
    {
        [HarmonyPatch]
        public class MissionCompletionPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(ActiveMission),"finish")]
            public static void Postfix(ActiveMission __instance)
            {
                HacknetAPCore.Logger.LogDebug($"Finished mission \"{__instance.email.subject}\"");
                SendOutLocationFromMission(__instance);
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(DLCHubServer), "PlayerAttemptCompleteMission")]
            public static void DLCMissionCheckPostfix(DLCHubServer __instance, ClaimableMission mission, bool ForceComplete)
            {
                OS os = OS.currentInstance;
                ActiveMission currentMission = mission.Mission;
                // There is exactly ONE (1) mission in Labyrinths which 
                if (__instance.MissionTextResponses.Any()) currentMission = __instance.os.currentMission;
                bool missionComplete = currentMission.isComplete(__instance.MissionTextResponses);

                if (!missionComplete && !ForceComplete) return;

                SendOutLocationFromMission(mission);
            }

            internal static void SendOutLocationFromMission(ActiveMission mission)
            {
                string missionName = mission.email.subject;
                var locations = ArchipelagoLocations.MissionToLocation;

                if (locations.TryGetValue(missionName, out string archiLocation))
                {
                    if(!HacknetAPCore.IsConnected)
                    {
                        HacknetAPCore._cachedChecks.Add(archiLocation);
                        HacknetAPCore.Logger.LogWarning($"Completed location \"{archiLocation}\", but player " +
                            "isn't currently connected to Archipelago. It's been saved for the next time the user " +
                            "connects to Archipelago.");
                        if (OS.DEBUG_COMMANDS) HacknetAPCore.SpeakAsSystem("Failed to send location -- offline");
                        return;
                    }

                    long locationID = HacknetAPCore.ArchipelagoSession.Locations.GetLocationIdFromName(HacknetAPCore.GameString,
                        archiLocation);
                    if (locationID > -1)
                    {
                        HacknetAPCore.SendArchipelagoLocations(locationID);
                    }
                    else
                    {
                        HacknetAPCore.Logger.LogError($"Location returned -1, probably doesn't exist in multiworld. " +
                            "Check your player settings in your YAML if this wasn't intentional.\n" +
                            $"Location Name: {archiLocation} / Location ID: {locationID}");
                        if (OS.DEBUG_COMMANDS) HacknetAPCore.SpeakAsSystem("Failed to send location -- location doesn't exist");
                    }
                }
                else
                {
                    HacknetAPCore.Logger.LogWarning($"Mission with email subject \"{missionName}\" not found in " +
                        "archi locations. If this is intended, you can ignore this message.");
                }
            }

            internal static void SendOutLocationFromMission(ClaimableMission mission)
            {
                SendOutLocationFromMission(mission.Mission);
            }
        }

        [HarmonyPatch]
        public class MissionModificationPatches
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(ActiveMission),"isComplete")]
            // This patch replaces goals that require a user to download/upload an executable
            // with goals to instead get admin access to the comp the exe file is from.
            //
            // This should prevent players from needing to download executables
            // they haven't unlocked yet.
            public static void ChangeDownloadExeGoals(ActiveMission __instance)
            {
                var goals = __instance.goals;
                for(int i = 0; i < goals.Count; i++)
                {
                    var goal = goals[i];
                    if(goal is FileDownloadMission downloadGoal)
                    {
                        if (!downloadGoal.target.EndsWith(".exe")) continue;
                        HacknetAPCore.Logger.LogDebug($"Replacing file download mission of \"{__instance.email.subject}\"" +
                            " with a GetAdmin goal.\n" +
                            $"Target Comp: {downloadGoal.targetComp}");
                        goals[i] = new GetAdminMission(downloadGoal.targetComp, OS.currentInstance);
                    } else if(goal is FileUploadMission uploadGoal)
                    {
                        if (!uploadGoal.target.EndsWith(".exe")) continue;
                        HacknetAPCore.Logger.LogDebug($"Replacing file upload mission of \"{__instance.email.subject}\"" +
                            " with a GetAdmin goal.\n" +
                            $"Target Comp: {uploadGoal.targetComp}");
                        goals[i] = new GetAdminMission(uploadGoal.targetComp.ip, OS.currentInstance);
                    }
                }
            }

            public const int ENTROPY_EOS_VALUE = 3;
            public const string ENTROPY_EOS_FLAG = "eosPathStarted";
            public const string ENTROPY_EOS_MISSION_PATH = "Content/Missions/Entropy/StartingSet/eosMissions/eosIntroDelayer.xml";
            public const string ENTROPY_NAIX_MISSION_PATH = "Content/Missions/Entropy/ThemeHackTransitionMission.xml";

            [HarmonyPrefix]
            [HarmonyPatch(typeof(EntropyFaction),"addValue")]
            // This patch prevents Entropy level-up missions from being automatically
            // assigned to the player.
            //
            // The missions will instead be added as separate missions in Entropy's
            // mission listing.
            public static bool PreventAutoEntropyMissionsPrefix(EntropyFaction __instance, int value, object os)
            {
                bool isEligibleForEosMission = __instance.playerValue + value == 3;
                bool isEligibleForNaix = __instance.playerValue + value >= 4;
                Console.WriteLine($"eOS: {isEligibleForEosMission} / Naix: {isEligibleForNaix}\n" +
                    $"Current: {__instance.playerValue} / New: {__instance.playerValue + value}");
                if(isEligibleForEosMission)
                {
                    actuallyAddValue();
                    // OS.currentInstance.Flags.AddFlag(ENTROPY_EOS_FLAG);
                    loadMissionIntoEntropy(ENTROPY_EOS_MISSION_PATH);
                    return false;
                } else if(isEligibleForNaix)
                {
                    actuallyAddValue();
                    loadMissionIntoEntropy(ENTROPY_NAIX_MISSION_PATH);
                    return false;
                } else
                {
                    return true;
                }

                static void loadMissionIntoEntropy(string filepath)
                {
                    ActiveMission missionToLoad = MissionLoader.LoadContentMission(filepath);
                    Computer entropyComp = ComputerLookup.FindById("entropy00");
                    MissionListingServer entropyListing = (MissionListingServer)entropyComp.getDaemon(typeof(MissionListingServer));
                    if(entropyListing.missions.Any(m => m.email.subject == missionToLoad.email.subject))
                    {
                        HacknetAPCore.Logger.LogDebug($"loadMissionIntroEntropy attempted to load mission " +
                            missionToLoad.email.subject + "" +
                            ", but it was already loaded.");
                        return;
                    }
                    if(missionToLoad.postingTitle.IsNullOrWhiteSpace())
                    {
                        missionToLoad.postingTitle = missionToLoad.email.subject;
                        missionToLoad.postingBody = "--- PROGRESSION MISSION ---\n" +
                            "You may not be able to collect checks from Entropy missions for a while if " +
                            "this mission is accepted. You have been warned!";
                    }
                    missionToLoad.postingTitle = "#PROGRESSION# - " + missionToLoad.postingTitle;

                    entropyListing.addMisison(missionToLoad, true);

                    sendCriticalMissionEmail(missionToLoad.email.subject);
                }

                static void sendCriticalMissionEmail(string missionName)
                {
                    MailServer.EMailData fauxEmail = new("Entropy MailBot",
                        "Hello, agent. This is to let you know that a critical mission, " +
                        $"\"{missionName}\", is now available for claiming in Entropy.\n\n" +
                        "We recommend you clear all previous contracts before claiming this mission.\n\n" +
                        "This email need not be replied to.",
                        "Critical Mission Alert",
                        []);
                    ActiveMission fauxMission = new([], "NONE", fauxEmail);
                    fauxMission.sendEmail(OS.currentInstance);
                }

                void actuallyAddValue()
                {
                    __instance.playerValue += value;
                    if(__instance.playerValue >= __instance.neededValue && !__instance.playerHasPassedValue)
                    {
                        __instance.playerPassedValue(OS.currentInstance);
                    }
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(EntropyFaction),"playerPassedValue")]
            // Entropy will forcibly shove the player into the Naix mission when the eOS mission
            // is finished. This is probably a failsafe, but, uh... I hate failsafes.
            // So, I'm forcibly gouging out the failsafe. Please forgive me, Matt.
            public static bool PreventLoadingNaixAutomatically(EntropyFaction __instance)
            {
                __instance.playerHasPassedValue = true;
                return false;
            }
        }
    }
}

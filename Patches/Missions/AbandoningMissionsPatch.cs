using System.Linq;

using Hacknet;
using Hacknet.Factions;

using HacknetArchipelago.Managers;

using HarmonyLib;

using Pathfinder.Util;

using static HacknetArchipelago.Static.MissionToFaction;

namespace HacknetArchipelago.Patches.Missions
{
    [HarmonyPatch]
    public class AbandoningMissionsPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MailServer),"attemptCompleteMission")]
        public static bool AllowAbandoningMissions(MailServer __instance, ActiveMission mission,
            ref bool __result)
        {
            var replies = __instance.emailReplyStrings;
            if (!replies.Contains("abandon") && !replies.Contains("quit")) return true;

            if(OS.currentInstance.currentFaction.idName == "lelzSec")
            {
                string playerName = ArchipelagoManager.PlayerName;
                OS.currentInstance.terminal.writeLine("The Polar Star whispers your name...");
                OS.currentInstance.terminal.writeLine("\"" + playerName + "! Just hold on for a little longer...!\"");
                OS.currentInstance.warningFlash();
                OS.currentInstance.beepSound.Play();
                return true;
            }

            var faction = OS.currentInstance.currentFaction;
            var missionFaction = mission.GetFaction();

            if(missionFaction != null) { faction = missionFaction; }

            if(faction.idName == ENTROPY_ID)
            {
                AttemptAddMissionToEntropy(mission);
            } else if(faction.GetType() == typeof(HubFaction))
            {
                AttemptAddMissionToCSEC(mission);
            } else
            {
                HacknetAPCore.Logger.LogError("Both current faction and mission faction are invalid! " +
                    "Falling back to original method.");
                return true;
            }

            OS.currentInstance.branchMissions.Clear();
            OS.currentInstance.currentMission = null;

            string emailBody = "Agent. You've shelved the contract \"" +
                mission.email.subject + "\" for the time being.\n\n" +
                "You may resume it again at any time through your respective Faction's hub server.";

            var jmail = (MailServer)OS.currentInstance.netMap.mailServer.getDaemon(typeof(MailServer));
            jmail.addMail(MailServer.generateEmail("Contract Shelved", emailBody, "Contract Helper"),
                OS.currentInstance.defaultUser.name);

            __result = false; // This prevents missionEnd functions from firing
            return false;
        }

        public const string RESUME_PREFIX = "RESUME - ";
        public const string RESUME_BODY = "Accept this posting to resume this mission.\n\n" +
            "If you are BK'd, might I suggest a game of PointClicker?";

        public static void AttemptAddMissionToEntropy(ActiveMission mission)
        {
            var entropyComp = ComputerLookup.FindById("entropy00");
            var entropy = (MissionListingServer)entropyComp.getDaemon(typeof(MissionListingServer));

            if (entropy.missions.Any(m => m.email.subject == mission.email.subject)) return;

            mission.postingTitle = RESUME_PREFIX + mission.email.subject;
            mission.postingBody = RESUME_BODY;

            entropy.addMisison(mission, true);
        }

        public static void AttemptAddMissionToCSEC(ActiveMission mission)
        {
            var csecComp = ComputerLookup.FindById("mainHub");
            var csec = (MissionHubServer)csecComp.getDaemon(typeof(MissionHubServer));

            if (csec.listingMissions.Any(m => m.Value == mission)) return;

            mission.postingTitle = RESUME_PREFIX + mission.email.subject;
            mission.postingBody = RESUME_BODY;

            csec.addMission(mission, true);
        }

        [HarmonyPrefix]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPatch(typeof(DLCHubServer), "PlayerAttemptCompleteMission")]
        public static bool AllowAbandoningDLCMissions(DLCHubServer __instance,
            DLCHubServer.ClaimableMission mission, bool ForceComplete, ref bool __result)
        {
            ForceComplete = ForceComplete && Settings.forceCompleteEnabled;
            if (ForceComplete) return true;

            if(!__instance.MissionTextResponses.Contains("abandon") &&
                !__instance.MissionTextResponses.Contains("quit"))
            {
                return true;
            }

            AttemptAbandonDLCMission(mission.Mission);

            __result = false;
            return false;
        }

        public static void AttemptAbandonDLCMission(ActiveMission mission)
        {
            var dhsComp = ComputerLookup.FindById("dhs");
            var dhs = (DLCHubServer)dhsComp.getDaemon(typeof(DLCHubServer));
            bool success = false;

            foreach(var m in dhs.ActiveMissions)
            {
                if(m.Mission == mission ||
                    m.AgentClaim == OS.currentInstance.defaultUser.name)
                {
                    m.AgentClaim = null;
                    success = true;
                }
            }

            if(success)
            {
                OS.currentInstance.branchMissions.Clear();
                OS.currentInstance.currentMission = null;
                OS.currentInstance.terminal.writeLine("You've unclaimed this mission - you can come back to it later.");
            } else
            {
                HacknetAPCore.Logger.LogError("Couldn't find mission claimed by player.");
                OS.currentInstance.terminal.writeLine("Unable to abandon mission due to unknown error.");
            }
        }

        private static ActiveMission _lastCompletedMission = null;

        [HarmonyPrefix]
        [HarmonyPriority(Priority.First)]
        [HarmonyPatch(typeof(ActiveMission),"finish")]
        public static void StoreLastCompletedMission(ActiveMission __instance)
        {
            MailServer jmail = (MailServer)OS.currentInstance.netMap.mailServer.getDaemon(typeof(MailServer));

            if(__instance.isComplete(jmail.emailReplyStrings))
            {
                _lastCompletedMission = __instance;
            }
        }

        [HarmonyPrefix]
        [HarmonyPriority(Priority.HigherThanNormal)]
        [HarmonyPatch(typeof(MissionFunctions), "runCommand")]
        // This patch directs addRank and addRankSilent to the respective factions for missions.
        // This allows the player to go "off-course" and go back to earlier missions,
        // without values getting added to the wrong faction. This is technically undefined behavior
        // in the realm of Hacknet.
        public static bool DirectAddValuesToFactions(int value, string name)
        {
            OS os = OS.currentInstance;

            if (name != "addRank" && name != "addRankSilent") return true;
            ActiveMission mission;
            Faction faction = OS.currentInstance.currentFaction;

            if(name == "addRank")
            {
                if (!valuesExist()) return true;

                if(faction.idName == DLC_ID)
                {
                    MissionFunctions.runCommand(value, "addRankFaction:" + faction.idName);
                    return false;
                }

                if (!OS.TestingPassOnly || os.currentFaction != null)
                {
                    string subject = LocaleTerms.Loc("Contract Successful");
                    string body = string.Format(Utils.readEntireFile("Content/LocPost/MissionCompleteEmail.txt"),
                        os.currentFaction.getRank(), os.currentFaction.getMaxRank(), os.currentFaction.name);
                    string sender = os.currentFaction.name + " ReplyBot";
                    string mail = MailServer.generateEmail(subject, body, sender);
                    MailServer mailServer = (MailServer)os.netMap.mailServer.getDaemon(typeof(MailServer));
                    mailServer.addMail(mail, os.defaultUser.name);
                }
                else if (OS.DEBUG_COMMANDS && os.currentFaction == null)
                {
                    os.write("----------");
                    os.write("----------");
                    os.write("ERROR IN FUNCTION 'addRank'");
                    os.write("Player is not assigned to a faction, so rank cannot be added!");
                    os.write("Make sure you have assigned a player a faction with the 'SetFaction' function before using this!");
                    os.write("----------");
                    os.write("----------");
                }

                MissionFunctions.runCommand(value, "addRankFaction:" + faction.idName);
            } else if(name == "addRankSilent")
            {
                if (!valuesExist()) return true;
                MissionFunctions.runCommand(value, "addRankFaction:" + faction.idName);
            }

            bool valuesExist()
            {
                mission = OS.currentInstance.currentMission;
                mission ??= _lastCompletedMission;
                if (mission == null)
                {
                    HacknetAPCore.Logger.LogError("Unable to direct addRank -- no mission was found. " +
                        "Function will run as usual.");
                    return false;
                }

                faction = mission.GetFaction();
                if (faction == null)
                {
                    HacknetAPCore.Logger.LogError("Unable to direct addRank -- no associated faction was found. " +
                        "Function will run as usual.");
                    return false;
                }

                return true;
            }

            return false;
        }
    }
}

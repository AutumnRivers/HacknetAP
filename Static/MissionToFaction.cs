using Hacknet;
using System.Collections.Generic;

namespace HacknetArchipelago.Static
{
    public static class MissionToFaction
    {
        public const string ENTROPY_ID = "entropy";
        public const string DLC_ID = "Bibliotheque";
        public const string CSEC_ID = "hub";

        public static readonly Dictionary<string, string> Missions = new()
        {
            { "Point Clicker", ENTROPY_ID },
            { "The famous counter-hack", ENTROPY_ID },
            { "Back to School", ENTROPY_ID },
            { "Re: Back to School", ENTROPY_ID },
            { "Internal investigations", ENTROPY_ID },
            { "Re: Internal investigations", ENTROPY_ID },
            { "eOS Device Scanning", ENTROPY_ID },
            { "Smash N' Grab", ENTROPY_ID },

            { "The Ricer", DLC_ID },
            { "DDOSer on some critical servers", DLC_ID },
            { "Cleanup", DLC_ID },
            { "It Follows", DLC_ID },
            { "Bean Stalk", DLC_ID },
            { "Expo Grave", DLC_ID },
            { "The Keyboard Life", DLC_ID },
            { "Take Flight", DLC_ID },
            { "Take_Flight Cont.", DLC_ID },
            { "Hermetic Alchemists", DLC_ID },
            { "Neopals", DLC_ID },
            { "Striker's Stash", DLC_ID },
            { "Memory Forensics", DLC_ID },

            { "Rod of Asclepius", CSEC_ID },
            { "Binary Universe(ity)", CSEC_ID },
            { "Ghosting the Vault", CSEC_ID },
            { "Imposters on Death Row", CSEC_ID },
            { "Through the Spyglass", CSEC_ID },
            { "Red Line", CSEC_ID },
            { "Wipe the record clean", CSEC_ID },
            { "A Convincing Application", CSEC_ID },
            { "Unjust Absence", CSEC_ID },
            { "Two ships in the night", CSEC_ID },
            { "Jailbreak", CSEC_ID },
            { "Project Junebug", CSEC_ID }
        };

        public static Faction GetFaction(this ActiveMission mission)
        {
            if(Missions.ContainsKey(mission.email.subject))
            {
                string factionID = Missions[mission.email.subject];
                return OS.currentInstance.allFactions.factions[factionID];
            } else
            {
                return null;
            }
        }
    }
}

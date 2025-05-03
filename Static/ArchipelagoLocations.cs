using System.Collections.Generic;

namespace HacknetArchipelago
{
    internal static class ArchipelagoLocations
    {
        public static readonly Dictionary<string, string> MissionToLocation = new()
        {
            // Intro
            { "First Contact", "Intro -- First Contact" },
            { "Maiden Flight", "Intro -- Maiden Flight" },
            { "Getting some tools together", "Intro -- Getting some tools together" },
            { "Something in return", "Intro -- Something in return" },
            { "Where to from here", "Intro -- Where to from here" },
            { "Confirmation Mission", "Entropy -- Confirmation Mission" },
            { "Welcome", "Entropy -- Welcome" },

            // Entropy
            { "Point Clicker", "Entropy -- PointClicker (Mission)" },
            { "The famous counter-hack", "Entropy -- The famous counter-hack" },
            { "Back to School", "Entropy -- Back to School" },
            { "Internal investigations", "Entropy -- X-C Project" },
            { "Smash N' Grab", "Entropy -- Smash N' Grab" },
            { "eOS Device Scanning", "Entropy -- eOS Device Scanning" },
            { "Aggression must be Punished", "Entropy -- Naix" },

            // /el
            { "gg wp", "Naix -- Deface Nortron Website" },
            { "Hilarious", "Naix -- Nortron Security Mainframe" },
            { "A Victory - Perhaps a turning point", "/el -- Head of Polar Star (Download Files)" },

            // CSEC
            { "CSEC Invitation - Attenuation", "CSEC -- CFC Herbs & Spices" },

            { "Rod of Asclepius", "CSEC -- Investigate a medical record" },
            { "Binary Universe(ity)", "CSEC -- Teach an old dog new tricks" },
            { "Imposters on Death Row", "CSEC -- Remove a Fabricated Death Row Record" },
            { "Red Line", "CSEC -- Check out a suspicious server" },
            { "Wipe the record clean", "CSEC -- Wipe clean an academic record" },
            { "Unjust Absence", "CSEC -- Add a Death Row record for a family member" },
            { "Jailbreak", "CSEC -- Compromise an eOS Device" },

            // CSEC DEC
            { "Ghosting the Vault", "CSEC -- Locate or Create Decryption Software" },
            { "Through the Spyglass", "CSEC -- Track an Encrypted File" },
            { "A Convincing Application", "CSEC -- Help an aspiring writer" },
            { "Two ships in the night", "CSEC -- Decrypt a secure transmission" },

            // Project Junebug
            { "Project Junebug", "CSEC -- Project Junebug" },

            // CSEC Bit Intro
            { "Bit's disappearance Investigation", "CSEC -- Investigate a CSEC member's disappearance" },

            // Bit / Finale
            { "Bit -- Foundation", "Bit -- Foundation" },
            { "Bit -- Substantiation", "Bit -- Substantiation" },
            { "Bit -- Investigation", "Bit -- Investigation" },
            { "Bit -- Propagation", "Bit -- Propagation" },
            { "Bit -- Vindication", "Bit -- Vindication" },
            { "Bit -- Termination", "Bit -- Termination" },

            // Labyrinths Missions
            // Kaguya trials mission has to be detected another way...
            { "The Ricer", "Labyrinths -- The Ricer" },
            { "DDOSer on some critical servers", "Labyrinths -- DDOSer on some critical servers" },
            { "The Hermetic Alchemists", "Labyrinths -- Hermetic Alchemists" },
            { "Memory Forensics", "Labyrinths -- Memory Forensics" },
            { "Striker's Stash", "Labyrinths -- Striker's Stash" },
            { "Cleanup", "Labyrinths -- Cleanup/It Follows" },
            { "It Follows", "Labyrinths -- Cleanup/It Follows" },
            { "Neopals", "Labyrinths -- Neopals" },
            { "Bean Stalk", "Labyrinths -- Bean Stalk/Expo Grave/The Keyboard Life" },
            { "Expo Grave", "Labyrinths -- Bean Stalk/Expo Grave/The Keyboard Life" },
            { "The Keyboard Life", "Labyrinths -- Bean Stalk/Expo Grave/The Keyboard Life" },
            { "Take Flight", "Labyrinths -- Take Flight" },
            { "Take_Flight Cont.", "Labyrinths -- Take Flight Cont." }
        };

        public static readonly Dictionary<string, List<string>> RequiredItemsForLocation = new()
        {
            // Intro and Finale don't need this
            // Entropy theoretically also doesn't need this
            { "eOS Device Scanning", ["eosDeviceScan"] },
            { "Smash N' Grab", ["eosDeviceScan"] },
            { "Aggression must be Punished", ["eosDeviceScan"] },

            { "Ghosting the Vault", ["DEC Suite"] },
            { "Through the Spyglass", ["DEC Suite"] },
            { "A Convincing Application", ["DEC Suite"] },
            { "Two ships in the night", ["DEC Suite"] },

            { "Project Junebug", ["DEC Suite", "KBTPortTest"] },
        };

        public static bool HasItemsForLocation(string locationName)
        {
            if (!RequiredItemsForLocation.ContainsKey(locationName)) return true;

            var requiredItems = RequiredItemsForLocation[locationName];
            bool hasRequiredItems = true;

            foreach(var reqItem in requiredItems)
            {
                if (!hasRequiredItems) return false;

                hasRequiredItems = HacknetAPCore._localInventory.ContainsKey(reqItem);
            }

            return hasRequiredItems;
        }

        public static readonly Dictionary<string, string> NodeIDToLocation = new();

        public static readonly Dictionary<string, string> FlagToLocation = new()
        {
            { "KaguyaTrialComplete", "Labyrinths -- Kaguya Trials" },
            { "clock_run_Unlocked", "Achievement -- TRUE ULTIMATE POWER!" },
            { "dlc_complete", "Watched Labyrinths Credits" },
            { "kill_tutorial_Unlocked", "Achievement -- Quickdraw" },
            { "pointclicker_basic_Unlocked", "Achievement -- PointClicker" },
            { "pointclicker_expert_Unlocked", "Achievement -- You better not have clicked for those..." },
            { "themeswitch_run_Unlocked", "Achievement -- Makeover!" },
            { "trace_close_Unlocked", "Achievement -- To the Wire" },
            { "progress_entropy_Unlocked", "Achievement -- Join Entropy" }
        };

        public static readonly Dictionary<string, string> CommandToLocation = new()
        {
            { "deActivateAircraftStatusOverlay", "Labyrinths -- Altitude Loss" }
        };
    }
}

using HacknetArchipelago.Managers;
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
            { "Re: Internal investigations", "Entropy -- X-C Project" },
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
            { "Take_Flight Cont.", "Labyrinths -- Take Flight Cont." },
            { "Silence Psylance", "CSEC -- Subvert Psylance Investigation" }
        };

        public static readonly Dictionary<string, List<string>> RequiredItemsForLocation = new()
        {
            // Intro and Finale don't need this
            { "eOS Device Scanning", ["eosDeviceScan"] },
            { "Smash N' Grab", ["eosDeviceScan"] },
            { "Aggression must be Punished", [
                "eosDeviceScan", "SSHCrack",
                "WebServerWorm", "SMTPOverflow"
                ] },

            { "Ghosting the Vault", ["DEC Suite"] },
            { "Through the Spyglass", ["DEC Suite"] },
            { "A Convincing Application", ["DEC Suite"] },
            { "Two ships in the night", ["DEC Suite"] },

            { "Project Junebug", ["DEC Suite", "KBTPortTest"] },

            { "Bit -- Foundation", [
                "DEC Suite", "KBTPortTest",
                "SMTPOverflow", "WebServerWorm",
                "SSHCrack", "SQL_MemCorrupt", "FTPBounce"
                ] }
        };

        public static readonly Dictionary<string, int> RequiredRAMUpgradesForLocation = new()
        {
            { "Bit -- Foundation", 650 },
            { "Project Junebug", 450 },
            { "The Kaguya Trials", 550 },
            { "Take Flight", 600 },
            { "Take Flight Cont.", 600 }
        };

        public static bool HasItemsForLocation(string locationName)
        {
            if (!RequiredItemsForLocation.ContainsKey(locationName)) return true;

            var requiredItems = RequiredItemsForLocation[locationName];
            bool hasRequiredItems = true;

            foreach(var reqItem in requiredItems)
            {
                if (!hasRequiredItems) return false;

                if(ArchipelagoItems.ExecutableNames.Contains(reqItem))
                {
                    hasRequiredItems = ArchipelagoItems.PlayerHasExecutable(reqItem);
                } else
                {
                    var alternateItem = reqItem;
                    if (reqItem == "FTPBounce") alternateItem = "FTPSprint";
                    hasRequiredItems = InventoryManager.PlayerCollectedItem(reqItem) ||
                        InventoryManager.PlayerCollectedItem(alternateItem);
                }
            }

            return hasRequiredItems;
        }

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
            { "progress_entropy_Unlocked", "Achievement -- Join Entropy" },
            { "secret_path_complete_Unlocked", "Achievement -- Rude//el Sec Champion" }
        };

        public static readonly Dictionary<string, string> CommandToLocation = new()
        {
            { "deActivateAircraftStatusOverlay", "Labyrinths -- Altitude Loss" }
        };

        public static readonly List<string> UpgradeIndexToLocation =
        [
            "PointClicker -- Click Me!",
            "PointClicker -- Autoclicker v1",
            "PointClicker -- Autoclicker v2",
            "PointClicker -- Pointereiellion",
            "PointClicker -- Upgrade 4",
            "PointClicker -- Upgrade 5",
            "PointClicker -- Upgrade 6",
            "PointClicker -- Upgrade 7",
            "PointClicker -- Upgrade 8",
            "PointClicker -- Upgrade 9",
            "PointClicker -- Upgrade 10",
            "PointClicker -- Upgrade 11",
            "PointClicker -- Upgrade 12",
            "PointClicker -- Upgrade 13",
            "PointClicker -- Upgrade 14",
            "PointClicker -- Upgrade 15",
            "PointClicker -- Upgrade 16",
            "PointClicker -- Upgrade 17",
            "PointClicker -- Upgrade 18",
            "PointClicker -- Upgrade 19",
            "PointClicker -- Upgrade 20",
            "PointClicker -- Upgrade 21",
            "PointClicker -- Upgrade 22",
            "PointClicker -- Upgrade 23",
            "PointClicker -- Upgrade 24",
            "PointClicker -- Upgrade 25",
            "PointClicker -- Upgrade 26",
            "PointClicker -- Upgrade 27",
            "PointClicker -- Upgrade 28",
            "PointClicker -- Upgrade 29",
            "PointClicker -- Upgrade 30",
            "PointClicker -- Upgrade 31",
            "PointClicker -- Upgrade 32",
            "PointClicker -- Upgrade 33",
            "PointClicker -- Upgrade 34",
            "PointClicker -- Upgrade 35",
            "PointClicker -- Upgrade 36",
            "PointClicker -- Upgrade 37",
            "PointClicker -- Upgrade 38",
            "PointClicker -- Upgrade 39",
            "PointClicker -- Upgrade 40",
            "PointClicker -- Upgrade 41",
            "PointClicker -- Upgrade 42",
            "PointClicker -- Upgrade 43",
            "PointClicker -- Upgrade 44",
            "PointClicker -- Upgrade 45",
            "PointClicker -- Upgrade 46",
            "PointClicker -- Upgrade 47",
            "PointClicker -- Upgrade 48",
            "PointClicker -- Upgrade 49",
            "PointClicker -- Upgrade 50"
        ];

        public static readonly Dictionary<string, string> NodeToLocation = new()
        {
            { "playerComp", "Intro -- Player's PC" },
            { "archiIRC", "Intro -- Archipelago IRC" },
            { "portcrack01", "Intro -- Viper-Battlestation" },
            { "viperScanEarlyGame", "Intro -- Entropy Asset Cache" },
            { "bitMission00", "Intro -- Bitwise Test PC" },
            { "bitMission01", "Intro -- P. Anderson's Bedroom PC" },
            { "bitMission02", "Intro -- Entropy test Server" },
            { "entropy01.1", "Entropy -- Slash-Bot News Network" },
            
            { "entropy01", "Entropy -- Entropy Asset Server" },
            { "milburgHigh", "Entropy -- Milburg High IT Office" },
            { "pointclicker", "Entropy -- PointClicker (Admin Access)" },
            { "ppMarketing", "Entropy -- PP Marketing Inc." }, // hehe, marketing
            { "xcTablet", "Entropy -- X-C Project Tablet#001//RESEARCH" },
            { "eosIntroComp", "Entropy -- Jason's PowerBook Plus" },
            { "eosIntroPhone", "Entropy -- Jason's ePhone 4S" },
            { "eosAdded1Comp", "Entropy -- JDel Home PC" },
            { "eosAddedPhone", "Entropy -- Jacob's ePhone 4" },
            
            { "naixGateway", "Entropy -- Naix Root Gateway" },
            { "themeHackComp", "Entropy -- Proxy_Node-X22" },
            { "themeHackTransComp", "Entropy -- Proxy_Node-X04" },
            
            { "nortronWebServer", "Naix -- Nortron Security Web Server" },
            { "nortronInternalServices", "Naix -- Nortron Internal Services Server" },
            { "nortronMainframe", "Naix -- Nortron Mainframe" },
            
            { "lelzSecHub", "/el -- /el Message Board" },
            { "secuLockDrive", "/el -- COME AT ME /EL's Secure SecuLock Drive" },
            { "SecuLockHome", "/el -- Stormrider" },
            
            { "polarSnake", "/el -- Shrine of the Polar Star" },
            { "psTrial01", "/el -- Polar Star - Trial of Patience" },
            { "psTrial02", "/el -- Polar Star - Trial of Haste" },
            { "psTrial03", "/el -- Polar Star - Trial of Diligence" },
            { "psTrial03b", "/el -- Tail of Diligence" },
            { "psTrial04", "/el -- Polar Star - Trial of Focus" },
            { "polarSnakeDest", "/el -- Head of the Polar Star (Admin Access)" },
            { "clockServer", "/el -- Timekeeper's Vault" },
            
            { "kfcWebServer", "CSEC -- www.cfc.com" },
            { "kfcMainframe", "CSEC -- CFC Corporate Mainframe" },
            { "kfcRecordsRepo", "CSEC -- CFC Records Repository" },
            
            { "hubCrossroads", "CSEC -- CSEC Crossroads Server" },
            { "hubPubDrop", "CSEC -- CSEC Public Drop Server" },
            { "hubEosMisison", "CSEC -- Sal_Home_Workstation" },
            { "salEosPhone", "CSEC -- Elanor Helleran's ePhone 4S" },
            { "honeypot01", "CSEC -- CCC Hacksquad Filedump" },
            { "producerComp", "CSEC -- Jason's LackBook Pro" },
            { "deathRow", "CSEC -- Death Row Records Database" },
            { "academic", "CSEC -- International Academic Database" },
            { "medical", "CSEC -- Universal Medical" },
            
            { "decSoftMainframe", "CSEC -- DEC Solutions Mainframe" },
            { "decSoftWeb", "CSEC -- DEC Solutions Web Server" },
            { "jscottHome", "CSEC -- Joseph Scott's Battlestation" },
            { "macrosoftWorkhorse04", "CSEC -- Macrosoft Workhorse Server 04" },
            { "mainHub", "CSEC -- CSEC (Contracts Server)" },
            { "mainHubAssets", "CSEC Assets Server" },
            
            { "pacemakerSW_BE", "CSEC -- Eidolon Soft Production Server" },
            { "kellisHW", "CSEC -- Kellis Biotech Client Services" },
            { "kellisHW_BE", "CSEC -- Kellis Biotech Production Asset Server" },
            { "pacemaker01", "CSEC -- KBT-PM 2.44 REG#10811" },
            
            { "bitDropBox", "CSEC -- Bitwise Drop Server" },
            { "bitRelay01", "CSEC -- Bitwise Relay 01" },
            
            { "BitWorkServer", "Bit -- Bitwise Repo Base" },
            { "EnTechOutsiderRepo", "Bit -- EnTech External Contractor Relay Server" },
            { "EnTechWeb", "Bit -- EnTech Web Server" },
            { "EnTechPrometheus", "Bit -- En_Prometheus" },
            { "EnTechRomulus", "Bit -- En_Romulus" },
            { "EnTechWSCore", "Bit -- EnWorkstationCore" },
            { "EnWS04", "Bit -- EnTech Workstation _008" },
            { "EnTechMainframe", "Bit -- EnTech_Zeus" },
            { "EnTechOfflineBackup", "Bit -- EnTech_Offline_Cycling_Backup" },
            { "miscMacrosoftStorage", "Bit -- Macrosoft Storage Server" },
            { "creditsComp", "Post-Game -- Credits Server" },
            
            { "dKaguya1", "Kaguya Trials -- Kaguya Sprint Trial" },
            { "dcentralTest2", "Kaguya Trials -- Kaguya Push Trial" },
            { "dKaguyaTrialIntro", "Kaguya Trials -- Kaguya Source" }, // Kaguya Trials -- Kaguya Source
            { "dhsDrop", "Labyrinths -- Bibliotheque DropServer" },
            
            { "dRicer", "Labyrinths -- Ricer PC" },
            { "dDdoserComp", "Labyrinths -- r00t_Tek Battlestation" },
            { "dDdoserHomeComp", "Labyrinths -- L. Shaffer's NetBook" },
            
            { "dMF_1_Misc", "Labyrinths -- iodependency~Atlas" },
            { "dMF_2_Passthru", "Labyrinths -- Snackintosh_PASSTHRU" },
            { "dMF_2_Snackintosh", "Labyrinths -- Snackintosh_Proxy" },
            { "dMF_2_Lihotas", "Labyrinths -- Lihota Productions" },
            { "dMF_2_Secret", "Labyrinths -- Raven Dataworks" },
            
            { "dhaEntry", "Labyrinths -- School of the Hermetic Alchemists" },
            { "dhaComp2", "Labyrinths -- HA_Solve" },
            { "dhaComp3", "Labyrinths -- HA_Rebis" },
            { "dhaAdminPhone", "Labyrinths -- Nate's ePhone 4S" },
            { "dhaAdminHome", "Labyrinths -- Nate Wesson Home" },
            { "dhaAdminSecret", "Labyrinths -- Nate Wesson_STOR-DRIVE(tm)" },
            { "dhaComp1", "Labyrinths -- HA_Coagula" },
            
            { "dAttackSource", "Labyrinths -- Striker Cache" },
            { "dAttackTarget", "Labyrinths -- Striker Proxy" },
            { "dAttackHome", "Labyrinths -- Striker_Battlestation" },
            
            { "dPets_Home", "Labyrinths -- Neopals Homepage" },
            { "dPets_MF", "Labyrinths -- Neopals_Mainframe" },
            { "dPets_Auth", "Labyrinths -- Neopals_Authentication" },
            { "dPets_VC", "Labyrinths -- Neopals_VersionControl" },
            { "dPets_Engineer1", "Labyrinths -- Thomas_Office" },
            { "dPets_Engineer2", "Labyrinths -- Ash-ALIENGEAR13" },
            { "dPets_Engineer3", "Labyrinths -- Tiff Doehan_PersonalPowerbook" },
            { "dPets_Engineer3_Phone", "Labyrinths -- Tiff's ePhone 7" },
            
            { "dal_lax", "Labyrinths -- LAX_Pacific_Server" },
            { "dpa_nethub", "Labyrinths -- PacificAir_Network_Hub" },
            { "dpa_whitelist", "Labyrinths -- PacificAir_Whitelist_Authenticator" },
            { "dpae_psy_1", "Labyrinths -- Faith Morello's Laptop" },
            { "dpae_psy_2", "Labyrinths -- Vito McMichael's Laptop" },
            { "dpae_airline_misc", "Labyrinths -- Mark Robertson's Office Computer" },
            { "dpae_airline_mailLink", "Labyrinths -- Kim Burnaby's Office Computer" },
            { "dpae_airline_home", "Labyrinths -- Yasu Arai's eBook Touch" },
            { "dpa_bookings", "Labyrinths -- PacificAir_BookingsMainframe" },
            
            { "dpa2_start", "Labyrinths -- Pacific_ATC_RoutingHub" },
            { "dpa2_target", "Labyrinths -- Pacific_ATC_Skylink" },
            { "dpa2_whitelist", "Labyrinths -- Pacific_ATC_WhitelistAuthenticator" },
            
            { "dair_crash", "Labyrinths -- PA_747_0022 Flight Computer" },
            { "dair_secondary", "Labyrinths -- PA_747_0018 Flight Computer" },
            
            { "dCredits", "Labyrinths -- Kaguya_Projects" },
            { "dKagGate", "Labyrinths -- Kaguya_Gateway" },
            { "dCreditsChat", "Labyrinths -- Labyrinths_DevChat" },
            
            { "dCoelgateway", "Labyrinths -- Coel__Gateway" },
            { "dNaixSecretLink", "Naix -- Pellium Box" },
            { "dPsyArchives", "CSEC -- Psylance Internal Archives" },
            { "dPsyInternal", "CSEC -- Psylance Internal Services" },
            
            { "dGibson", "Labyrinths -- The Gibson (Veteran)" }
        };
    }
}
